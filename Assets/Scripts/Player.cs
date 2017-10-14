using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int playerNum { get; private set; }

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

    [SerializeField]
    private bool viewingHand;
    public List<Polyomino> deck { get; private set; }
    public List<Polyomino> hand { get; private set; }
    public Polyomino selectedPiece { get; private set; }
    private Polyomino pieceToBePlayed;
    public bool placementAvailable { get; private set; }
    [SerializeField]
    private Vector3 handSpacing;
    [SerializeField]
    private Vector3 handOffset;
    [SerializeField]
    private int startingHandSize;
    [SerializeField]
    private int maxHandSize;
    [SerializeField]
    private int piecesPerHandColumn;
    public RectTransform handZone { get; private set; }
    [SerializeField]
    private float factoryPlayRateIncrement;
    private int factoryCount;
    [SerializeField]
    private float baseDrawPeriod;
    private float drawRate
    {
        get
        {
            return (1 / baseDrawPeriod) * (1 + factoryCount * factoryPlayRateIncrement);
        }
    }
    private float drawMeter;
    [SerializeField]
    private float mineDrawRateIncrement;
    private int mineCount;
    [SerializeField]
    private float basePlayPeriod;
    private float playRate
    {
        get
        {
            return (1 / basePlayPeriod) * (1 + mineCount * mineDrawRateIncrement);
        }
    }
    private float playMeter;

    // Use this for initialization
    public void Init(Color[] colorScheme, int posOffset)
    {
        viewingHand = true;
        playerNum = posOffset + 1;

        _activeTilePrimaryColors[0] = colorScheme[0];
        _activeTilePrimaryColors[1] = colorScheme[1];

        handZone = Services.UIManager.handZones[playerNum - 1];

        hand = new List<Polyomino>();

        InitializeDeck();
        DrawPieces(startingHandSize);


        //for now just allow placement always
        placementAvailable = true;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateDrawMeter();
        UpdatePlayMeter();
    }

    void UpdateDrawMeter()
    {
        drawMeter += drawRate * Time.deltaTime;
        Services.UIManager.UpdateDrawMeter(playerNum, drawMeter);
        if (drawMeter >= 1)
        {
            DrawPieces(1);
            drawMeter -= 1;
        }
    }

    void UpdatePlayMeter()
    {
        playMeter += playRate * Time.deltaTime;
        if (playMeter >= 1)
        {
            placementAvailable = true;
            playMeter -= 1;
        }
        Services.UIManager.UpdatePlayMeter(playerNum, playMeter, placementAvailable);
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
        int handSpace = maxHandSize - hand.Count;
        if (selectedPiece != null) handSpace -= 1;
        int drawsAllowed = Mathf.Min(handSpace, numPiecesToDraw);
        for (int i = 0; i < drawsAllowed; i++)
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
        piece.MakePhysicalPiece(viewingHand);
        OrganizeHand();
    }

    Polyomino GetRandomPieceFromDeck()
    {
        int index = Random.Range(0, deck.Count);
        return deck[index];
    }

    void OrganizeHand()
    {
        Vector3 offset = handOffset;
        float spacingMultiplier = 1;
        if(playerNum == 2)
        {
            spacingMultiplier = -1;
            offset = new Vector3(-handOffset.x, handOffset.y, handOffset.z);
        }
        offset += Services.GameManager.MainCamera.ScreenToWorldPoint(handZone.transform.position);
        offset = new Vector3(offset.x, offset.y, 0);
        for (int i = 0; i < hand.Count; i++)
        {
            Vector3 newPos = new Vector3(
                handSpacing.x * (i / piecesPerHandColumn) * spacingMultiplier,
                handSpacing.y * (i % piecesPerHandColumn), 0) + offset;
            hand[i].Reposition(newPos);
        }
    }
    #endregion

    public void ToggleHandZoneView(bool viewPieces)
    {
        viewingHand = viewPieces;
        foreach(Polyomino piece in hand)
        {
            piece.SetVisible(viewingHand);
        }
    }

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
        placementAvailable = false;
    }

    public void CancelSelectedPiece()
    {
        hand.Add(selectedPiece);
        OrganizeHand();
        selectedPiece = null;
    }
}
