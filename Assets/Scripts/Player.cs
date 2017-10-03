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
    private bool placeMode_;
    private bool placeMode
    {
        get { return placeMode_; }
        set
        {
            if (value != placeMode_)
            {
                placeMode_ = value;
                uiCursor.SetActive(!placeMode_);
                placementCursor.SetActive(placeMode_);
                if (placeMode_)
                {
                    Services.GameEventManager
                        .Unregister<LeftStickAxisEvent>(MoveUICursorOnJoystickInput);
                    Services.GameEventManager
                        .Register<LeftStickAxisEvent>(MovePlacementCursor);
                    SetPlacementCursorPosition(new Coord(
                        Services.MapManager.MapWidth / 2,
                        Services.MapManager.MapLength / 2));
                }
                else
                {
                    Services.GameEventManager
                        .Register<LeftStickAxisEvent>(MoveUICursorOnJoystickInput);
                    Services.GameEventManager
                        .Unregister<LeftStickAxisEvent>(MovePlacementCursor);
                }
            }
        }
    }
    private bool placementAvailable;
    [SerializeField]
    private Vector3 handSpacing;
    [SerializeField]
    private Vector3 handOrigin;
    [SerializeField]
    private int startingHandSize;
    public Transform uiArea { get; private set; }
    private GameObject uiCursor;
    private int uiCursorPos;
    [SerializeField]
    private Vector3 uiCursorOffset;
    [SerializeField]
    private float uiCursorMovementCooldown;
    private float timeSinceUICursorMovement;
    private GameObject placementCursor;
    private Coord placementCursorPos;
    [SerializeField]
    private float placementCursorBaseSpeed;
    private float placementCursorSpeed;
    [SerializeField]
    private float placementCursorAccel;
    [SerializeField]
    private float placementCursorDrag;
    private float placementCursorMovementCooldown
    {
        get
        {
            return 1 / placementCursorSpeed;
        }
    }
    private float timeSincePlacementCursorMovement;
    private Coord lastMovementDirection;
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
        uiArea = Services.GameScene.uiAreas[playerNum - 1];
        uiCursor = uiArea.transform.GetChild(0).gameObject;
        placementCursor = Services.GameScene.placementCursors[playerNum - 1];
        placementCursorSpeed = placementCursorBaseSpeed;

        Services.GameEventManager.Register<ButtonPressed>(OnButtonPressed);

        hand = new List<Polyomino>();

        InitializeDeck();
        DrawPieces(startingHandSize);

        SetUICursorPosition(0);
        placeMode = true;
        placeMode = false; // this looks silly but it's meant to trigger the property changes

        //for now just allow placement always
        placementAvailable = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (placeMode)
        {
            timeSincePlacementCursorMovement += Time.deltaTime;
            placementCursorSpeed = Mathf.Max(
                    placementCursorBaseSpeed, placementCursorSpeed * placementCursorDrag);
        }
        else timeSinceUICursorMovement += Time.deltaTime;
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

    void SelectPiece()
    {
        selectedPiece = hoveredPiece;
        placeMode = true;
        hand.Remove(selectedPiece);
        selectedPiece.holder.transform.parent = placementCursor.transform;
        selectedPiece.Reposition(Vector3.zero);
        OrganizeHand();
    }

    void TryToPlayPiece()
    {
        Debug.Log("trying to place");
        if (selectedPiece.IsPlacementLegal() && placementAvailable) PlaySelectedPiece();
        else Debug.Log("invalid placement");
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
        Debug.Log("placing");
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
                    if (placeMode) TryToPlayPiece();
                    else SelectPiece();
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

    

    void SetUICursorPosition(int pos)
    {
        uiCursorPos = pos;
        uiCursor.transform.localPosition = (pos * handSpacing) + handOrigin + uiCursorOffset;
        timeSinceUICursorMovement = 0;
        hoveredPiece = hand[pos];
    }

    void MoveUICursorOnJoystickInput(LeftStickAxisEvent e)
    {
        if(e.playerNum == playerNum && timeSinceUICursorMovement > uiCursorMovementCooldown)
        {
            if (e.leftStickAxis.y > 0 && uiCursorPos > 0)
            {
                SetUICursorPosition(uiCursorPos - 1);
            }
            else if (e.leftStickAxis.y < 0 && uiCursorPos < hand.Count - 1)
            {
                SetUICursorPosition(uiCursorPos + 1);
            }
        }
    }

    void MovePlacementCursor(LeftStickAxisEvent e)
    {
        if (e.playerNum == playerNum)
        {
            Coord movementDirection = new Coord(0, 0);
            if (e.leftStickAxis.y < 0 && placementCursorPos.y > 0)
            {
                movementDirection = movementDirection.Add(new Coord(0, -1));
            }
            else if (e.leftStickAxis.y > 0 && placementCursorPos.y < Services.MapManager.MapLength - 1)
            {
                movementDirection = movementDirection.Add(new Coord(0, 1));
            }
            if (e.leftStickAxis.x < 0 && placementCursorPos.x > 0)
            {
                movementDirection = movementDirection.Add(new Coord(-1, 0));
            }
            else if (e.leftStickAxis.x > 0 && placementCursorPos.x < Services.MapManager.MapWidth - 1)
            {
                movementDirection = movementDirection.Add(new Coord(1, 0));
            }
            if (movementDirection == lastMovementDirection)
            {
                placementCursorSpeed += placementCursorAccel;
            }
            lastMovementDirection = movementDirection;
            if(timeSincePlacementCursorMovement > placementCursorMovementCooldown)
            {
                SetPlacementCursorPosition(placementCursorPos.Add(movementDirection));

            }
        }
    }

    void SetPlacementCursorPosition(Coord pos)
    {
        placementCursorPos = pos;
        placementCursor.transform.position = 
            Services.MapManager.Map[pos.x, pos.y].transform.position + 5 * Vector3.back;
        timeSincePlacementCursorMovement = 0;
        if (selectedPiece != null) selectedPiece.SetTileCoords(pos);
    }
}
