using UnityEngine;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
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

    public List<Polyomino> deck { get; private set; }
    public List<Polyomino> hand { get; private set; }
    private Polyomino selectedPiece;
    private Polyomino hoveredPiece;
    private Polyomino pieceToBePlayed;
    private bool placeMode;
    private bool placementAvailable;
    [SerializeField]
    private Vector3 handSpacing;
    [SerializeField]
    private int startingHandSize;

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

        //InitializeDeck();
        //DrawPieces(startingHandSize);
    }

    private void OnDestroy()
    {
        Services.GameEventManager.Unregister<ButtonPressed>(OnButtonPressed);
    }

    void InitializeDeck()
    {
        deck = new List<Polyomino>();
        for (int numBlocks = 3; numBlocks <= 5; numBlocks++)
        {
            int numTypes = Polyomino.pieceTypes[numBlocks];
            for (int index = 0; index < numTypes; index++)
            {
                deck.Add(new Polyomino(numBlocks, index, this));
            }
        }
    }

    public void DrawPieces(int numPiecesToDraw)
    {
        for (int i = 0; i < numPiecesToDraw; i++)
        {
            DrawPiece();
        }
    }

    void DrawPiece()
    {
        if (deck.Count == 0) InitializeDeck();
        Polyomino piece = GetRandomPieceFromDeck();
        deck.Remove(piece);
        hand.Add(piece);
        piece.MakePhysicalPiece();
    }

    Polyomino GetRandomPieceFromDeck()
    {
        int index = Random.Range(0, deck.Count);
        return deck[index];
    }

    void OrganizeHand()
    {
        for (int i = 0; i < hand.Count; i++)
        {
            hand[i].Reposition(handSpacing * i);
        }
    }

    void SelectPiece()
    {
        selectedPiece = hoveredPiece;
        placeMode = true;
        hand.Remove(selectedPiece);
        OrganizeHand();
    }

    void TryToPlayPiece()
    {
        if (selectedPiece.IsPlacementLegal() && placementAvailable) PlaySelectedPiece();
    }

    void MoveToSelectMode()
    {
        placeMode = false;
        if (selectedPiece != null)
        {
            hand.Add(selectedPiece);
            OrganizeHand();
        }
        //move cursor over to ui
    }

    void PlaySelectedPiece()
    {
        selectedPiece.PlaceAtCurrentLocation();
        selectedPiece = null;
        MoveToSelectMode();
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

    void OnButtonPressed(ButtonPressed e)
    {
        if (e.playerNum == playerNum)
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
    }

    void CreatePolyomino()
    {
        Polyomino newPolyomino = new Polyomino(4, 0, this);
        cursorPos.heldPiece = newPolyomino;
        newPolyomino.MakePhysicalPiece();
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
