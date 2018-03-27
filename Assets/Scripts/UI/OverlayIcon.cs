﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OverlayIcon : MonoBehaviour {

    private Polyomino piece;
    private Tile tile;
    private Image image;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (piece != null) UpdatePositionAndScale();
        else UpdatePositionForTile();
	}

    public void Init(Tile tile_)
    {
        tile = tile_;
        UpdatePositionForTile();
        image = GetComponent<Image>();
        if (tile.pieceParent.owner != null)
        {
            Quaternion rot = tile.pieceParent.owner.playerNum == 1 ?
                    Quaternion.Euler(0, 0, -90) : Quaternion.Euler(0, 0, 90);
            transform.localRotation = rot;
        }
    }

    public void Init(Polyomino piece_)
    {
        piece = piece_;
        UpdatePositionAndScale();
        image = GetComponent<Image>();
        if (piece.owner != null)
        {
            Quaternion rot = piece.owner.playerNum == 1 ?
                    Quaternion.Euler(0, 0, -90) : Quaternion.Euler(0, 0, 90);
            transform.localRotation = rot;
        }
    }

    private void UpdatePositionForTile()
    {
        Vector3 uiPos = Camera.main.WorldToScreenPoint(tile.transform.position);
        uiPos = new Vector3(uiPos.x, uiPos.y, 0);
        transform.position = uiPos;
    }

    private void UpdatePositionAndScale()
    {
        Vector3 uiPos = Camera.main.WorldToScreenPoint(piece.GetCenterpoint());
        uiPos = new Vector3(uiPos.x, uiPos.y, 0);
        transform.position = uiPos;
        transform.localScale = piece.holder.transform.localScale;
    }

    public void Remove()
    {
        Destroy(gameObject);
    }

    public void SetStatus(bool status)
    {
        image.enabled = status;
    }

    public void SetSprite(Sprite sprite_)
    {
        image.sprite = sprite_;
    }

    public void SetSize(int size)
    {
        GetComponent<RectTransform>().sizeDelta = new Vector2(size, size);
    }
}
