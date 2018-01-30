using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    public int playerNum { get; private set; }

    public Coord Coord { get; private set; }

    [SerializeField] private Color[] colorScheme = new Color[2];
    public Color[] ColorScheme
    {
        get { return colorScheme; }
    }

    [SerializeField]
    protected bool viewingHand;
    private List<Polyomino> normalDeck;
    private List<Destructor> destructorDeck;
    [SerializeField]
    protected int deckClumpCount;
    protected List<List<Polyomino>> deckClumped;
    protected List<Polyomino> hand;
    public int handCount { get { return hand.Count; } }
    private List<Vector3> handTargetPositions;
    protected List<Blueprint> blueprints;
    public Polyomino selectedPiece { get; private set; }
    private int selectedPieceHandPos;
    [SerializeField]
    protected Vector3 handSpacing;
    [SerializeField]
    protected Vector3 handOffset;
    [SerializeField]
    protected int startingHandSize;
    [SerializeField]
    protected int maxHandSize;
    [SerializeField]
    protected int piecesPerHandColumn;
    public RectTransform handZone { get; private set; }
    public Base mainBase;
    public bool gameOver { get; private set; }
    protected UITabs uiTabs;
    public List<Polyomino> boardPieces { get; protected set; }
    [SerializeField]
    protected int startingResources;
    [SerializeField]
    protected int baseMaxResources;
    protected int maxResources;
    protected int resources_;
    public int resources {
        get
        {
            return resources_;
        }
        private set
        {
            resources_ = value;
            Services.UIManager.UpdateResourceCount(resources_, maxResources, this);
        }
    }
    public float resourceGainFactor { get; protected set; }
    public float drawRateFactor { get; protected set; }
    public bool autoFortify { get; protected set; }
    public bool shieldedPieces { get; protected set; }
    protected bool biggerBricks;
    protected bool biggerBombs;
    public bool splashDamage { get; protected set; }
    private float resourceGainRate;
    private float normalDrawRate;
    private float destructorDrawRate;
    private float resourceMeterFillAmt;
    private float normalDrawMeterFillAmt;
    private float destructorDrawMeterFillAmt;
    [SerializeField]
    protected int resourcesPerTick;


    // Use this for initialization
    public virtual  void Init(Color[] playerColorScheme, int posOffset)
    {
        viewingHand = true;
        playerNum = posOffset + 1;

        colorScheme[0] = playerColorScheme[0];
        colorScheme[1] = playerColorScheme[1];

        handZone = Services.UIManager.handZones[playerNum - 1];

        hand = new List<Polyomino>();
        blueprints = new List<Blueprint>();
        boardPieces = new List<Polyomino>();

        InitializeNormalDeck();
        InitializeDestructorDeck();
        DrawPieces(startingHandSize);
        OrganizeHand(hand, true);
        if (Services.GameManager.usingBlueprints)
        {
            Factory factory = new Factory(this);
            AddBluePrint(factory);

            Mine mine = new Mine(this);
            AddBluePrint(mine);

            BombFactory bombFactory = new BombFactory(this);
            AddBluePrint(bombFactory);
        }

        Coord basePos;
        if (playerNum == 1) basePos = new Coord(1, 1);
        else
        {
            basePos = new Coord(
                Services.MapManager.MapWidth - 2, 
                Services.MapManager.MapLength - 2);
        }
        Services.MapManager.CreateMainBase(this, basePos);
        maxResources = baseMaxResources;
        resources = startingResources;
        resourceGainFactor = 1;
        drawRateFactor = 1;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (!gameOver)
        {
            UpdateMeters();

            if (Input.GetKeyDown(KeyCode.Space) && selectedPiece != null)
            {
                selectedPiece.Rotate();
            }

            for (int i = 0; i < boardPieces.Count; i++)
            {
                //if(boardPieces[i].connected)
                    boardPieces[i].Update();
            }

            for (int i = 0; i < hand.Count; i++)
            {
                hand[i].SetAffordableStatus(this);
                hand[i].ApproachHandPosition(handTargetPositions[i]);
            }
        }

    }

    void UpdateMeters()
    {
        normalDrawMeterFillAmt += normalDrawRate * drawRateFactor * Time.deltaTime;
        destructorDrawMeterFillAmt += destructorDrawRate * drawRateFactor * Time.deltaTime;
        resourceMeterFillAmt += resourceGainRate * resourceGainFactor * Time.deltaTime;
        Services.UIManager.UpdateDrawMeters(playerNum, normalDrawMeterFillAmt, 
            destructorDrawMeterFillAmt);
        if (normalDrawMeterFillAmt >= 1)
        {
            Vector3 rawDrawPos = Services.GameManager.MainCamera.ScreenToWorldPoint(
                Services.UIManager.factoryBlueprintLocations[playerNum - 1].position);
            DrawPieces(1, new Vector3(rawDrawPos.x,rawDrawPos.y, 0));
            normalDrawMeterFillAmt -= 1;
        }
        if (destructorDrawMeterFillAmt >= 1)
        {
            Vector3 rawDrawPos = Services.GameManager.MainCamera.ScreenToWorldPoint(
                Services.UIManager.bombFactoryBlueprintLocations[playerNum - 1].position);
            DrawPieces(1, new Vector3(rawDrawPos.x, rawDrawPos.y, 0), true);
            destructorDrawMeterFillAmt -= 1;
        }
        if (resourceMeterFillAmt >= 1)
        {
            GainResources(resourcesPerTick);
            resourceMeterFillAmt -= 1;
        }
    }

    public void InitializeUITabs(UITabs tabs)
    {
        uiTabs = tabs;
    }

    #region DECK FUNCTIONS
    void InitializeNormalDeck()
    {
        normalDeck = new List<Polyomino>();
        for (int numBlocks = 4; numBlocks <= 4; numBlocks++)
        {
            int pieceSize = numBlocks;
            if (biggerBricks)
            {
                pieceSize += 1;
            }
            int numTypes = Polyomino.pieceTypes[pieceSize];
            for (int index = 0; index < numTypes; index++)
            {
                normalDeck.Add(new Polyomino(pieceSize, index, this));
            }
        }
    }

    void InitializeDestructorDeck()
    {
        destructorDeck = new List<Destructor>();
        for (int numBlocks = 3; numBlocks <= 3; numBlocks++)
        {
            int pieceSize = numBlocks;
            if (biggerBombs)
            {
                pieceSize += 1;
            }
            int numTypes = Polyomino.pieceTypes[pieceSize];
            for (int index = 0; index < numTypes; index++)
            {
                destructorDeck.Add(new Destructor(pieceSize, index, this, false));
            }
        }
    }

    public void DrawPieces(int numPiecesToDraw)
    {
        int handSpace = maxHandSize - hand.Count;
        if (selectedPiece != null) handSpace -= 1;
        int numPiecesToBurn = Mathf.Max(numPiecesToDraw - handSpace, 0);
        for (int i = numPiecesToBurn - 1; i >= 0; i--)
        {
            hand[i].BurnFromHand();
            hand.RemoveAt(i);
        }
        for (int i = 0; i < numPiecesToDraw; i++)
        {
            DrawPiece();
        }
    }

    public void DrawPieces(int numPiecesToDraw, Vector3 startPos)
    {
        DrawPieces(numPiecesToDraw, startPos, false);
    }

    public void DrawPieces(int numPiecesToDraw, Vector3 startPos, bool onlyDestructors)
    {
        int handSpace = maxHandSize - hand.Count;
        if (selectedPiece != null) handSpace -= 1;
        if (DrawTask.pieceInTransit[playerNum - 1]) handSpace -= 1;
        int numPiecesToBurn = Mathf.Max(numPiecesToDraw - handSpace, 0);
        for (int i = numPiecesToBurn - 1; i >= 0; i--)
        {
            hand[i].BurnFromHand();
            hand.RemoveAt(i);
        }
        for (int i = 0; i < numPiecesToDraw; i++)
        {
            DrawPiece(startPos, onlyDestructors);
        }
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
            //if (deckClumped.Count == 0) InitializeNormalDeck();
            //int index = Random.Range(0, deckClumped[0].Count);
            //piece = deckClumped[0][index];
            //deckClumped[0].Remove(piece);
            //if (deckClumped[0].Count == 0) deckClumped.Remove(deckClumped[0]);
            if (normalDeck.Count == 0) InitializeNormalDeck();
            int index = Random.Range(0, normalDeck.Count);
            piece = normalDeck[index];
            normalDeck.Remove(piece);
        }
        else
        {
            //List<Polyomino> destructors = new List<Polyomino>();
            //int bigPieceIncrement = 0;
            //if (biggerBombs) bigPieceIncrement = 1;
            //for (int numBlocks = 2 + bigPieceIncrement; numBlocks <= 3 + bigPieceIncrement; 
            //    numBlocks++)
            //{
            //    int numTypes = Polyomino.pieceTypes[numBlocks];
            //    for (int index = 0; index < numTypes; index++)
            //    {
            //        destructors.Add(new Destructor(numBlocks, index, this, false));
            //    }
            //}
            //piece = destructors[Random.Range(0,destructors.Count)];
            if (destructorDeck.Count == 0) InitializeDestructorDeck();
            int index = Random.Range(0, destructorDeck.Count);
            piece = destructorDeck[index];
            destructorDeck.Remove(piece as Destructor);
        }
        return piece;
    }

    Vector3 GetBlueprintPosition(Blueprint blueprint)
    {
        Vector3 screenPos = Vector3.zero;
        switch (blueprint.buildingType)
        {
            case BuildingType.FACTORY:
                screenPos = Services.UIManager.factoryBlueprintLocations[playerNum - 1].position;
                break;
            case BuildingType.MINE:
                screenPos = Services.UIManager.mineBlueprintLocations[playerNum - 1].position;
                break;
            case BuildingType.BOMBFACTORY:
                screenPos = Services.UIManager.bombFactoryBlueprintLocations[playerNum - 1].position;
                break;
            default:
                break;
        }
        Vector3 rawWorldPos = Services.GameManager.MainCamera.ScreenToWorldPoint(screenPos);
        return new Vector3(rawWorldPos.x, rawWorldPos.y, 0);
    }

    public void AddBluePrint(Blueprint blueprint)
    {
        //blueprints.Add(blueprint);
        blueprint.MakePhysicalPiece(false);
        blueprint.Reposition(GetBlueprintPosition(blueprint));
        //OrganizeHand(blueprints);
    }

    void OrganizeHand<T>(List<T> heldpieces) where T : Polyomino
    {
        OrganizeHand<T>(heldpieces, false);
    }

    void OrganizeHand<T>(List<T> heldpieces, bool instant) where T :Polyomino
    {
        int provisionalHandCount = heldpieces.Count;
        bool emptySpace = false;
        if (selectedPiece != null) {
            provisionalHandCount += 1;
            emptySpace = true;
        }
        handTargetPositions = new List<Vector3>();
        for (int i = 0; i < provisionalHandCount; i++)
        {
            Vector3 newPos = GetHandPosition(i);
            int handPos = i;
            if(emptySpace && i > selectedPieceHandPos)
            {
                handPos -= 1;
            }
            if (!emptySpace || (emptySpace && i != selectedPieceHandPos))
            {
                if (instant) heldpieces[handPos].Reposition(newPos);
                handTargetPositions.Add(newPos);
            }
        }
    }

    public Vector3 GetHandPosition(int handIndex)
    {
        Vector3 offset = handOffset;
        float spacingMultiplier = 1;
        if (playerNum == 2)
        {
            spacingMultiplier = -1;
            offset = new Vector3(-handOffset.x, -handOffset.y, handOffset.z);
        }
        //offset += Services.GameManager.MainCamera.ScreenToWorldPoint(handZone.transform.position);
        offset += Services.GameManager.MainCamera.transform.position;
        offset = new Vector3(offset.x, offset.y, 0);
        Vector3 newPos = new Vector3(
                handSpacing.x * (handIndex / piecesPerHandColumn) * spacingMultiplier,
                handSpacing.y * (handIndex % piecesPerHandColumn) * spacingMultiplier, 0) + offset;
        return newPos;
    }
    #endregion

    public void ToggleHandZoneView(bool viewPieces)
    {
        viewingHand = viewPieces;
        //if (!viewingHand) Services.UIManager.SetGreyOutBox(playerNum, false);
        //else Services.UIManager.SetGreyOutBox(playerNum, !placementAvailable);
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
        for (int i = 0; i < hand.Count; i++)
        {
            if (hand[i] == selectedPiece)
            {
                selectedPieceHandPos = i;
                break;
            }
        }
        if (piece.buildingType == BuildingType.NONE) hand.Remove(piece);
        else blueprints.Remove((Blueprint)piece);

        OrganizeHand(hand);
    }


    public void OnPiecePlaced(Polyomino piece)
    {
        BuildingType blueprintType = piece.buildingType;
        if (!(piece is Blueprint) && piece.cost != 10)
        {
            resources -= piece.cost;
        }
        else if(piece is Blueprint)
        {
            AddBluePrint(System.Activator.CreateInstance(
                piece.GetType(), new Object[] { this }) as Blueprint);
            uiTabs.ToggleHandZoneView(true);
        }
        selectedPiece = null;
        OrganizeHand(hand);
        piece.SetGlowState(false);
        boardPieces.Add(piece);
        Services.MapManager.DetermineConnectedness(this);
        if (Services.MapManager.CheckForWin(piece)) Services.GameScene.GameWin(this);
    }

    public void OnPieceRemoved(Polyomino piece)
    {
        boardPieces.Remove(piece);
        Services.MapManager.DetermineConnectedness(this);
    }

    public void CancelSelectedPiece()
    {
        hand.Insert(selectedPieceHandPos, selectedPiece);
        selectedPiece = null;
        OrganizeHand(hand);
    }

    public void CancelSelectedBlueprint()
    {
        //blueprints.Add((Blueprint)selectedPiece);
        selectedPiece.SetGlowState(false);
        //OrganizeHand(blueprints);
        Blueprint selectedBlueprint = selectedPiece as Blueprint;
        selectedBlueprint.Reposition(GetBlueprintPosition(selectedBlueprint));
        selectedPiece = null;
    }

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

    public int GainResources(int numResources)
    {
        int prevResources = resources;
        resources = Mathf.Min(maxResources, resources + numResources);
        Services.AudioManager.CreateTempAudio(Services.Clips.ResourceGained, 0.2f);
        return resources - prevResources;
    }

    public void AugmentDrawRateFactor(float factorChangeIncrement)
    {
        drawRateFactor += factorChangeIncrement;
    }

    public void AugmentResourceGainRate(float gainRateAmt)
    {
        resourceGainRate += gainRateAmt;
    }

    public void AugmentNormalDrawRate(float drawRateAmt)
    {
        normalDrawRate += drawRateAmt;
    }

    public void AugmentDestructorDrawRate(float drawRateAmt)
    {
        destructorDrawRate += drawRateAmt;
    }

    public void AugmentResourceGainIncrementFactor(float factorChangeIncrement)
    {
        resourceGainFactor += factorChangeIncrement;
    }

    public void ToggleAutoFortify(bool autoFortify_)
    {
        autoFortify = autoFortify_;
    }

    public void ToggleShieldedPieces(bool shieldedPieces_)
    {
        shieldedPieces = shieldedPieces_;
    }

    public void GainOwnership(Structure structure)
    {
        boardPieces.Add(structure);
    }

    public void LoseOwnership(Structure structure)
    {
        boardPieces.Remove(structure);
    }

    public void ToggleBiggerBricks(bool status)
    {
        biggerBricks = status;
        InitializeNormalDeck();
    }

    public void ToggleBiggerBombs(bool status)
    {
        biggerBombs = status;
        InitializeNormalDeck();
    }

    public void ToggleSplashDamage(bool status)
    {
        splashDamage = status;
    }
}
