using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//  Currently Not Used for RTS_Blokus
public class NetFaceSelector : CursorPosition
{
    [SerializeField] private NetMap _netController;
    public NetMap NetController
    {
        get { return _netController; }
    }

    [SerializeField] private KeyCode up = KeyCode.UpArrow;
    [SerializeField] private KeyCode up_alt = KeyCode.RightArrow;
    [SerializeField] private KeyCode down = KeyCode.DownArrow;
    [SerializeField] private KeyCode down_alt = KeyCode.LeftArrow;


    private int netFaceIndex;
    


	// Use this for initialization
	void Start ()
    {
        netFaceIndex = 0;
        _netController = transform.parent.GetComponent<NetMap>();
        transform.position = NetController.NetFaces[0].transform.position;
        Services.GameEventManager.Register<KeyPressedEvent>(OnKeyPressed);
    }

    private void OnDestroy()
    {
        Services.GameEventManager.Unregister<KeyPressedEvent>(OnKeyPressed);
    }

    private void OnKeyPressed(KeyPressedEvent e)
    {
        if (e.key == up || e.key == up_alt)
        {
            netFaceIndex++;
            transform.position = NetController.NetFaces[Mathf.Abs(netFaceIndex) % NetMap.NUM_NET_FACES].transform.position;
        }
        else if (e.key == down || e.key == down_alt)
        {
            netFaceIndex--;
            transform.position = NetController.NetFaces[Mathf.Abs(netFaceIndex) % NetMap.NUM_NET_FACES].transform.position;
        }
        transform.position = new Vector3(transform.position.x, 0.5f, transform.position.z);
    }
}
