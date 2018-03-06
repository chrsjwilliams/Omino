using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialTooltip : MonoBehaviour {

    private Text textComponent;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Init(Vector2 location, string text, bool dismissable)
    {
        textComponent = GetComponentInChildren<Text>();
        GetComponent<RectTransform>().anchoredPosition = location;
        textComponent.text = text;
        GetComponent<Button>().enabled = dismissable;
    }

    public void Dismiss()
    {
        Destroy(gameObject);
        Services.TutorialManager.OnDismiss();
    }

}
