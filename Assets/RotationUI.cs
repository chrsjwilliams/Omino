using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationUI : MonoBehaviour {

    private Polyomino piece;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Init(Polyomino piece_)
    {
        piece = piece_;
        float rot = piece.owner.playerNum == 1  ? -90 : 90;
        transform.localRotation = Quaternion.Euler(0, 0, rot);
        transform.position = Services.GameManager.MainCamera
            .WorldToScreenPoint(piece.holder.transform.position);
    }

    public void Rotate(bool clockwise)
    {
        piece.Rotate(true, clockwise);
    }
}
