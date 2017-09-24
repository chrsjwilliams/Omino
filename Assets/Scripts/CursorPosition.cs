using UnityEngine;

public class CursorPosition : MonoBehaviour
{
    [SerializeField] private bool _disableMovement;

    [SerializeField] private int _offsetX;
    private int maxX;
    public int X
    {
        get { return _offsetX; }
        private set
        {
            _offsetX = value;
            if (_offsetX < 0)
            {
                _offsetX = 0;
            }
            else if (_offsetX > maxX)
            {
                _offsetX = maxX;
            }
        }
    }

    [SerializeField] private int _offsetY;
    private int maxY;
    public int Y
    {
        get { return _offsetY; }
        private set
        {
            _offsetY = value;
            if (_offsetY < 0)
            {
                _offsetY = 0;
            }
            else if (_offsetY > maxY)
            {
                _offsetY = maxY;
            }
        }
    }

    private bool resetLeftStick = false;
    private bool resetDPad = false;
    private float _worldSpaceOffset;

    [SerializeField] private KeyCode upKey = KeyCode.UpArrow;
    [SerializeField] private KeyCode downKey = KeyCode.DownArrow;
    [SerializeField] private KeyCode leftKey = KeyCode.LeftArrow;
    [SerializeField] private KeyCode rightKey = KeyCode.RightArrow;

    void Start()
    {
        _disableMovement = false;
        _worldSpaceOffset = 0.3f;
        maxX = Services.MapManager.MapWidth - 1;
        maxY = Services.MapManager.MapLength - 1;

        Services.GameEventManager.Register<ButtonPressed>(OnButtonPressed);
        Services.GameEventManager.Register<KeyPressedEvent>(OnKeyPressed);
        Services.GameEventManager.Register<DisablePlayerMovement>(OnTogglePlayerMovement);
    }

    private void OnDestroy()
    {
        Services.GameEventManager.Unregister<ButtonPressed>(OnButtonPressed);
        Services.GameEventManager.Unregister<KeyPressedEvent>(OnKeyPressed);
        Services.GameEventManager.Unregister<DisablePlayerMovement>(OnTogglePlayerMovement);
    }

    public virtual void SetPosition(int _x, int _y)
    {
        _offsetX = _x;
        _offsetY = _y;
    }

    void OnButtonPressed(ButtonPressed e)
    {    
        if (e.button == "B") { }

        if (e.button == "A") { }

        if (e.button == "X") { }

        if (e.button == "Y") { }
    }

    public void LeftStickMotion(IntVector2 axis)
    {
        resetLeftStick = !resetLeftStick;
        if (resetLeftStick && !_disableMovement)
        {
            X += axis.x;
            Y -= axis.y;         
        }
        AdjustTilePos();
    }

    public void DPadMotion(IntVector2 axis)
    {

        resetDPad = !resetDPad;
        if (resetDPad && !_disableMovement)
        {
            X += axis.x;
            Y += axis.y;
        }
        AdjustTilePos();
    }

    private void OnKeyPressed(KeyPressedEvent e)
    {
        if(!_disableMovement)
        {
            if(e.key == upKey)
            {
                Y++;
            }
            else if(e.key == downKey)
            {
                Y--;
            }

            if(e.key == leftKey)
            {
                X--;
            }
            else if (e.key == rightKey)
            {
                X++;
            }
        }

        Vector3 tilePos = Services.MapManager.Map[X, Y].transform.position;

        //  Places the cursor slight above the grid map
        tilePos = new Vector3(tilePos.x, tilePos.y + _worldSpaceOffset, tilePos.z);
        transform.position = tilePos;
    }

    private void AdjustTilePos()
    {
        Vector3 tilePos = Services.MapManager.Map[X, Y].transform.position;

        //  Places the cursor slight above the grid map
        tilePos = new Vector3(tilePos.x, tilePos.y + _worldSpaceOffset, tilePos.z);
        transform.position = tilePos;
    }

    private void OnTogglePlayerMovement(DisablePlayerMovement e)
    {
        _disableMovement = e.toggleMovement;
    }
}
