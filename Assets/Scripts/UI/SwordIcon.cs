using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordIcon : MonoBehaviour {

    private Polyomino piece;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        UpdatePositionAndScale();
	}

    public void Init(Polyomino piece_)
    {
        piece = piece_;
        UpdatePositionAndScale();
    }

    private void UpdatePositionAndScale()
    {
        Vector3 uiPos = Camera.main.WorldToScreenPoint(piece.GetCenterpoint());
        uiPos = new Vector3(uiPos.x, uiPos.y, 0);
        transform.position = uiPos;
        transform.localScale = piece.holder.transform.localScale;
    }

    public void Remove()
    {
        Destroy(gameObject);
    }
}
