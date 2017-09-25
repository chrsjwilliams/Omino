using UnityEngine;

//  Currently Not Used for RTS_Blokus
public class Player : MonoBehaviour
{
    /*
     *      This class will hold color info and total
     *      number of tiles controlled by that color.
     * 
     */
    public int playerNum { get; private set; }
    public CursorPosition cursorPos { get; private set; }
    public Transform cursor { get; private set; }

    public Coord Coord { get; private set; }

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

    // Use this for initialization
    public void Init(Color[] colorScheme, int posOffset)
    {
        playerNum = posOffset + 1;

        _activeTilePrimaryColors[0] = colorScheme[0];
        _activeTilePrimaryColors[1] = colorScheme[1];

        _activeTileSecondaryColors[0] = colorScheme[2];
        _activeTileSecondaryColors[1] = colorScheme[3];

        cursorPos = GetComponent<CursorPosition>();
        cursorPos.SetPosition(posOffset * (Services.MapManager.MapWidth - 1), posOffset * (Services.MapManager.MapLength - 1));

        Coord = new Coord(posOffset * (Services.MapManager.MapWidth - 1), posOffset * (Services.MapManager.MapLength - 1));

        cursor = transform.GetChild(0);

        Services.GameEventManager.Register<ButtonPressed>(OnButtonPressed);
    }

    private void OnDestroy()
    {
        Services.GameEventManager.Unregister<ButtonPressed>(OnButtonPressed);
    }

    public void MovePlayerLeftStick(IntVector2 axis)
    {
        cursorPos.LeftStickMotion(axis);
        UpdatePlayerCoord();
    }

    public void MovePlayerDPad(IntVector2 axis)
    {
        cursorPos.DPadMotion(axis);
        UpdatePlayerCoord();
    }

    private void UpdatePlayerCoord()
    {
        Coord = new Coord(cursorPos.X, cursorPos.Y);
    }

    private Pentominoes test;

    void OnButtonPressed(ButtonPressed e)
    {
        switch (e.button)
        {
            case "A":
                if (cursor.childCount == 0) CreatePolyomino();
                else PlacePolyomino();
                break;
            case "B":
                break;
            case "X":
                break;
            case "Y":
                break;
            default:
                break;
        }
    }

    void CreatePolyomino()
    {
        test = new Pentominoes();
        test.Create(0, this);
    }

    void PlacePolyomino()
    {
        Services.GameEventManager.Fire(new PlacePieceEvent(this));
    }

    // Update is called once per frame
    void Update ()
    {

	}
}
