using UnityEngine;

//  Currently Not Used for RTS_Blokus
public class Player : MonoBehaviour
{
    /*
     *      This class will hold color info and total
     *      number of tiles controlled by that color.
     * 
     */
    [SerializeField] private int _xCoord;
    public int XCoord
    {
        get { return _xCoord; }
    }

    [SerializeField] private int _yCoord;
    public int YCoord
    {
        get { return _yCoord; }
    }

    [SerializeField] private Color[] _activeTilePrimaryColors = new Color[2];
    public Color[] ActiveTilePrimaryColors
    {
        get { return _activeTilePrimaryColors; }
    }

    [SerializeField] private Color[] _activeTileSecondaryColors = new Color[2];
    public Color[] ActiveTileSecondaryColors
    {
        get { return _activeTileSecondaryColors; }
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

    private CursorPosition position;

    // Use this for initialization
    public void Init(Color[] colorScheme, int posOffset)
    {
        _activeTilePrimaryColors[0] = colorScheme[0];
        _activeTilePrimaryColors[1] = colorScheme[1];

        _activeTileSecondaryColors[0] = colorScheme[2];
        _activeTileSecondaryColors[1] = colorScheme[3];

        position = GetComponent<CursorPosition>();
        position.SetPosition(posOffset * (Services.MapManager.MapWidth - 1), posOffset * (Services.MapManager.MapLength - 1));
    }

    public void MovePlayerLeftStick(IntVector2 axis)
    {
        position.LeftStickMotion(axis);
    }

    public void MovePlayerDPad(IntVector2 axis)
    {
        position.DPadMotion(axis);
    }

    // Update is called once per frame
    void Update ()
    {
		
	}
}
