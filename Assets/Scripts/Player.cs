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
    private List<Polyomino> deck;
    [SerializeField]
    private int deckClumpCount;
    private List<List<Polyomino>> deckClumped;
    private List<Polyomino> hand;
    public int handCount { get { return hand.Count; } }
    private List<Blueprint> blueprints;
    public Polyomino selectedPiece { get; private set; }
    public bool placementAvailable
    {
        get { return placementAvailable_; }
        private set
        {
            placementAvailable_ = value;
            SetHandStatus();
        }
    }
    private bool placementAvailable_;
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
    //[SerializeField]
    //private float factoryPlayRateIncrement;
    //private int factoryCount;
    //[SerializeField]
    //private float baseDrawPeriod;
    //private float drawRate
    //{
    //    get
    //    {
    //        return (1 / baseDrawPeriod) * (1 + mineCount * mineDrawRateIncrement);
    //    }
    //}
    //private float drawMeter;
    //[SerializeField]
    //private float mineDrawRateIncrement;
    //private int mineCount;
    //[SerializeField]
    //private float basePlayPeriod;
    //private float playRate
    //{
    //    get
    //    {
    //        return (1 / basePlayPeriod) * (1 + factoryCount * factoryPlayRateIncrement);
    //    }
    //}
    //private float playMeter;
    public bool gameOver { get; private set; }
    private UITabs uiTabs;
    private List<Polyomino> boardPieces;
    [SerializeField]
    private int startingResources;
    private int resources_;
    public int resources {
        get
        {
            return resources_;
        }
        private set
        {
            resources_ = value;
            Services.UIManager.UpdateResourceCount(resources_, this);
        }
    }
    public float resourceGainIncrementFactor { get; private set; }
    public float drawRateFactor { get; private set; }

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
        boardPieces = new List<Polyomino>();

        InitializeDeck();
        DrawPieces(startingHandSize);
        Factory factory = new Factory(this);
        AddBluePrint(factory);

        Mine mine = new Mine(this);
        AddBluePrint(mine);

        BombFactory bombFactory = new BombFactory(this);
        AddBluePrint(bombFactory);

        Coord basePos;
        if (playerNum == 1) basePos = new Coord(1, 1);
        else
        {
            basePos = new Coord(
                Services.MapManager.MapWidth - 2, 
                Services.MapManager.MapLength - 2);
        }
        Services.MapManager.CreateMainBase(this, basePos);
        //for now just allow placement always
        placementAvailable = true;
        resources = startingResources;
        resourceGainIncrementFactor = 1;
        drawRateFactor = 1;
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameOver)
        {
            //UpdateDrawMeter();
            //UpdatePlayMeter();

            if (Input.GetKeyDown(KeyCode.Space) && selectedPiece != null)
            {
                selectedPiece.Rotate();
            }

            for (int i = 0; i < boardPieces.Count; i++)
            {
                boardPieces[i].Update();
            }

            for (int i = 0; i < hand.Count; i++)
            {
                hand[i].SetAffordableStatus(this);
            }
        }

    }

    //void UpdateDrawMeter()
    //{
    //    drawMeter += drawRate * Time.deltaTime;
    //    Services.UIManager.UpdateDrawMeter(playerNum, drawMeter);
    //    if (drawMeter >= 1)
    //    {
    //        DrawPieces(1);
    //        drawMeter -= 1;
    //    }
    //}

    //void UpdatePlayMeter()
    //{
    //    playMeter += playRate * Time.deltaTime;
    //    if (playMeter >= 1)
    //    {
    //        placementAvailable = true;
    //        playMeter -= 1;
    //        Services.AudioManager.CreateTempAudio(Services.Clips.PlayAvailable[playerNum -1], 
    //            0.5f);
    //    }
    //    Services.UIManager.UpdatePlayMeter(playerNum, playMeter, placementAvailable);
    //}

    void SetHandStatus()
    {
        MakeAllPiecesGlow(placementAvailable);
        for (int i = 0; i < hand.Count; i++)
        {
            hand[i].SetPieceState(placementAvailable);
        }
        Services.UIManager.SetGreyOutBox(playerNum, !placementAvailable);
    }

    public void InitializeUITabs(UITabs tabs)
    {
        uiTabs = tabs;
    }

    void MakeAllPiecesGlow(bool makeGlow)
    {
        foreach (Polyomino piece in hand)
        {
            piece.SetGlowState(makeGlow);
        }
    }

    #region DECK FUNCTIONS
    void InitializeDeck()
    {
        //deck = new List<Polyomino>();
        deckClumped = new List<List<Polyomino>>();
        List<Polyomino> destructors = new List<Polyomino>();
        List<Polyomino> nonDestructors = new List<Polyomino>();
        for (int numBlocks = 3; numBlocks <= 5; numBlocks++)
        {
            int numTypes = Polyomino.pieceTypes[numBlocks];
            for (int index = 0; index < numTypes; index++)
            {
                if (numBlocks < 5) destructors.Add(new Destructor(numBlocks, index, this, false));
                else nonDestructors.Add(new Polyomino(numBlocks, index, this));
            }
        }
        for (int i = 0; i < deckClumpCount; i++)
        {
            deckClumped.Add(new List<Polyomino>());
        }
        for (int i = 0; i < destructors.Count; i++)
        {
            Polyomino destructorToAdd = destructors[Random.Range(0, destructors.Count)];
            deckClumped[i % deckClumpCount].Add(destructorToAdd);
            destructors.Remove(destructorToAdd);
        }
        for (int i = 0; i < nonDestructors.Count; i++)
        {
            Polyomino nonDestructorToAdd = nonDestructors[Random.Range(0, nonDestructors.Count)];
            deckClumped[i % deckClumpCount].Add(nonDestructorToAdd);
            nonDestructors.Remove(nonDestructorToAdd);
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
        SetHandStatus();
    }

    public void DrawPieces(int numPiecesToDraw, Vector3 startPos)
    {
        DrawPieces(numPiecesToDraw, startPos, false);
    }

    public void DrawPieces(int numPiecesToDraw, Vector3 startPos, bool onlyDestructors)
    {
        int handSpace = maxHandSize - hand.Count;
        if (selectedPiece != null) handSpace -= 1;
        int drawsAllowed = Mathf.Min(handSpace, numPiecesToDraw);
        for (int i = 0; i < drawsAllowed; i++)
        {
            DrawPiece(startPos, onlyDestructors);
        }
        SetHandStatus();
    }

    void DrawPiece()
    {
        Polyomino piece = GetRandomPieceFromDeck(false);
        piece.MakePhysicalPiece(viewingHand);
        AddPieceToHand(piece);
    }

    void DrawPiece(Vector3 startPos, bool onlyDestructors)
    {
        Polyomino piece = GetRandomPieceFromDeck(onlyDestructors);
        Task drawTask = new DrawTask(piece, startPos);
        drawTask.Then(new ActionTask(SetHandStatus));
        Services.GeneralTaskManager.Do(drawTask);
    }

    public void AddPieceToHand(Polyomino piece)
    {
        hand.Add(piece);
        piece.SetVisible(viewingHand);
        OrganizeHand(hand);
    }

    Polyomino GetRandomPieceFromDeck(bool onlyDestructors)
    {
        Polyomino piece;
        if (!onlyDestructors)
        {
            if (deckClumped.Count == 0) InitializeDeck();
            int index = Random.Range(0, deckClumped[0].Count);
            piece = deckClumped[0][index];
            deckClumped[0].Remove(piece);
            if (deckClumped[0].Count == 0) deckClumped.Remove(deckClumped[0]);
        }
        else
        {
            List<Polyomino> destructors = new List<Polyomino>();
            for (int numBlocks = 3; numBlocks <= 4; numBlocks++)
            {
                int numTypes = Polyomino.pieceTypes[numBlocks];
                for (int index = 0; index < numTypes; index++)
                {
                    destructors.Add(new Destructor(numBlocks, index, this, false));
                }
            }
            piece = destructors[Random.Range(0,destructors.Count)];
        }
        return piece;
    }

    public void AddBluePrint(Blueprint blueprint)
    {
        blueprints.Add(blueprint);
        blueprint.MakePhysicalPiece(viewingHand);
        OrganizeHand(blueprints);
    }

    void OrganizeHand<T>(List<T> heldpieces) where T :Polyomino
    {
        for (int i = 0; i < heldpieces.Count; i++)
        {
            Vector3 newPos = GetHandPosition(i);
            heldpieces[i].Reposition(newPos);
        }
    }

    public Vector3 GetHandPosition(int handIndex)
    {
        Vector3 offset = handOffset;
        float spacingMultiplier = 1;
        if (playerNum == 2)
        {
            spacingMultiplier = -1;
            offset = new Vector3(-handOffset.x, handOffset.y, handOffset.z);
        }
        offset += Services.GameManager.MainCamera.ScreenToWorldPoint(handZone.transform.position);
        offset = new Vector3(offset.x, offset.y, 0);
        Vector3 newPos = new Vector3(
                handSpacing.x * (handIndex / piecesPerHandColumn) * spacingMultiplier,
                handSpacing.y * (handIndex % piecesPerHandColumn), 0) + offset;
        return newPos;
    }
    #endregion

    public void ToggleHandZoneView(bool viewPieces)
    {
        viewingHand = viewPieces;
        if (!viewingHand) Services.UIManager.SetGreyOutBox(playerNum, false);
        else Services.UIManager.SetGreyOutBox(playerNum, !placementAvailable);
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


    public void OnPiecePlaced(Polyomino piece)
    {
        BuildingType blueprintType = piece.buildingType;
        if (!(piece is Blueprint))
        {
            //placementAvailable = false;
            resources -= piece.cost;
        }
        else
        {
            AddBluePrint(System.Activator.CreateInstance(
                piece.GetType(), new Object[] { this }) as Blueprint);
            uiTabs.ToggleHandZoneView(true);
        }
        selectedPiece = null;
        piece.SetGlowState(false);
        boardPieces.Add(piece);
        if (Services.MapManager.CheckForWin(piece)) Services.GameScene.GameWin(this);
    }

    public void OnPieceRemoved(Polyomino piece)
    {
        boardPieces.Remove(piece);
    }

    public void CancelSelectedPiece()
    {
        hand.Add(selectedPiece);
        SetHandStatus();
        OrganizeHand(hand);
        selectedPiece = null;
    }

    public void CancelSelectedBlueprint()
    {
        blueprints.Add((Blueprint)selectedPiece);
        selectedPiece.SetGlowState(false);
        OrganizeHand(blueprints);
        selectedPiece = null;
    }

    //public void ToggleMineCount(int newMineCount)
    //{
    //    mineCount += newMineCount;
    //}

    //public void ToggleFactoryCount(int newFactoryCount)
    //{
    //    factoryCount += newFactoryCount;
    //}

    public void OnGameOver()
    {
        gameOver = true;
    }

    public void AddPieceToHand(SuperDestructorResource resource)
    {
        Destructor newPiece = new Destructor(resource.units, resource.index, this, true);
        resource.Remove();
        newPiece.MakePhysicalPiece(viewingHand);
        AddPieceToHand(newPiece);
    }

    public void GainResources(int numResources)
    {
        resources += numResources;
    }

    public void AugmentDrawRateFactor(float factorChangeIncrement)
    {
        drawRateFactor += factorChangeIncrement;
    }

    public void AugmentResourceGainIncrementFactor(float factorChangeIncrement)
    {
        resourceGainIncrementFactor += factorChangeIncrement;
    }

    public void GainOwnership(Structure structure)
    {
        boardPieces.Add(structure);
    }

    public void LoseOwnership(Structure structure)
    {
        boardPieces.Remove(structure);
    }
}
