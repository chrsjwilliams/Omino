using UnityEngine;
using System.Collections;

public class DeathAnimation : Task
{
    private Polyomino piece;
    private float timeElapsed;
    private float duration;
    private Color initialColor;
    private Color targetColor;

    public DeathAnimation(Polyomino piece_)
    {
        piece = piece_;
    }

    protected override void Init()
    {
        timeElapsed = 0;
        duration = Polyomino.deathAnimDur;
        initialColor = piece.tiles[0].sr.color;
        targetColor = new Color(initialColor.r, initialColor.g, initialColor.b, 0);
        foreach(Tile tile in piece.tiles)
        {
            tile.highlightSr.color = new Color(1,1,1,0);
            tile.highlightSr.sortingOrder += 2;
        }
    }

    internal override void Update()
    {
        timeElapsed += Time.deltaTime;
        foreach (Tile tile in piece.tiles)
        {
            tile.SetColor(Color.Lerp(initialColor, targetColor,
                EasingEquations.Easing.QuadEaseOut(timeElapsed / duration)));
            if (timeElapsed <= duration / 2)
            {
                tile.SetHighlightAlpha(Mathf.Lerp(0, 1,
                    EasingEquations.Easing.QuadEaseOut(timeElapsed / (duration / 2))));
            }
            else
            {
                tile.SetHighlightAlpha(Mathf.Lerp(1, 0,
                    EasingEquations.Easing.QuadEaseIn(
                        (timeElapsed - (duration / 2)) / (duration / 2))));
            }
        }
        //if (timeElapsed <= duration / 2)
        //{
            piece.ScaleHolder(Vector3.Lerp(Vector3.one, Polyomino.deathAnimScaleUp * Vector3.one,
                EasingEquations.Easing.QuadEaseOut(timeElapsed / duration)));
        //}
        //else
        //{
        //    piece.ScaleHolder(Vector3.Lerp(Polyomino.deathAnimScaleUp * Vector3.one, Vector3.one,
        //        EasingEquations.Easing.QuadEaseIn((timeElapsed - (duration / 2)) / (duration / 2))));
        //}

        if (timeElapsed >= duration) SetStatus(TaskStatus.Success);
    }
}
