
using UnityEngine;

public class ScaleDownWidth : MonoBehaviour {

	// Use this for initialization
	void Start () {
        GetComponent<RectTransform>().localScale *=
            ((float)Screen.width / Screen.height) / (3f / 4f);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
