using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private bool _isActive;
    public bool isActive
    {
        get { return _isActive; }
    }

    public Coord coord { get; private set; }
    public BoxCollider boxCol { get; private set; }
    public Material material { get; set; }
    public Polyomino occupyingPiece { get; private set; }

    public void Init(Coord coord_, int yOffset)
    {
        coord = coord_;
        transform.position = new Vector3(coord.x, coord.y, 0);
        material = GetComponent<MeshRenderer>().material;
        if ((coord.x + coord.y) % 2 == 0)
        {
            material.color = Services.GameManager.MapColorScheme[0];
        }
        else
        {
            material.color = Services.GameManager.MapColorScheme[1];
        }
        boxCol = GetComponent<BoxCollider>();
    }

    public void Init(Coord coord_, Color color1, Color color2)
    {
        coord = coord_;
        transform.position = new Vector3(coord.x, coord.y, 0);
        material = GetComponent<MeshRenderer>().material;
        if ((coord.x + coord.y) % 2 == 0)
        {
            material.color = color1;
        }
        else
        {
            material.color = color2;
        }
        boxCol = GetComponent<BoxCollider>();
    }

    public void Create()
    {
        material = GetComponent<MeshRenderer>().material;
        if ((coord.x + coord.y) % 2 == 0)
        {
            material.color = Services.GameManager.MapColorScheme[0];
        }
        else
        {
            material.color = Services.GameManager.MapColorScheme[1];
        }
        boxCol = GetComponent<BoxCollider>();
    }

    public void ActivateTile(Player player)
    {
        _isActive = true;
        if ((coord.x + coord.y) % 2 == 0)
        {
            material.color = player.ActiveTilePrimaryColors[0];
        }
        else
        {
            material.color = player.ActiveTilePrimaryColors[1];
        }
    }

    public void DeactivateTile()
    {
        _isActive = false;
        if ((coord.x + coord.y) % 2 == 0)
        {
            material.color = Services.GameManager.MapColorScheme[0];
        }
        else
        {
            material.color = Services.GameManager.MapColorScheme[1];
        }
    }

    public void SetOccupyingPiece(Polyomino piece)
    {
        occupyingPiece = piece;
        Debug.Log(piece.holder.name);
    }

    public bool IsOccupied()
    {
        return occupyingPiece == null;
    }

    public Polyomino OccupiedBy()
    {
        if(IsOccupied())
        {
            return occupyingPiece;
        }
        else
        {
            return null;
        }
    }
}
