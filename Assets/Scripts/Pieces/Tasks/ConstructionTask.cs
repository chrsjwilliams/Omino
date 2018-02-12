using UnityEngine;
using System.Collections;

public class ConstructionTask : Task
{
    private float timeElapsed;
    private const float duration = 0.2f;
    private const float staggerTime = 0.1f;
    private Polyomino piece;
    private GameObject[] blocks;
    private bool[] blocksCreated;
    private Vector3[] blockStartLocations;
    private Vector3[] blockTargets;
    private bool hadSplashDamage;

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
        blockStartLocations = new Vector3[blocks.Length];
        blockTargets = new Vector3[blocks.Length];
        for (int i = 0; i < blocks.Length; i++)
        {
            blocksCreated[i] = false;
            Vector3 screenStartLoc = Services.GameManager.MainCamera.ScreenToWorldPoint(
                Services.UIManager.resourceSlotZones[piece.owner.playerNum - 1].transform.position);
            blockStartLocations[i] = new Vector3(screenStartLoc.x, screenStartLoc.y, 0);
            Coord targetCoord = piece.tiles[i].coord;
            blockTargets[i] = new Vector3(targetCoord.x, targetCoord.y, 0);
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
                    Services.AudioManager.CreateTempAudio(Services.Clips.BlockConstructed, 1);
                }
                else
                {
                    if ((timeElapsed - (i * staggerTime)) < duration)
                    {
                        blocks[i].transform.position = Vector3.Lerp(
                            blockStartLocations[i],
                            blockTargets[i],
                            EasingEquations.Easing.QuadEaseOut(
                                timeElapsed - (i * staggerTime)) / duration);
                    }
                    else if (blocks[i] != null)
                    {
                        GameObject.Destroy(blocks[i]);
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
                }
            }
        }

        if (timeElapsed >= duration + (staggerTime * (blocks.Length - 1)))
            SetStatus(TaskStatus.Success);
    }
}
