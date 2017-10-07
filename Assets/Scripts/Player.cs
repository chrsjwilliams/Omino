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
    public Polyomino selectedPiece { get; private set; }
    private Polyomino pieceToBePlayed;
    public bool placementAvailable { get; private set; }
    [SerializeField]
    private Vector3 handSpacing;
    [SerializeField]
    private Vector3 handOrigin;
    [SerializeField]
    private int startingHandSize;
    public Transform uiArea { get; private set; }

    // Use this for initialization
    public void Init(Color[] colorScheme, int posOffset)
    {
        playerNum = posOffset + 1;

        _activeTilePrimaryColors[0] = colorScheme[0];
        _activeTilePrimaryColors[1] = colorScheme[1];

        _activeTileSecondaryColors[0] = colorScheme[2];
        _activeTileSecondaryColors[1] = colorScheme[3];

        uiArea = Services.GameScene.uiAreas[playerNum - 1];

        hand = new List<Polyomino>();

        InitializeDeck();
        DrawPieces(startingHandSize);

        //for now just allow placement always
        placementAvailable = true;
    }

    // Update is called once per frame
    void Update()
    {

    }

    #region DECK FUNCTIONS
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
        OrganizeHand();
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
            hand[i].Reposition(handOrigin + handSpacing * i);
        }
    }
    #endregion

    public void OnPieceSelected(Polyomino piece)
    {
        if (selectedPiece != null) CancelSelectedPiece();
        selectedPiece = piece;
        hand.Remove(piece);
        OrganizeHand();
    }

    public void OnPiecePlaced()
    {
        selectedPiece = null;
    }

    public void CancelSelectedPiece()
    {
        hand.Add(selectedPiece);
        OrganizeHand();
        selectedPiece = null;
    }
}
