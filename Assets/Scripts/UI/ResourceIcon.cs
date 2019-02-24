using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ResourceIcon : MonoBehaviour
{
    private bool increasing;
    private bool highlighting;
    private float highlightTimeElapsed;
    private bool missingIndicatorActive;
    private bool missingIndicatorIncreasing;
    private float missingIndicatorTimeElapsed;
    private const float highlightDuration = 0.3f;
    private const float highlightScale = 2f;
    private const float missingHighlightScale = 1.25f;
    private const float energyFillMin = 0.13f;
    private const float energyFillMax = 0.9f;
    private const float attackFillMin = 0.08f;
    private const float attackFillMax = 0.94f;
    private float fillMin;
    private float fillMax;
    private Image front;
    private Image missingIndicator;
    [SerializeField]
    private bool attack;

    // Use this for initialization
    void Awake()
    {

    }

    public void Init(int playerNum)
    {
        Image[] images = GetComponentsInChildren<Image>();
        front = images[1];
        missingIndicator = images[2];
        fillMin = attack ? attackFillMin : energyFillMin;
        fillMax = attack ? attackFillMax : energyFillMax;
        StopMissingHighlight();
        for (int i = 0; i < 2; i++)
        {
            images[i].color = Services.GameManager
                .GetColorScheme()[playerNum - 1][0];
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (highlighting) Highlight();
        if (missingIndicatorActive) HighlightMissing();
    }

    private void Highlight()
    {
        highlightTimeElapsed += Time.deltaTime;
        Vector3 startScale;
        Vector3 targetScale;
        float progress = highlightTimeElapsed / highlightDuration;
        Easing.FunctionType easeType;
        if (increasing)
        {
            startScale = Vector3.one;
            targetScale = highlightScale * Vector3.one;
            easeType = Easing.FunctionType.QuadEaseOut;
            if (highlightTimeElapsed > highlightDuration)
            {
                increasing = false;
                highlightTimeElapsed = 0;
            }
        }
        else
        {
            startScale = highlightScale * Vector3.one;
            targetScale = Vector3.one;
            easeType = Easing.FunctionType.QuadEaseIn;
            if (highlightTimeElapsed > highlightDuration)
            {
                highlighting = false;
            }
        }
        transform.localScale = Vector3.Lerp(startScale, targetScale,
            Easing.GetFunctionWithTypeEnum(easeType)(progress));
    }

    public void StartHighlight()
    {
        highlighting = true;
        increasing = true;
        highlightTimeElapsed = 0;
    }

    private void HighlightMissing()
    {
        missingIndicatorTimeElapsed += Time.deltaTime;
        Vector3 startScale;
        Vector3 targetScale;
        Color startColor;
        Color targetColor;
        float progress = missingIndicatorTimeElapsed / highlightDuration;
        Easing.FunctionType easeType;
        Color baseColor = missingIndicator.color;
        if (increasing)
        {
            startScale = Vector3.one;
            targetScale = missingHighlightScale * Vector3.one;
            easeType = Easing.FunctionType.QuadEaseOut;
            startColor = new Color(baseColor.r * 0.75f, baseColor.g * 0.75f, baseColor.b * 0.75f, 0);
            targetColor = new Color(baseColor.r * 0.75f, baseColor.g * 0.75f, baseColor.b * 0.75f, 1);
            if (missingIndicatorTimeElapsed > highlightDuration)
            {
                missingIndicatorIncreasing = false;
                missingIndicatorTimeElapsed = 0;
            }
        }
        else
        {
            startScale = missingHighlightScale * Vector3.one;
            targetScale = Vector3.one;
            easeType = Easing.FunctionType.QuadEaseIn;
            startColor = new Color(baseColor.r, baseColor.g, baseColor.b, 1);
            targetColor = new Color(baseColor.r, baseColor.g, baseColor.b, 0);
            if (missingIndicatorTimeElapsed > highlightDuration)
            {
                missingIndicatorActive = false;
            }
        }
        missingIndicator.transform.localScale = Vector3.Lerp(
            startScale, targetScale,
            Easing.GetFunctionWithTypeEnum(easeType)(progress));
        missingIndicator.color = Color.Lerp(startColor, targetColor,
            Easing.GetFunctionWithTypeEnum(easeType)(progress));
    }

    public void StartMissingHighlight()
    {
        missingIndicatorActive = true;
        missingIndicatorIncreasing = true;
        missingIndicatorTimeElapsed = 0;
    }

    public void StopHighlight()
    {
        transform.localScale = Vector3.one;
        highlighting = false;
    }

    public void StopMissingHighlight()
    {
        missingIndicator.transform.localScale = Vector3.one;
        Color color = missingIndicator.color;
        missingIndicator.color = new Color(color.r, color.g, color.b, 0);
        missingIndicatorActive = false;
    }


    public void SetFill(float fillPercent)
    {
        front.fillAmount = fillMin + ((fillMax - fillMin) *
            EasingEquations.Easing.QuadEaseIn(fillPercent));
        float alpha; 
        if (fillPercent < 1)
        {
            alpha = 0.5f;
        }
        else
        {
            alpha = 1;
        }
        Color color = front.color;
        front.color = new Color(color.r, color.g, color.b, alpha);

    }
}
