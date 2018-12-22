using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructClaimAura : MonoBehaviour {

    private SpriteRenderer ren;
    [SerializeField]
    private float distortionSpeed;
    [SerializeField]
    private float maxScaleMagnitude;
    [SerializeField]
    private float timeToMaxScale;
    private float timeElapsed;
    [SerializeField]
    private float baseScaleMagnitude;
    private Vector3 baseScale;
    private Vector3 targetScale;
    private Color baseColor;
    private Color targetColor;
    [SerializeField]
    private float startAlpha;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {

        timeElapsed += Time.deltaTime;
        transform.localScale = Vector3.Lerp(baseScale, targetScale,
            EasingEquations.Easing.QuadEaseOut(timeElapsed / timeToMaxScale));
        ren.material.SetFloat("_OffsetDistort", timeElapsed * distortionSpeed);
        ren.material.SetColor("_MainColor", 
            Color.Lerp(baseColor, targetColor,
            EasingEquations.Easing.QuadEaseOut(timeElapsed / timeToMaxScale)));
        if (timeElapsed >= timeToMaxScale)
        {
            Destroy(gameObject);
            //timeElapsed = 0;
        }
	}

    public void Init(Player claimingPlayer)
    {
        ren = GetComponent<SpriteRenderer>();
        baseScale = new Vector3(baseScaleMagnitude, baseScaleMagnitude, 1);
        targetScale = new Vector3(maxScaleMagnitude, maxScaleMagnitude, 1);
        transform.localScale = baseScale;
        baseColor = claimingPlayer.ColorScheme[0];
        baseColor = new Color(baseColor.r, baseColor.g, baseColor.b, startAlpha);
        targetColor = new Color(baseColor.r, baseColor.g, baseColor.b, 0f);
        ren.material.SetColor("_MainColor", baseColor);
    }
}
