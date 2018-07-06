using UnityEngine;
using System.Collections;

public class StructureEffectAnimation : MonoBehaviour
{
    private const float animDuration = 1f;
    private const float fadeDuration = 0.3f;
    private const float rotationSpeed = 1f;
    private float timeElapsed;
    private SpriteRenderer sr;

    public void Init(BuildingType tech, Vector3 pos)
    {
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = Services.TechDataLibrary.GetIcon(tech);
        transform.localPosition = pos;
    }

    // Update is called once per frame
    void Update()
    {
        timeElapsed += Time.deltaTime;

        if (timeElapsed < animDuration)
        {
            float angleIncrement = Mathf.Lerp(rotationSpeed, 0, 
                timeElapsed / animDuration);
            transform.Rotate(Vector3.forward, angleIncrement);
        }
        else
        {
            sr.color = Color.Lerp(Color.white, new Color(1, 1, 1, 0),
                EasingEquations.Easing.QuadEaseIn(
                    (timeElapsed - animDuration) / fadeDuration));
        }

        if (timeElapsed > animDuration + fadeDuration) Destroy(gameObject);
    }
}
