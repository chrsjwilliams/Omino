﻿using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Player : MonoBehaviour
{

    public int playerNum { get; private set; }

    [SerializeField] private Color[] colorScheme = new Color[2];
    public Color[] ColorScheme
    {
        get { return colorScheme; }
    }

    protected readonly Vector3 handSpacing = new Vector3(2.35f, -2.35f, 0f);
    protected readonly Vector3 handOffset = new Vector3(-9.25f, 1.75f, 0f);
    protected const int startingHandSize = 4;
    protected const int maxHandSize = 5;
    protected const int piecesPerHandRow = 5;
    protected const int baseMaxResources = 3;
    [SerializeField]
    protected float startingResources = 1.5f;
    [SerializeField]
    protected int dangerDistance = 11;

    private List<Polyomino> normalDeck;
    private List<Destructor> destructorDeck;
    public List<Polyomino> hand { get; protected set; }
    public int handCount { get { return hand.Count; } }
    private List<Vector3> handTargetPositions;
    public List<Blueprint> blueprints { get; protected set; }
    public Polyomino selectedPiece { get; protected set; }
    private int selectedPieceHandPos;
    public Base mainBase { get; private set; }
    public bool gameOver { get; private set; }
    public List<Polyomino> boardPieces { get; protected set; }
     
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

    // Tech Bonuses
    public float resourceGainFactor { get; protected set; }
    public float drawRateFactor { get; protected set; }
    public bool shieldedPieces { get; protected set; }
    protected bool biggerBricks;
    protected bool biggerBombs;
    public bool splashDamage { get; protected set; }
    // 

    protected float resourceGainRate;
    protected float normalDrawRate;
    protected float destructorDrawRate;
    protected int resourceProdLevel
    {
        get { return resourceProdLevel_; }
        set
        {
            resourceProdLevel_ = value;
            Services.UIManager.UpdateResourceLevel(resourceProdLevel_, playerNum);
        }
    }
    private int resourceProdLevel_;
    protected int normProdLevel
    {
        get { return normProdLevel_; }
        set
        {
            normProdLevel_ = value;
            Services.UIManager.UpdateNormLevel(normProdLevel_, playerNum);
        }
    }
    private int normProdLevel_;
    protected int destProdLevel
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
    public Polyomino queuedNormalPiece { get; private set; }
    public Polyomino queuedDestructor { get; private set; }
    public bool ready { get; private set; }
    private bool handLocked;
    private Coord homeBasePos;

    private HashSet<Mine> activeMines;
    private HashSet<Factory> activeFactories;
    private HashSet<BombFactory> activeBombFactories;
    private HashSet<Base> activeExpansions;
    [SerializeField]
    public bool inDanger;
    [SerializeField]
    protected Tile dangerTile;

    private List<Tile> bpAssistHighlightedTiles = new List<Tile>();
    private const float bpAssistDuration = 5f;
    private const float bpAssistFlashPeriod = 1f;
    private float bpAssistTimeElapsed;


    public virtual void Init(int playerNum_, AIStrategy strategy, AILEVEL level_)
    {
        Init(playerNum_);
    }

    // Use this for initialization
    public virtual void Init(int playerNum_)
    {
        playerNum = playerNum_;

        colorScheme = Services.GameManager.colorSchemes[playerNum - 1];

        hand = new List<Polyomino>();
        blueprints = new List<Blueprint>();
        boardPieces = new List<Polyomino>();

        InitializeNormalDeck();
        if(Services.GameManager.destructorsEnabled)
            InitializeDestructorDeck();
        if (Services.GameManager.levelSelected != null &&
            Services.GameManager.levelSelected.stackDestructorInOpeningHand)
        {
            DrawPieces(startingHandSize - 1);
            DrawPieces(1, true);
        }
        else
        {
            DrawPieces(startingHandSize);
        }
        OrganizeHand(hand, true);
        foreach(Polyomino piece in hand)
        {
            piece.holder.gameObject.SetActive(false);
        }
        if (Services.GameManager.destructorsEnabled)
        {
            QueueUpNextPiece(true);
            queuedDestructor.holder.gameObject.SetActive(false);
        }
        QueueUpNextPiece(false);
        queuedNormalPiece.holder.gameObject.SetActive(false);

        if (Services.GameManager.blueprintsEnabled)
        {
            Factory factory = new Factory(this);
            AddBluePrint(factory);

            Mine mine = new Mine(this);
            AddBluePrint(mine);

            BombFactory bombFactory = new BombFactory(this);
            AddBluePrint(bombFactory);
        }

        foreach(Blueprint blueprint in blueprints)
        {
            blueprint.holder.gameObject.SetActive(false);
        }

        if (playerNum == 1) homeBasePos = new Coord(1, 1);
        else
        {
            homeBasePos = new Coord(
                Services.MapManager.MapWidth - 2,
                Services.MapManager.MapHeight - 2);
        }
        Services.MapManager.CreateMainBase(this, homeBasePos);
        maxResources = baseMaxResources;
        resources = Mathf.FloorToInt(startingResources);
        resourceMeterFillAmt = startingResources - resources;
        resourceGainFactor = 1;
        drawRateFactor = 1;
        activeMines = new HashSet<Mine>();
        activeFactories = new HashSet<Factory>();
        activeBombFactories = new HashSet<BombFactory>();
        activeExpansions = new HashSet<Base>();
        SetProductionValues();
        ToggleHandLock(true);

        inDanger = false;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (!gameOver && Services.GameScene.gameInProgress)
        {
            UpdateMeters();

            if (((Input.GetKeyDown(KeyCode.Space) || (Input.GetMouseButtonDown(1))) && !(this is AIPlayer)) && selectedPiece != null)
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
            if (selectedPiece != null && !Services.GameManager.disableUI)
            {
                selectedPiece.SetLegalityGlowStatus();
            }

            if(bpAssistHighlightedTiles.Count > 0)
            {
                bpAssistTimeElapsed += Time.deltaTime;
                float alpha;
                float periodicTime = bpAssistTimeElapsed % bpAssistFlashPeriod;
                if (periodicTime < bpAssistFlashPeriod / 2)
                {
                    alpha = Mathf.Lerp(0, 1,
                        EasingEquations.Easing.QuadEaseOut(
                            periodicTime / (bpAssistFlashPeriod / 2)));
                }
                else
                {
                    alpha = Mathf.Lerp(1, 0,
                        EasingEquations.Easing.QuadEaseIn(
                            (periodicTime - (bpAssistFlashPeriod / 2)) / (bpAssistFlashPeriod / 2)));
                }
                foreach (Tile tile in bpAssistHighlightedTiles)
                {
                    tile.SetBpAssistAlpha(alpha);
                }
                if(bpAssistTimeElapsed >= bpAssistDuration)
                {
                    ClearBlueprintAssistHighlight();
                }
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
        if (destructorDrawMeterFillAmt >= 1 && 
            Services.GameManager.destructorsEnabled)
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

    public void DrawPieces(int numPiecesToDraw, bool onlyDestructors)
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
            DrawPiece(onlyDestructors);
        }
    }

    public void DrawPieces(int numPiecesToDraw)
    {
        DrawPieces(numPiecesToDraw, false);
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
    public virtual void DrawPiece(bool onlyDestructors)
    {
        Polyomino piece = GetRandomPieceFromDeck(onlyDestructors);
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
        Services.GameScene.tm.Do(drawTask);
        QueueUpNextPiece(onlyDestructors);
        Services.AudioManager.CreateTempAudio(Services.Clips.PieceDrawn, 1);
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
        OrganizeHand(hand);
    }

    Polyomino GetRandomPieceFromDeck(bool onlyDestructors)
    {
        Polyomino piece;
        if (!onlyDestructors)
        {
            if (normalDeck.Count == 0) InitializeNormalDeck();
            int index = Random.Range(0, normalDeck.Count);
            piece = normalDeck[index];
            normalDeck.Remove(piece);
        }
        else
        { 
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
        Vector3 centerpoint = blueprint.GetCenterpoint() * Polyomino.unselectedScale.x;
        rawWorldPos -= centerpoint;
        return new Vector3(rawWorldPos.x, rawWorldPos.y, 0);
    }

    public void AddBluePrint(Blueprint blueprint)
    {
        blueprints.Add(blueprint);
        blueprint.MakePhysicalPiece();
        blueprint.Reposition(GetBlueprintPosition(blueprint));
    }

    public void TurnOnHand()
    {
        foreach(Polyomino piece in hand)
        {
            piece.holder.gameObject.SetActive(true);
        }
    }

    public void OrganizeHand<T>(List<T> heldpieces, bool instant = false) where T :Polyomino
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
            int handPos = i;
            if(emptySpace && i > selectedPieceHandPos)
            {
                handPos -= 1;
            }
            if (!emptySpace || (emptySpace && i != selectedPieceHandPos))
            {
                Vector3 newPos = GetHandPosition(i);
                if (instant) heldpieces[handPos].Reposition(newPos, true);
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
        offset += Services.CameraController.screenEdges[playerNum - 1];
        offset = new Vector3(offset.x, offset.y, 0);
        Vector3 newPos = new Vector3(
                handSpacing.x * (handIndex % piecesPerHandRow) * spacingMultiplier,
                handSpacing.y * (handIndex / piecesPerHandRow) * spacingMultiplier,
                0)
                + offset;
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
            if(piece is Destructor && dangerTile != null)
            {
                if (dangerTile.occupyingPiece == null ||
                    dangerTile.occupyingPiece.owner == null ||
                    dangerTile.occupyingPiece.owner == this ||
                    !dangerTile.occupyingPiece.connected)
                {
                    dangerTile = null;
                    inDanger = false;
                }
            }
        }
        else if(piece is Blueprint)
        {
            AddBluePrint(System.Activator.CreateInstance(
                piece.GetType(), new Object[] { this }) as Blueprint);
            ClearBlueprintAssistHighlight();
        }
        selectedPiece = null;
        OrganizeHand(hand);
        piece.SetGlowState(false);
        foreach(Polyomino subpiece in subpieces)
        {
            if (!subpiece.dead) boardPieces.Add(subpiece);
        }
        bool hadSplashDamage = splashDamage;
        Services.MapManager.DetermineConnectedness(this);
        if (!(piece is Blueprint) && !(this is AIPlayer) && Services.GameManager.BlueprintAssistEnabled)
        {
            BlueprintAssistCheck(piece);
        }
        if (!(piece is Destructor && hadSplashDamage) 
            && Services.MapManager.CheckForWin(piece))
            Services.GameScene.GameWin(this);
        else if (Services.GameManager.Players[playerNum % 2] != null)
            Services.GameManager.Players[playerNum % 2].OnOpposingPiecePlaced(piece);

    }

    public void AddBase(Base mainBase_)
    {
        boardPieces.Add(mainBase_);
        mainBase = mainBase_;
        mainBase.holder.gameObject.SetActive(false);
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
                inDanger = true;
                if(dist < minDist)
                {
                    minDist = dist;
                    pos = tile.transform.position;
                    closeEnough = true;
                    Tile mapTile = Services.MapManager.Map[tile.coord.x, tile.coord.y];
                    dangerTile = mapTile;
                }
            }
        }
        if (closeEnough && !(this is AIPlayer))
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
        foreach(Tile tile in bpAssistHighlightedTiles)
        {
            if (piece.tiles.Contains(tile) || !tile.pieceParent.connected)
            {
                ClearBlueprintAssistHighlight();
                break;
            }
        }
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
        return resourcesGained;
    }

    public void AugmentDrawRateFactor(float factorChangeIncrement)
    {
        drawRateFactor += factorChangeIncrement;
    }

    public void AugmentResourceGainRate(float gainRateAmt)
    {
        resourceGainRate += gainRateAmt;
        int levelChange = gainRateAmt > 0 ? 1 : -1;
        resourceProdLevel += levelChange;
    }

    public void AugmentNormalDrawRate(float drawRateAmt)
    {
        normalDrawRate += drawRateAmt;
        int levelChange = drawRateAmt > 0 ? 1 : -1;
        normProdLevel += levelChange;
    }

    public void AugmentDestructorDrawRate(float drawRateAmt)
    {
        destructorDrawRate += drawRateAmt;
        int levelChange = drawRateAmt > 0 ? 1 : -1;
        destProdLevel += levelChange;
    }

    public void AugmentResourceGainIncrementFactor(float factorChangeIncrement)
    {
        resourceGainFactor += factorChangeIncrement;
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

    public void AddActiveExpansion(Base expansion)
    {
        activeExpansions.Add(expansion);
        SetProductionValues();
    }

    public void RemoveActiveExpansion(Base expansion)
    {
        activeExpansions.Remove(expansion);
        SetProductionValues();
    }

    public void AddActiveBlueprint(Blueprint blueprint)
    {
        switch (blueprint.buildingType)
        {
            case BuildingType.FACTORY:
                activeFactories.Add(blueprint as Factory);
                break;
            case BuildingType.MINE:
                activeMines.Add(blueprint as Mine);
                break;
            case BuildingType.BOMBFACTORY:
                activeBombFactories.Add(blueprint as BombFactory);
                break;
            default:
                break;
        }
        SetProductionValues();
    }

    public void RemoveActiveBlueprint(Blueprint blueprint)
    {
        switch (blueprint.buildingType)
        {
            case BuildingType.FACTORY:
                activeFactories.Remove(blueprint as Factory);
                break;
            case BuildingType.MINE:
                activeMines.Remove(blueprint as Mine);
                break;
            case BuildingType.BOMBFACTORY:
                activeBombFactories.Remove(blueprint as BombFactory);
                break;
            default:
                break;
        }
        SetProductionValues();
    }

    private void SetProductionValues()
    {
        int expansionCount = activeExpansions.Count;
        normProdLevel = activeFactories.Count + expansionCount + 1;
        destProdLevel = activeBombFactories.Count + expansionCount + 1;
        resourceProdLevel = activeMines.Count + expansionCount + 1;
        normalDrawRate = Base.normalDrawRate + 
            ((normProdLevel - 1) * Factory.drawRateBonus);
        destructorDrawRate = Base.destDrawRate +
            ((destProdLevel - 1) * BombFactory.drawRateBonus);
        resourceGainRate = Base.resourceGainRate +
            ((resourceProdLevel - 1) * Mine.resourceRateBonus);
    }

    private void BlueprintAssistCheck(Polyomino pieceJustPlaced)
    {
        HashSet<Coord> possibleBlueprintCoords = new HashSet<Coord>();
        foreach(Tile tile in pieceJustPlaced.tiles)
        {
            Coord tileCoord = tile.coord;
            for (int x = -3; x <= 3; x++)
            {
                for (int y = -3; y <= 3; y++)
                {
                    Coord coordCandidate = tileCoord.Add(new Coord(x, y));
                    Tile mapTile = null;
                    if (Services.MapManager.IsCoordContainedInMap(coordCandidate))
                    {
                        mapTile = Services.MapManager.Map[coordCandidate.x, coordCandidate.y];
                    }
                    if(mapTile != null && mapTile.occupyingBlueprint == null && 
                        mapTile.occupyingPiece != null && !(mapTile.occupyingPiece is Structure)
                        && mapTile.occupyingPiece.connected && mapTile.occupyingPiece.owner == this)
                    {
                        possibleBlueprintCoords.Add(coordCandidate);
                    }
                }
            }
        }
        List<BlueprintMap> possibleBlueprintMoves = new List<BlueprintMap>();
        foreach (Blueprint blueprint in blueprints)
        {
            Coord roundedPos = new Coord((int)blueprint.holder.transform.position.x,
                                            (int)blueprint.holder.transform.position.y);

            blueprint.SetTileCoords(roundedPos);
            int numRotations = blueprint.maxRotations;
            foreach (Coord coord in possibleBlueprintCoords)
            {
                for (int rotations = 0; rotations < 4; rotations++)
                {
                    blueprint.Rotate(false, true);
                    blueprint.SetTileCoords(coord);
                    blueprint.TurnOffGlow();
                    if (rotations < numRotations)
                    {
                        if (blueprint.IsPlacementLegal())
                        {
                            possibleBlueprintMoves.Add(new BlueprintMap(blueprint, null, coord, rotations));
                        }
                    }
                }
            }
        }
        if(possibleBlueprintMoves.Count > 0)
        {
            BlueprintMap moveToHighlight = null;
            List<BuildingType> priorityBlueprints = new List<BuildingType>();
            int minProdLevel = Mathf.Min(normProdLevel, destProdLevel, resourceProdLevel);
            if (resourceProdLevel == minProdLevel) priorityBlueprints.Add(BuildingType.MINE);
            else if (normProdLevel == minProdLevel) priorityBlueprints.Add(BuildingType.FACTORY);
            else if (destProdLevel == minProdLevel) priorityBlueprints.Add(BuildingType.BOMBFACTORY);
            foreach(BlueprintMap blueprintMove in possibleBlueprintMoves)
            {
                bool containsRecentlyPlayedPiece = false;
                foreach (Tile tile in pieceJustPlaced.tiles)
                {
                    if (blueprintMove.allCoords.Contains(tile.coord))
                    {
                        containsRecentlyPlayedPiece = true;
                        break;
                    }
                }
                if (containsRecentlyPlayedPiece &&
                        priorityBlueprints.Contains(blueprintMove.blueprint.buildingType))
                {
                    moveToHighlight = blueprintMove;
                    break;
                }
            }
            if (moveToHighlight == null) moveToHighlight = possibleBlueprintMoves[0];
            //Debug.Log("highlighting possible blueprint of type " + moveToHighlight.blueprint.buildingType +
            //    " at coord " + moveToHighlight.targetCoord);
            foreach(Coord coord in moveToHighlight.allCoords)
            {
                bpAssistHighlightedTiles.Add(
                    Services.MapManager.Map[coord.x, coord.y].occupyingPiece.tiles[0]);
            }
        }
    }

    private void ClearBlueprintAssistHighlight()
    {
        foreach(Tile tile in bpAssistHighlightedTiles)
        {
            if (!tile.pieceParent.dead) tile.SetBpAssistAlpha(0);
        }
        bpAssistHighlightedTiles.Clear();
        bpAssistTimeElapsed = 0;
    }
}
