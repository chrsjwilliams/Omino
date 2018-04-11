using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SlideInTitleScreenButtons : Task
{
    private const float duration = 0.5f;
    private const float staggerTime = 0.05f;
    private float totalDuration;
    private float timeElapsed;
    private GameObject[] buttons;
    private Vector3 startPosition;
    private Vector3[] targetPositions;
    private bool[] buttonsOn;

    public SlideInTitleScreenButtons(Button[] buttons_, Vector3 startPos)
    {
        buttons = new GameObject[buttons_.Length];
        for (int i = 0; i < buttons_.Length; i++)
        {
            buttons[i] = buttons_[i].gameObject;
        }
        startPosition = startPos;
    }

    protected override void Init()
    {
        timeElapsed = 0;
        totalDuration = duration + (staggerTime * buttons.Length);
        targetPositions = new Vector3[buttons.Length];
        buttonsOn = new bool[buttons.Length];
        for (int i = 0; i < buttons.Length; i++)
        {
            GameObject button = buttons[i];
            buttonsOn[i] = false;
            targetPositions[i] = button.transform.position;
            button.transform.position = startPosition;
        }
    }

    internal override void Update()
    {
        timeElapsed += Time.deltaTime;

        for (int i = 0; i < buttons.Length; i++)
        {
            if (timeElapsed >= i * staggerTime &&
                timeElapsed <= duration + (i * staggerTime))
            {
                if (!buttonsOn[i])
                {
                    buttons[i].SetActive(true);
                    buttonsOn[i] = true;
                }
                buttons[i].transform.position = Vector3.Lerp(
                    startPosition,
                    targetPositions[i],
                    EasingEquations.Easing.QuadEaseOut(
                        (timeElapsed - (i * staggerTime)) / duration));
            }
        }

        if (timeElapsed >= totalDuration) SetStatus(TaskStatus.Success);
    }
}
