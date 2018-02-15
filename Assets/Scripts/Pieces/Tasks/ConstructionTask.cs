using UnityEngine;
using System.Collections;

public class ConstructionTask : Task
{
    private float timeElapsed;
    private const float duration = 0.15f;
    private const float staggerTime = 0.2f;
    private const float settleDuration = 0.15f;
    private Polyomino piece;
    private GameObject[] blocks;
    private bool[] blocksCreated;
    private Vector3[] blockStartLocations;
    private Vector3[] blockSettleTargets;
    private Vector3[] blockTargets;
    private bool hadSplashDamage;
    private Vector3 startScale = 0.47f * Vector3.one;
    private Vector3 targetScale = Vector3.one;
    private Vector3 settleTargetOffset = new Vector3(0.35f, 0.35f);
    private bool[] soundPlayed;

    public ConstructionTask(Polyomino piece_)
    {
        piece = piece_;
        hadSplashDamage = piece.owner.splashDamage;
    }

    protected override void Init()
    {
        timeElapsed = 0;
        blocks = new GameObject[piece.tiles.Count];
        blocksCreated = new bool[blocks.Length];
        soundPlayed = new bool[blocks.Length];
        blockStartLocations = new Vector3[blocks.Length];
        blockSettleTargets = new Vector3[blocks.Length];
        blockTargets = new Vector3[blocks.Length];
        for (int i = 0; i < blocks.Length; i++)
        {
            blocksCreated[i] = false;
            Vector3 screenStartLoc = Services.GameManager.MainCamera.ScreenToWorldPoint(
                Services.UIManager.resourceSlotZones[piece.owner.playerNum - 1].transform.position);
            blockStartLocations[i] = new Vector3(screenStartLoc.x, screenStartLoc.y, 0);
            Coord targetCoord = piece.tiles[i].coord;

            blockTargets[i] = new Vector3(targetCoord.x, targetCoord.y, 0);
            if (piece.owner.playerNum == 1)
            {
                blockSettleTargets[i] = blockTargets[i] + settleTargetOffset;
            }
            else
            {
                blockSettleTargets[i] = blockTargets[i] - settleTargetOffset;
            }
        }
    }

    internal override void Update()
    {
        timeElapsed += Time.deltaTime;

        for (int i = 0; i < blocks.Length; i++)
        {
            if (timeElapsed >= staggerTime * i)
            {
                if (!blocksCreated[i])
                {
                    blocks[i] = GameObject.Instantiate(Services.Prefabs.Block,
                        Services.GameScene.transform);
                    blocks[i].transform.position = blockStartLocations[i];
                    blocks[i].GetComponent<SpriteRenderer>().color = piece.owner.ColorScheme[0];
                    blocksCreated[i] = true;
                    blocks[i].transform.localScale = startScale;
                }
                else
                {
                    if ((timeElapsed - (i * staggerTime)) < duration)
                    {
                        blocks[i].transform.position = Vector3.Lerp(
                            blockStartLocations[i],
                            blockSettleTargets[i],
                            EasingEquations.Easing.Linear(
                                (timeElapsed - (i * staggerTime)) / duration));
                        blocks[i].transform.localScale = Vector3.Lerp(
                            startScale, targetScale, EasingEquations.Easing.QuadEaseOut(
                                (timeElapsed - (i * staggerTime)) / duration));
                    }
                    else if ((timeElapsed - (i*staggerTime) - duration) < settleDuration)
                    {
                        if (!soundPlayed[i] && (timeElapsed - (i * staggerTime) - duration) > settleDuration/2)
                        {

                            soundPlayed[i] = true;
                            GameObject.Instantiate(Services.Prefabs.DustCloud,
                                blockTargets[i], Quaternion.identity);
                            if (piece is Destructor)
                            {
                                GameObject.Instantiate(Services.Prefabs.FireBurst,
                                    blockTargets[i], Quaternion.identity);
                                if (hadSplashDamage)
                                {
                                    Coord tileCoord = new Coord(
                                        Mathf.RoundToInt(blockTargets[i].x),
                                        Mathf.RoundToInt(blockTargets[i].y));
                                    foreach (Coord dir in Coord.Directions())
                                    {
                                        Coord adjCoord = tileCoord.Add(dir);
                                        Vector3 adjPos = new Vector3(adjCoord.x, adjCoord.y, 0);
                                        GameObject.Instantiate(Services.Prefabs.FireBurst,
                                            adjPos, Quaternion.identity);
                                    }
                                }
                            }
                        }
                        blocks[i].transform.position = Vector3.Lerp(
                            blockSettleTargets[i],
                            blockTargets[i],
                            EasingEquations.Easing.QuadEaseIn(
                                (timeElapsed - (i * staggerTime) - duration) / settleDuration));
                    }
                    else if (blocks[i] != null)
                    {
                        GameObject.Destroy(blocks[i]);
                        Services.AudioManager.CreateTempAudio(
                            Services.Clips.BlockConstructed, 1);
                        if (piece.tiles[i] != null) piece.tiles[i].SetAlpha(1f);
                        
                    }
                }
            }
        }

        if (timeElapsed >= duration + settleDuration + (staggerTime * (blocks.Length - 1)))
            SetStatus(TaskStatus.Success);
    }
}
