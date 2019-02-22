using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Tooltip : MonoBehaviour {

    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    [SerializeField]
    private Vector2 tooltipOffset;
    [SerializeField]
    private Vector2 inHandOffset;
   
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {		
	}

    public void Init(string name, string description, float rot, 
        Vector2 basePos, bool blueprintInHand)
    {
        nameText.text = name.ToLower();
        descriptionText.text = description.ToLower();
        transform.localRotation = Quaternion.Euler(0, 0, rot);
        Reposition(basePos, rot, blueprintInHand);
    }

    public void Reposition(Vector2 basePos, float rot, bool blueprintInHand)
    {
        Vector2 offset = blueprintInHand ? inHandOffset : tooltipOffset;
        offset *= Screen.height / 2048f;
        Debug.Log(Screen.width);
        Debug.Log(basePos.x);
        if (basePos.x < 5)
        {
            offset = new Vector2(-offset.x, offset.y);
        }

        if (rot > 0)
        {
            offset = new Vector2(offset.x, -offset.y);
        }
        transform.position = basePos + offset;
    }

    public void Init(string name, string description, float rot, Vector2 basePos)
    {
        Init(name, description, rot, basePos, false);
    }


}
