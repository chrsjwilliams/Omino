using UnityEngine;
using UnityEngine.UI;

//  Currently Not Used for RTS_Blokus
//  This class is left over from Thesis Jam for reference
public class UI_Cursor : MonoBehaviour
{
	public GameObject buildingCursor;

	[SerializeField] private int offsetX = 0;
    private int maxX;
    public int X
    {
        get { return offsetX; }
        private set
        {
            offsetX = value;
            if(offsetX < 0)
            {
                offsetX = 0;
            }
            else if (offsetX > maxX)
            {
                offsetX = maxX;
            }
        }
    }

    [SerializeField] private int offsetY = 0;
	private int maxY;
    public int Y
    {
        get { return offsetY; }
        private set
        {
            offsetY = value;
            if (offsetY < 0)
            {
                offsetY = 0;
            }
            else if (offsetY > maxY)
            {
                offsetY = maxY;
            }
        }
    }

    private bool usingAxis = false;
	private float t;
    private float angle;
    private float delay = 0.3f;

    void Start ()
    {
		maxX = Services.MapManager.MapWidth - 1;
		maxY = Services.MapManager.MapLength - 1;

		Services.GameEventManager.Register<ButtonPressed> (OnButtonPressed);

        buildingCursor = Instantiate(Services.Prefabs.Cursors[0]);
        buildingCursor.transform.parent = transform.parent;
	}

	// Update is called once per frame
	void Update ()
    {
        GetTileBuildingInfo ();

		Vector3 tilePos = Services.GameManager.MainCamera.WorldToScreenPoint (Services.MapManager.Map[X,Y].transform.position);
        buildingCursor.transform.position = tilePos;
        transform.position = tilePos;

		if (buildingCursor != null)
        {
			buildingCursor.transform.position = transform.position;
		}

		float x = Input.GetAxisRaw("Horizontal");
		float y = Input.GetAxisRaw("Vertical");
		if (x != 0.0f || y != 0.0f)
        { 
			angle = Mathf.Atan2 (y, x) * Mathf.Rad2Deg;

			if (angle >= 0 && angle < 22.5) {

				if (!usingAxis) {
					Y--;
					X--;

					usingAxis = true;
				}

			} else if (angle >= 22.5f && angle <= 67.5f) {
				if (!usingAxis) {
					Y--;
					usingAxis = true;
				}

			} else if (angle > 67.5f && angle < 112.5f) {
				
				if (!usingAxis) {
					X++;
					Y--;
					usingAxis = true;
				}

			} else if (angle >= 112.5f && angle <= 157.5) {

				if (!usingAxis) {
					X++;
					usingAxis = true;
				}

				
			} else if (angle > 157.5 && angle <= 180f) {

				if (!usingAxis) {
					Y++;
					X++;
					usingAxis = true;
				}

			} else if (angle >= -180f && angle < -157.5) {

				if (!usingAxis) {
					Y++;
					X++;
					usingAxis = true;
				}

			} else if (angle >= -157.5f && angle <= -112.5f) {

				if (!usingAxis) {
					Y++;
					usingAxis = true;
				}
					
			} else if (angle > -112.5f && angle < -67.5f) {

				if (!usingAxis) {
					X--;
					Y++;
					usingAxis = true;
				}

			} else if (angle >= -67.5f && angle <= -22.5) {

				if (!usingAxis) {
					X--;
					usingAxis = true;
				}

			} else if (angle > -22.5 && angle < 0) {

				if (!usingAxis) {
					Y--;
					X--;

					usingAxis = true;
				}
	
			}
		}
        else
        {
			usingAxis = false;
		}

        if (usingAxis)
        {
			t += Time.deltaTime;

			if (t >= delay)
            {
				t = 0;
				usingAxis = false;
			}
		}
	}

	void GetTileBuildingInfo() { }

	void OnButtonPressed(ButtonPressed e)
    {
		if (e.button == "B") { }

		if (e.button == "A") { }

		if (e.button == "X") { }

		if (e.button == "Y") { }
	}
}
