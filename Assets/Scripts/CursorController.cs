using UnityEngine;

public class CursorController : MonoBehaviour
{

    [SerializeField] private GameObject _currentCursor;
    public GameObject CurrentCursor
    {
        get { return _currentCursor; }
    }

    [SerializeField] private int _cursorIndex;
    public int CursorIndex
    {
        get { return _cursorIndex; }
    }

    [SerializeField] private KeyCode rotateRight = KeyCode.Equals;
    [SerializeField] private KeyCode rotateLeft = KeyCode.Minus;
    [SerializeField] private KeyCode reflectX = KeyCode.LeftBracket;
    [SerializeField] private KeyCode reflectY = KeyCode.RightBracket;

    private int _rotationIndex;
    private int[] _yRotationPossiblities = new int[4];
    private int _mirrorRotationIndex;
    private int[] _mirrorRotationPossibilities = new int[2];

    private CursorPosition _location;

    // Use this for initialization
    void Start ()
    {
        _rotationIndex = 0;
        _yRotationPossiblities[0] = 0;
        _yRotationPossiblities[1] = 90;
        _yRotationPossiblities[2] = 180;
        _yRotationPossiblities[3] = 270;

        _mirrorRotationIndex = 0;
        _mirrorRotationPossibilities[0] = 0;
        _mirrorRotationPossibilities[1] = 180;

        _location = GetComponent<CursorPosition>();

        _cursorIndex = 0;
        ToggleCursor(CursorIndex);

        Services.GameEventManager.Register<KeyPressedEvent>(OnKeyPressed);
	}

    private void OnKeyPressed(KeyPressedEvent e)
    {
        if (e.key == KeyCode.LeftShift)
        {
            _cursorIndex = _cursorIndex + 1;
            ToggleCursor(CursorIndex);
        }
        else if (e.key == KeyCode.RightShift)
        {
            _cursorIndex = _cursorIndex + Services.Prefabs.Cursors.Length - 1;
            ToggleCursor(CursorIndex);
        }

        if(e.key == KeyCode.Z && CursorIndex % Services.Prefabs.Cursors.Length == 0)
        {
            //  This can be used to place peices
            Services.MapManager.ActivateTile(Services.MapManager.Map[_location.X, _location.Y]);
        }

        if (e.key == rotateRight)
        {
            //  Rotate the net 90 degress to the right
            _rotationIndex = _rotationIndex + 1;
            RotateCursor(_rotationIndex);   
        }
        else if (e.key == rotateLeft)
        {
            //  Rotate the net 90 degrees to the left
            _rotationIndex = _rotationIndex + _yRotationPossiblities.Length - 1;
            RotateCursor(_rotationIndex);
        }

        if (e.key == reflectX)
        {
            _mirrorRotationIndex = _mirrorRotationIndex + 1;
            MirrorCursor(_mirrorRotationIndex, true);
        }
        else if (e.key == reflectY)
        {
            _mirrorRotationIndex = _mirrorRotationIndex + _mirrorRotationPossibilities.Length - 1;
            MirrorCursor(_mirrorRotationIndex, false);
        }
    }

    //  This can be used to cycle through the piece options
    private void ToggleCursor(int cursorIndex)
    {
        if (_currentCursor != null)
        {
            Destroy(_currentCursor);
        }

        _currentCursor = Instantiate(Services.Prefabs.Cursors[cursorIndex % Services.Prefabs.Cursors.Length]);

        _currentCursor.transform.position = transform.position;

        string cursorName = _currentCursor.name.Replace("(Clone)", "");
        _currentCursor.name = cursorName;

        _currentCursor.transform.parent = transform;
        _currentCursor.GetComponent<CursorPosition>().SetPosition(_location.X, _location.Y);
    }

    private void RotateCursor(int rotationIndex)
    {
        _currentCursor.transform.rotation = Quaternion.Euler(   transform.rotation.x, 
                                                                _yRotationPossiblities[rotationIndex % _yRotationPossiblities.Length],
                                                                transform.rotation.z);
    }

    private void MirrorCursor(int mirrorIndex, bool mirrorX)
    {
        if(mirrorX)
        {
            _currentCursor.transform.rotation = Quaternion.Euler(   _mirrorRotationPossibilities[mirrorIndex % _mirrorRotationPossibilities.Length], 
                                                                    transform.rotation.y,
                                                                    transform.rotation.z);
        }
        else
        {
            _currentCursor.transform.rotation = Quaternion.Euler(   transform.rotation.x,
                                                                    transform.rotation.y,
                                                                    _mirrorRotationPossibilities[mirrorIndex % _mirrorRotationPossibilities.Length]);
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
    }
}
