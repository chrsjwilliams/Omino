using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private bool _isActive;
    public bool isActive
    {   get { return _isActive; }
        set
        {
            _isActive = value;
            if (_isActive)
            {
                //  Need to change this based on which player activates a tile
                if ((coord.x + coord.y) % 2 == 0)
                {
                    material.color = Services.GameManager.PlayerColorScheme[0];
                }
                else
                {
                    material.color = Services.GameManager.PlayerColorScheme[0];
                }
            }
            else
            {
                if ((coord.x + coord.y) % 2 == 0)
                {
                    material.color = Services.GameManager.MapColorScheme[0];
                }
                else
                {
                    material.color = Services.GameManager.MapColorScheme[1];
                }
            }
        }
    }
    public Coord coord { get; private set; }
    public BoxCollider boxCol { get; private set; }
    public Material material { get; set; }

    public void Init(Coord coord_)
    {
        coord = coord_;
        transform.position = new Vector3(coord.x, 0, coord.y);
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
        transform.position = new Vector3(coord.x, 0, coord.y);
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
}
