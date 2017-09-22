using UnityEngine;

//  Currently Not Used for RTS_Blokus
public class NetFace : Tile
{
    [SerializeField] private bool _canActivate;
    public bool CanActivate
    {
        get { return _canActivate; }
        private set
        {
            _canActivate = value;
            if(_canActivate)
            {
                if ((int)transform.position.x + (int)transform.position.z % 2 == 0)
                {
                    material.color = new Color(0.478f, 0.722f, 0.361f, 0.9f);
                }
                else
                {
                    material.color = new Color(0.671f, 0.867f, 0.576f, 0.9f);
                }
            }
            else
            {
                material.color = new Color(1, 1, 1, 0.5f);
            }
        }
    }

    private bool _outsideLowerBound;
    private bool _outsideUpperBound;

	// Use this for initialization
	void Start ()
    {
        material = GetComponent<MeshRenderer>().material;
    }
	
	// Update is called once per frame
	void Update ()
    {
        _outsideLowerBound = (int)transform.position.x < 0 || (int)transform.position.z < 0;

        _outsideUpperBound = (int)transform.position.x > Services.MapManager.MapWidth - 1 ||
                             (int)transform.position.z > Services.MapManager.MapLength - 1;

        if (_outsideLowerBound || _outsideUpperBound)
        {
            material.color = new Color(1, 1, 1, 0.5f);
            CanActivate = false;
        }
        else
        {
            if (Services.MapManager.Map[(int)transform.position.x, (int)transform.position.z].isActive)
            {
                CanActivate = true;
            }
            else
            {
                CanActivate = false;
            }
        }
		
	}
}
