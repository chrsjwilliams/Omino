using UnityEngine;
using System.Collections;

public class DeathAnimation : Task
{
    private Polyomino piece;
    private float timeElapsed;
    private float duration;
    private Color initialColor;
    private Color targetColor;
    private Vector3[] startPositions;
    private Vector3[] targetPositions;
    private const float explosionDistanceMin = 1.5f;
    private const float explosionDistanceMax = 5f;

    public DeathAnimation(Polyomino piece_)
    {
        piece = piece_;
    }

    protected override void Init()
    {
        timeElapsed = 0;
        duration = Polyomino.deathAnimDur;
        initialColor = piece.tiles[0].mainSr.color;
        targetColor = new Color(initialColor.r, initialColor.g, initialColor.b, 0);
        targetPositions = new Vector3[piece.tiles.Count];
        startPositions = new Vector3[piece.tiles.Count];
        for (int i = 0; i < piece.tiles.Count; i++)
        {
            piece.tiles[i].highlightSr.color = new Color(1, 1, 1, 0);
            piece.tiles[i].SortOnSelection(true);
            Vector3 tilePos = piece.tiles[i].transform.position;
            float explosionDist = Random.Range(explosionDistanceMin, explosionDistanceMax);
            float angleOffset = Random.Range(5f, 10f);
            angleOffset *= Random.Range(0, 2) == 0 ? 1 : -1;
            Vector3 tileOffset = (tilePos - piece.GetCenterpoint()).normalized;
            float explosionAngle = (Mathf.Atan2(tileOffset.y, tileOffset.x) * Mathf.Rad2Deg)
                + angleOffset;
            //Vector3 offset = explosionDist * new Vector3(Mathf.Sin(explosionAngle),
            //    Mathf.Cos(explosionAngle), 0);
            Vector3 offset = explosionDist 
                * new Vector3(Mathf.Sin(explosionAngle), Mathf.Cos(explosionAngle), 0);
            targetPositions[i] = tilePos + offset;
            startPositions[i] = tilePos;
        }
    }

    internal override void Update()
    {
        timeElapsed += Time.deltaTime;
        for (int i = 0; i < piece.tiles.Count; i++)
        {
            piece.tiles[i].SetColor(Color.Lerp(initialColor, targetColor,
                EasingEquations.Easing.QuadEaseIn(timeElapsed / duration)));
            piece.tiles[i].transform.position = Vector3.Lerp(startPositions[i],
                targetPositions[i], 
                EasingEquations.Easing.QuadEaseOut(timeElapsed / duration));
        //if (timeElapsed <= duration / 2)
        //{
        //    tile.SetHighlightAlpha(Mathf.Lerp(0, 1,
        //        EasingEquations.Easing.QuadEaseOut(timeElapsed / (duration / 2))));
        //}
        //else
        //{
        //    tile.SetHighlightAlpha(Mathf.Lerp(1, 0,
        //        EasingEquations.Easing.QuadEaseIn(
        //            (timeElapsed - (duration / 2)) / (duration / 2))));
        //}
    }
        //if (timeElapsed <= duration / 2)
        //{
            //piece.ScaleHolder(Vector3.Lerp(Vector3.one, Polyomino.deathAnimScaleUp * Vector3.one,
            //    EasingEquations.Easing.QuadEaseOut(timeElapsed / duration)));
        //}
        //else
        //{
        //    piece.ScaleHolder(Vector3.Lerp(Polyomino.deathAnimScaleUp * Vector3.one, Vector3.one,
        //        EasingEquations.Easing.QuadEaseIn((timeElapsed - (duration / 2)) / (duration / 2))));
        //}

        if (timeElapsed >= duration) SetStatus(TaskStatus.Success);
    }
}
