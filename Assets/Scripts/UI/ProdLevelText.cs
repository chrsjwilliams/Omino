using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProdLevelText : MonoBehaviour {
    private bool active;
    private float timeElapsed;
    private const float highlightDuration = 0.5f;
    private readonly Vector3 maxScale = 1.5f * Vector3.one;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (active)
        {
            timeElapsed += Time.deltaTime;

            if(timeElapsed < highlightDuration / 2)
            {
                transform.localScale = Vector3.Lerp(Vector3.one, maxScale,
                    EasingEquations.Easing.QuadEaseOut(
                        timeElapsed / (highlightDuration / 2)));
            }
            else
            {
                transform.localScale = Vector3.Lerp(maxScale, Vector3.one,
                    EasingEquations.Easing.QuadEaseIn(
                        (timeElapsed - (highlightDuration / 2)) /
                        (highlightDuration / 2)));
            }

            if (timeElapsed >= highlightDuration) active = false;

        }
	}

    public void StartHighlight()
    {
        active = true;
        timeElapsed = 0;
    }
}
