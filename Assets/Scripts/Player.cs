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
    public List<Blueprint> blueprints { get; private set; }
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
    public int factoryCount { get; private set; }
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
    public int mineCount { get; private set; }
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
        blueprints = new List<Blueprint>();

        InitializeDeck();
        DrawPieces(startingHandSize);
        Blueprint factory = new Blueprint(BuildingType.FACTORY, this);
        AddBluePrint(factory);

        Blueprint mine = new Blueprint(BuildingType.MINE, this);
        AddBluePrint(mine);


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
        OrganizeHand(hand);
    }

    Polyomino GetRandomPieceFromDeck()
    {
        int index = Random.Range(0, deck.Count);
        return deck[index];
    }

    public void AddBluePrint(Blueprint blueprint)
    {
        blueprints.Add(blueprint);
        blueprint.MakePhysicalPiece(viewingHand);
        OrganizeHand(blueprints);
    }

    void OrganizeHand<T>(List<T> heldpieces) where T :Polyomino
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
        for (int i = 0; i < heldpieces.Count; i++)
        {
            Vector3 newPos = new Vector3(
                handSpacing.x * (i / piecesPerHandColumn) * spacingMultiplier,
                handSpacing.y * (i % piecesPerHandColumn), 0) + offset;
            heldpieces[i].Reposition(newPos);
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

        foreach(Blueprint blueprint in blueprints)
        {
            blueprint.SetVisible(!viewingHand);
        }
    }

    public void OnPieceSelected(Polyomino piece)
    {
        if (selectedPiece != null) CancelSelectedPiece();
        selectedPiece = piece;
        if (piece.buildingType == BuildingType.NONE) hand.Remove(piece);
        else blueprints.Remove((Blueprint)piece);

        OrganizeHand(hand);
    }


    public void OnPiecePlaced()
    {
        BuildingType blueprintType = selectedPiece.buildingType;
        if (selectedPiece.buildingType == BuildingType.NONE) placementAvailable = false;
        else AddBluePrint(new Blueprint(blueprintType, this));
        selectedPiece = null;
    }

    public void CancelSelectedPiece()
    {
        hand.Add(selectedPiece);
        OrganizeHand(hand);
        selectedPiece = null;
    }

    public void CancelSelectedBlueprint()
    {
        blueprints.Add((Blueprint)selectedPiece);
        OrganizeHand(blueprints);
        selectedPiece = null;
    }

    public void ToggleMineCount(int newMineCount)
    {
        mineCount += newMineCount;
    }

    public void ToggleFactoryCount (int newFacotryCount)
    {
        factoryCount += newFacotryCount;
    }
}
