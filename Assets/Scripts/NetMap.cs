using UnityEngine;

//  Currently Not Used for RTS_Blokus
//  This class can be used to select individual tiles of a peice
public class NetMap : MonoBehaviour
{
    [SerializeField] private bool _canMakeCube;
    public bool CanMakeCube
    {
        get { return _canMakeCube; }
    }

    [SerializeField] private bool _choosingBase;
    public bool ChoosingBase
    {
        get { return _choosingBase; }
    }

    [SerializeField] private GameObject _baseSelect;
    public GameObject BaseSelect
    {
        get { return _baseSelect; }
    }

    [SerializeField] private NetFace[] _netFaces;
    public NetFace[] NetFaces
    {
        get { return _netFaces; }
    }

    [SerializeField] private KeyCode selectKey = KeyCode.Z;

    public const int NUM_NET_FACES = 6;
    private const string TILE = "Tile ";

	// Use this for initialization
	void Start ()
    {
        _netFaces = new NetFace[NUM_NET_FACES];
	    for(int i = 0; i< NUM_NET_FACES; i++)
        {
            _netFaces[i] = transform.Find(TILE + i).GetComponent<NetFace>();
        }

        Services.GameEventManager.Register<KeyPressedEvent>(OnKeyPressed);
	}

    private void OnDestroy()
    {
        Services.GameEventManager.Unregister<KeyPressedEvent>(OnKeyPressed);
    }

    private void OnKeyPressed(KeyPressedEvent e)
    {

        if (e.key == selectKey && _canMakeCube)
        {
            _choosingBase = !_choosingBase;
            if (_choosingBase)
            {
                _baseSelect = Instantiate(Services.Prefabs.NetCursor);
                _baseSelect.transform.parent = transform;
                Services.GameEventManager.Fire(new DisablePlayerMovement(true));
            }
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
		if (NetFaces[0].CanActivate && NetFaces[1].CanActivate && NetFaces[2].CanActivate &&
            NetFaces[3].CanActivate && NetFaces[4].CanActivate && NetFaces[5].CanActivate)
        {
            _canMakeCube = true;
        }
        else
        {
            _canMakeCube = false;
            _choosingBase = false;
        }

        if(!_choosingBase && BaseSelect)
        {
            Destroy(BaseSelect);
            Services.GameEventManager.Fire(new DisablePlayerMovement(false));
        }
	}
}
