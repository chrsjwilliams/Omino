using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIHolderScaler : MonoBehaviour {
    [SerializeField]
    private bool widthBased;

	// Use this for initialization
	void Awake () {
        RectTransform rectTform = GetComponent<RectTransform>();
        float height = rectTform.sizeDelta.y;
        float width = rectTform.sizeDelta.x;
        if (widthBased)
        {
            rectTform.sizeDelta =
                new Vector2(width,
                width * (float)Screen.height / (float)Screen.width);
        }
        else
        {
            rectTform.sizeDelta =
                new Vector2(
                    height * (float)Screen.width / (float)Screen.height,
                    height);

        }

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
