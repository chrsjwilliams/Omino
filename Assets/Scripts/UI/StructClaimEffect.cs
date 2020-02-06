
using UnityEngine;

public class StructClaimEffect : MonoBehaviour {


    private LineRenderer[] lineRenderers;
    private Material[] lineMats;
    [SerializeField]
    private float textureCycleSpeed;
    [SerializeField]
    private float maxScale;
    private float startScale;
    private float timeElapsed;
    [SerializeField]
    private float scaleUpDuration;
    private Color startColor;

	// Use this for initialization
	void Start () {

    }

    public void Init(Player claimingPlayer)
    {
        lineRenderers = GetComponentsInChildren<LineRenderer>();
        lineMats = new Material[lineRenderers.Length];
        for (int i = 0; i < lineRenderers.Length; i++)
        {
            lineMats[i] = lineRenderers[i].material;
        }
        startScale = transform.localScale.x;
        startColor = claimingPlayer.ColorScheme[0] / 2;
        for (int i = 0; i < lineRenderers.Length; i++)
        {
            lineRenderers[i].startColor = startColor;
            lineRenderers[i].endColor = startColor;
        }
    }

    // Update is called once per frame
    void Update () {
        timeElapsed += Time.deltaTime;
        for (int i = 0; i < lineMats.Length; i++)
        {
            lineMats[i].mainTextureOffset += 
                textureCycleSpeed * Time.deltaTime * Vector2.right;
        }
        transform.localScale = Vector3.Lerp(
            startScale * Vector3.one,
            startScale * maxScale * Vector3.one, 
            EasingEquations.Easing.QuadEaseOut(timeElapsed / scaleUpDuration));
        for (int i = 0; i < lineRenderers.Length; i++)
        {
            Color targetColor = new Color(startColor.r, startColor.g, startColor.b, 0);
            Color lerpedColor = Color.Lerp(startColor, targetColor,
                EasingEquations.Easing.QuadEaseOut(timeElapsed / scaleUpDuration));
            lineRenderers[i].startColor = lerpedColor;
            lineRenderers[i].endColor = lerpedColor;
        }
        //for testing
        if(timeElapsed >= scaleUpDuration)
        {
            Destroy(gameObject);
        }
	}
}
