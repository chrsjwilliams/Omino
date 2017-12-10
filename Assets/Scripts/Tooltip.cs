﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour {

    public Text nameText;
    public Text descriptionText;
    [SerializeField]
    private Vector2 tooltipOffset;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Init(string name, string description, float rot, Vector2 basePos, 
        bool blueprintInHand)
    {
        nameText.text = name;
        descriptionText.text = description;
        transform.localRotation = Quaternion.Euler(0, 0, rot);
        Vector2 offset = tooltipOffset * Screen.width / 2048f;
        if (basePos.y > Screen.width / 2)
        {
            offset = new Vector2(offset.x, -offset.y);
        }
        if (rot < 0)
        {
            offset = new Vector2(-offset.x, offset.y);
        }
        if (blueprintInHand)
        {
            offset = new Vector2(-offset.x, offset.y);
        }
        transform.position = basePos + offset;
    }

    public void Init(string name, string description, float rot, Vector2 basePos)
    {
        Init(name, description, rot, basePos, false);
    }


}