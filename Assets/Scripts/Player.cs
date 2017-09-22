using UnityEngine;

//  Currently Not Used for RTS_Blokus
public class Player : MonoBehaviour
{
    /*
     *      This class will hold color info and total
     *      number of tiles controlled by that color.
     * 
     */ 
    [SerializeField] private Color _activeTilePrimaryColor;
    public Color ActiveTilePrimaryColor
    {
        get { return _activeTilePrimaryColor; }
    }

    [SerializeField] private Color _activeTileSecondaryColor;
    public Color ActiveTileSecondaryColor
    {
        get { return _activeTileSecondaryColor; }
    }

    [SerializeField] private int _tilesControlled;
    public int TotalTilesControlled
    {
        get { return _tilesControlled; }
    }

    [SerializeField] private int _strongholdsBuilt;
    public int TotalStrongholdsBuilt
    {
        get { return _strongholdsBuilt; }
    }

    // Use this for initialization
    void Start ()
    {
        
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}
}
