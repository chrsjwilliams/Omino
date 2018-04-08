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

    private List<Polyomino> normalDeck;
    private List<Destructor> destructorDeck;
    [SerializeField]
    protected int deckClumpCount;
    protected List<List<Polyomino>> deckClumped;
    protected List<Polyomino> hand;
    public int handCount { get { return hand.Count; } }
    private List<Vector3> handTargetPositions;
    public List<Blueprint> blueprints { get; protected set; }
    public Polyomino selectedPiece { get; protected set; }
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
    protected float resourceGainRate;
    protected float normalDrawRate;
    protected float destructorDrawRate;
    private int resourceProdLevel
    {
        get { return resourceProdLevel_; }
        set
        {
            resourceProdLevel_ = value;
            Services.UIManager.UpdateResourceLevel(resourceProdLevel_, playerNum);
        }
    }
    private int resourceProdLevel_;
    private int normProdLevel
    {
        get { return normProdLevel_; }
        set
        {
            normProdLevel_ = value;
            Services.UIManager.UpdateNormLevel(normProdLevel_, playerNum);
        }
    }
    private int normProdLevel_;
    private int destProdLevel
    {
        get { return destProdLevel_; }
        set
        {
            destProdLevel_ = value;
            Services.UIManager.UpdateDestLevel(destProdLevel_, playerNum);
        }
    }
    private int destProdLevel_;
    public float resourceMeterFillAmt { get; private set; }
    private float normalDrawMeterFillAmt;
    private float destructorDrawMeterFillAmt;
    [SerializeField]
    protected int resourcesPerTick;
    private Polyomino queuedNormalPiece;
    private Polyomino queuedDestructor;
    public bool ready { get; private set; }
    private bool handLocked;
    private Coord homeBasePos;
    [SerializeField]
    private int dangerDistance;


    public virtual void Init(int playerNum_, AIStrategy strategy, int level_)
    {
        Init(playerNum_);
    }

    // Use this for initialization
    public virtual void Init(int playerNum_)
    {
        playerNum = playerNum_;

        colorScheme = Services.GameManager.colorSchemes[playerNum - 1];

        handZone = Services.UIManager.handZones[playerNum - 1];

        hand = new List<Polyomino>();
        blueprints = new List<Blueprint>();
        boardPieces = new List<Polyomino>();

        InitializeNormalDeck();
        InitializeDestructorDeck();
        if (Services.GameManager.tutorialMode && !(this is AIPlayer))
        {
            DrawPiece(new Polyomino(4, 1, this));
        }
        else DrawPieces(startingHandSize);
        OrganizeHand(hand, true);
        QueueUpNextPiece(true);
        QueueUpNextPiece(false);

        Factory factory = new Factory(this);
        AddBluePrint(factory);

        Mine mine = new Mine(this);
        AddBluePrint(mine);

        BombFactory bombFactory = new BombFactory(this);
        AddBluePrint(bombFactory);


        if (playerNum == 1) homeBasePos = new Coord(1, 1);
        else
        {
            homeBasePos = new Coord(
                Services.MapManager.MapWidth - 2,
                Services.MapManager.MapHeight - 2);
        }
        Services.MapManager.CreateMainBase(this, homeBasePos);
        maxResources = baseMaxResources;
        resources = startingResources;
        resourceGainFactor = 1;
        drawRateFactor = 1;
        ToggleHandLock(true);
        //testing
        ToggleAutoFortify(true);
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (!gameOver && Services.GameScene.gameInProgress)
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
            if (selectedPiece != null)
            {
                selectedPiece.SetLegalityGlowStatus();
            }
        }

    }

    void UpdateMeters()
    {
        normalDrawMeterFillAmt += normalDrawRate * drawRateFactor * Time.deltaTime;
        destructorDrawMeterFillAmt += destructorDrawRate * drawRateFactor * Time.deltaTime;
        resourceMeterFillAmt += resourceGainRate * resourceGainFactor * Time.deltaTime;
        float normalTimeLeft = ((1 - normalDrawMeterFillAmt) /
            (normalDrawRate * drawRateFactor));
        float destructorTimeLeft = ((1 - destructorDrawMeterFillAmt) /
            (destructorDrawRate * drawRateFactor));
        Services.UIManager.UpdateDrawMeters(playerNum, normalDrawMeterFillAmt, 
            destructorDrawMeterFillAmt, normalTimeLeft, destructorTimeLeft);
        Services.UIManager.UpdateResourceMeter(playerNum, resourceMeterFillAmt);
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
        if (queuedNormalPiece != null)
        {
            if ((queuedNormalPiece.tiles.Count == 4 && biggerBricks) ||
                queuedNormalPiece.tiles.Count == 5 && !biggerBricks)
            {
                QueueUpNextPiece(false);
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
                destructorDeck.Add(new Destructor(pieceSize, index, this));
            }
        }

        if (queuedDestructor != null)
        {
            if((queuedDestructor.tiles.Count == 3 && biggerBombs) ||
                (queuedDestructor.tiles.Count == 4 && !biggerBombs))
            QueueUpNextPiece(true);
        }
    }

    protected virtual void BurnFromHand(Polyomino piece)
    {
        piece.BurnFromHand();
        hand.Remove(piece);
    }

    public void DrawPieces(int numPiecesToDraw)
    {
        int handSpace = maxHandSize - hand.Count;
        if (selectedPiece != null && !(selectedPiece is Blueprint)) handSpace -= 1;
        int numPiecesToBurn = Mathf.Max(numPiecesToDraw - handSpace, 0);
        for (int i = numPiecesToBurn - 1; i >= 0; i--)
        {
            BurnFromHand(hand[i]);
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
        if (selectedPiece != null && !(selectedPiece is Blueprint)) handSpace -= 1;
        if (DrawTask.pieceInTransit[playerNum - 1]) handSpace -= 1;
        int numPiecesToBurn = Mathf.Max(numPiecesToDraw - handSpace, 0);
        for (int i = numPiecesToBurn - 1; i >= 0; i--)
        {
            BurnFromHand(hand[i]);
        }
        for (int i = 0; i < numPiecesToDraw; i++)
        {
            DrawPiece(startPos, onlyDestructors);
        }
    }

    public virtual void DrawPiece(Polyomino piece)
    {
        piece.MakePhysicalPiece();
        AddPieceToHand(piece);
    }

    //draw instantly
    public virtual void DrawPiece()
    {
        Polyomino piece = GetRandomPieceFromDeck(false);
        DrawPiece(piece);
    }

    //draw with task
    public virtual void DrawPiece(Vector3 startPos, bool onlyDestructors)
    {
        Polyomino piece;
        if (onlyDestructors)
        {
            piece = queuedDestructor;
            queuedDestructor = null;
        }
        else
        {
            piece = queuedNormalPiece;
            queuedNormalPiece = null;
        }
        Task drawTask = new DrawTask(piece, startPos);
        Services.GeneralTaskManager.Do(drawTask);
        QueueUpNextPiece(onlyDestructors);
    }

    void QueueUpNextPiece(bool destructor)
    {
        Polyomino nextPiece = GetRandomPieceFromDeck(destructor);
        nextPiece.MakePhysicalPiece();
        Vector3 position;
        position = Services.GameManager.MainCamera.ScreenToWorldPoint(
            Services.UIManager.GetBarPosition(playerNum, destructor));
        nextPiece.Reposition(new Vector3(position.x, position.y, 0));
        nextPiece.QueueUp();
        if (destructor)
        {
            if (queuedDestructor != null) queuedDestructor.DestroyThis();
            queuedDestructor = nextPiece;
        }
        else
        {
            if (queuedNormalPiece != null) queuedNormalPiece.DestroyThis();
            queuedNormalPiece = nextPiece;
        }
    }

    public virtual void AddPieceToHand(Polyomino piece)
    {
        hand.Add(piece);
        //piece.SetVisible(viewingHand);
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
        blueprints.Add(blueprint);
        blueprint.MakePhysicalPiece();
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
        if (selectedPiece != null && !(selectedPiece is Blueprint)) {
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


    public virtual void OnPiecePlaced(Polyomino piece, List<Polyomino> subpieces)
    {
        Services.GameEventManager.Fire(new PiecePlaced(piece));
        BuildingType blueprintType = piece.buildingType;
        if (!(piece is Blueprint))
        {
            resources -= piece.cost;
        }
        else if(piece is Blueprint)
        {
            AddBluePrint(System.Activator.CreateInstance(
                piece.GetType(), new Object[] { this }) as Blueprint);
            //uiTabs.ToggleHandZoneView(true);
        }
        selectedPiece = null;
        OrganizeHand(hand);
        piece.SetGlowState(false);
        foreach(Polyomino subpiece in subpieces)
        {
            if (!subpiece.dead) boardPieces.Add(subpiece);
        }
        Services.MapManager.DetermineConnectedness(this);
        if (Services.MapManager.CheckForWin(piece))
            Services.GameScene.GameWin(this);
        if (Services.GameManager.Players[playerNum % 2] != null)
            Services.GameManager.Players[playerNum % 2].OnOpposingPiecePlaced(piece);

    }

    public void AddBase(Base mainBase_)
    {
        boardPieces.Add(mainBase_);
        mainBase = mainBase_;
    }

    public virtual void OnOpposingPiecePlaced(Polyomino piece)
    {
        if (piece is Blueprint) return;
        float minDist = Mathf.Infinity;
        Vector3 pos = Vector3.zero;
        bool closeEnough = false;
        foreach (Tile tile in piece.tiles)
        {
            float dist = tile.coord.Distance(homeBasePos);
            if (dist < dangerDistance)
            {
                if(dist < minDist)
                {
                    minDist = dist;
                    pos = tile.transform.position;
                    closeEnough = true;
                }
            }
        }
        if (closeEnough)
        {
            GameObject dangerEffect = GameObject.Instantiate(Services.Prefabs.DangerEffect);
            dangerEffect.transform.position = pos;
            float rot = playerNum == 1 ? -90 : 90;
            dangerEffect.transform.rotation = Quaternion.Euler(0, 0, rot);
        }
    }

    public virtual void OnPieceRemoved(Polyomino piece)
    {
        boardPieces.Remove(piece);
        Services.MapManager.DetermineConnectedness(this);
    }

    public virtual void CancelSelectedPiece()
    {
        if (selectedPiece.cost > resources)
            Services.UIManager.FailedPlayFromLackOfResources(this, 
                selectedPiece.cost - resources);
        if (selectedPiece is Destructor && (selectedPiece as Destructor).StoppedByShield())
        {
            Services.AudioManager.CreateTempAudio(Services.Clips.ShieldHit, 1);
        }
        else
        {
            Services.AudioManager.CreateTempAudio(Services.Clips.IllegalPlay, 0.01f);
        }
        int handPosToPlace = Mathf.Min(selectedPieceHandPos, hand.Count);
        hand.Insert(handPosToPlace, selectedPiece);
        selectedPiece.SetGlowState(false);
        selectedPiece = null;
        OrganizeHand(hand);
    }

    public void CancelSelectedBlueprint()
    {
        blueprints.Add((Blueprint)selectedPiece);
        selectedPiece.SetGlowState(false);
        Services.AudioManager.CreateTempAudio(Services.Clips.IllegalPlay, 0.01f);
        //OrganizeHand(blueprints);
        Blueprint selectedBlueprint = selectedPiece as Blueprint;
        selectedBlueprint.Reposition(GetBlueprintPosition(selectedBlueprint));
        selectedPiece = null;
    }

    public void OnGameOver()
    {
        if (selectedPiece != null)
        {
            if (selectedPiece is Blueprint) CancelSelectedBlueprint();
            else CancelSelectedPiece();
        }
        gameOver = true;
    }

    public virtual int GainResources(int numResources)
    {
        int prevResources = resources;
        resources = Mathf.Min(maxResources, resources + numResources);
        int resourcesGained = resources - prevResources;
        if(resourcesGained > 0)
            Services.AudioManager.CreateTempAudio(Services.Clips.ResourceGained, 0.2f);
        //Vector3 resourceUILocation = Services.GameManager.MainCamera.ScreenToWorldPoint(
        //    Services.UIManager.resourceCounters[playerNum - 1].transform.position);
        //resourceUILocation = new Vector3(resourceUILocation.x, resourceUILocation.y, 0);
        //Vector3 offset = Services.UIManager.resourceGainAnimationOffset;
        //if (playerNum == 2) offset = new Vector3(offset.x, -offset.y, offset.z);
        //resourceUILocation += offset;
        //FloatText floatText = new FloatText("+" + resourcesGained,
        //   resourceUILocation, this,
        //   Services.UIManager.resourceGainAnimationDist, 
        //   Services.UIManager.resourceGainAnimationDur);
        //Services.GeneralTaskManager.Do(floatText);
        return resourcesGained;
    }

    public void AugmentDrawRateFactor(float factorChangeIncrement)
    {
        drawRateFactor += factorChangeIncrement;
    }

    public void AugmentResourceGainRate(float gainRateAmt)
    {
        resourceGainRate += gainRateAmt;
        resourceProdLevel += gainRateAmt > 0 ? 1 : -1;
    }

    public void AugmentNormalDrawRate(float drawRateAmt)
    {
        normalDrawRate += drawRateAmt;
        normProdLevel += drawRateAmt > 0 ? 1 : -1;
    }

    public void AugmentDestructorDrawRate(float drawRateAmt)
    {
        destructorDrawRate += drawRateAmt;
        destProdLevel += drawRateAmt > 0 ? 1 : -1;
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
        InitializeDestructorDeck();
    }

    public void ToggleSplashDamage(bool status)
    {
        splashDamage = status;
        Services.GameEventManager.Fire(new SplashDamageStatusChange(this));
    }

    public void ToggleReady()
    {
        ready = !ready;
    }

    public void ToggleHandLock(bool lockHand)
    {
        bool wasLocked = handLocked;
        handLocked = lockHand;

        for (int i = 0; i < hand.Count; i++)
        {
            if (handLocked)
            {
                hand[i].Lock();
            }
            else if(wasLocked)
            {
                hand[i].Unlock();
            }
        }
        for (int i = 0; i < blueprints.Count; i++)
        {
            if (handLocked)
            {
                blueprints[i].Lock();
            }
            else if (wasLocked)
            {
                blueprints[i].Unlock();
            }
        }
    }
}
