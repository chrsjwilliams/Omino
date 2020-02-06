using UnityEngine;
using UnityEngine.UI;

public class SlideOutTitleScreenButtons : Task
{
    private const float animDuration = 0.2f;
    private const float staggerTime = 0.05f;
    private float timeElapsed;
    private Button[] buttons;
    private Vector3[] startPositions;
    private Vector3[] targetPositions;
    private readonly Vector3 offset = 800f * Vector3.up;

    public SlideOutTitleScreenButtons(Button[] buttons_)
    {
        buttons = buttons_;
    }

    protected override void Init()
    {
        timeElapsed = 0;

        startPositions = new Vector3[buttons.Length];
        targetPositions = new Vector3[buttons.Length];

        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];
            startPositions[i] = button.transform.localPosition;
            targetPositions[i] = button.transform.localPosition + offset;
        }
    }

    internal override void Update()
    {
        timeElapsed += Time.deltaTime;

        for (int i = 0; i < buttons.Length; i++)
        {
            if(timeElapsed >= i * staggerTime)
            {
                buttons[i].transform.localPosition = Vector3.Lerp(
                    startPositions[i], targetPositions[i],
                    EasingEquations.Easing.QuadEaseIn(
                        Mathf.Min(1, (timeElapsed - (i * staggerTime)) / animDuration)));
            }
        }

        if (timeElapsed >= animDuration + buttons.Length * staggerTime) SetStatus(TaskStatus.Success);
    }
}
