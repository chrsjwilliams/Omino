using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIHolderScaler : MonoBehaviour {


	// Use this for initialization
	void Awake () {
        RectTransform rectTform = GetComponent<RectTransform>();
        float height = rectTform.sizeDelta.y;
        rectTform.sizeDelta =
            new Vector2(height * (float)Screen.width / (float)Screen.height,
            height);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
