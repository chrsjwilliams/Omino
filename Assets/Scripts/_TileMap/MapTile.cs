using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTile : MonoBehaviour {

    public Sprite gridSpriteEmpty;
    public Sprite gridSpriteFilled;
    public Sprite gridSpriteLegalHovered;
    public Sprite gridSpriteIllegalHovered;
    public Coord coord { get; private set; }
    public Polyomino occupyingPiece { get; private set; }
    public Blueprint occupyingBlueprint { get; private set; }
    public SpriteRenderer bgSr;
    public SpriteRenderer sr;

    public void Init(Coord coord_)
    {
        coord = coord_;
        transform.position = new Vector3(coord.x, coord.y, 0);
        switch (Services.GameManager.mode)
        {
            case TitleSceneScript.GameMode.HyperSOLO:
            case TitleSceneScript.GameMode.HyperVS:
                sr.color = Services.GameManager.MapColorScheme[1];
                break;
            default:
                sr.color = Services.GameManager.MapColorScheme[0];
                break;
        }
        SetMapSprite();
    }

    public void SetMapSprite()
    {
        SetMapSprite(occupyingPiece != null);
    }

    public void SetMapSprite(bool status)
    {
        if (status) sr.sprite = gridSpriteFilled;
        else sr.sprite = gridSpriteEmpty;
    }

    public void SetMapSpriteHovered(bool legal)
    {
        if (legal) sr.sprite = gridSpriteLegalHovered;
        else sr.sprite = gridSpriteIllegalHovered;
    }

    public void SetOccupyingPiece(Polyomino piece)
    {
        occupyingPiece = piece;
        SetMapSprite();
    }

    public void SetOccupyingBlueprint(Blueprint blueprint)
    {
        occupyingBlueprint = blueprint;
    }

    public bool IsOccupied()
    {
        return occupyingPiece != null;
    }

    public bool PartOfExistingBlueprint()
    {
        return occupyingBlueprint != null;
    }

    public Polyomino OccupiedBy()
    {
        if (IsOccupied())
        {
            return occupyingPiece;
        }
        else
        {
            return null;
        }
    }

    public void SetBackgroundColor(Color color)
    {
        bgSr.color = color;
    }

    public void SetBackgroundAlpha(float alpha)
    {
        Color color = bgSr.color;
        bgSr.color = new Color(color.r, color.g, color.b, alpha);
    }
}
