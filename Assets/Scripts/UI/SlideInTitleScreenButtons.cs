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
    private GameObject title;
    private Vector3 titleStartPos;
    private const float titleOffset = 450f;
    private GameObject buttonParent;
    private const float buttonParentOffset = 150f;
    private Vector3 buttonParentStartPos;

    public SlideInTitleScreenButtons(Button[] buttons_, Vector3 startPos, GameObject title_)
    {
        buttons = new GameObject[buttons_.Length];
        for (int i = 0; i < buttons_.Length; i++)
        {
            buttons[i] = buttons_[i].gameObject;
        }
        startPosition = startPos;
        title = title_;
        buttonParent = buttons[0].transform.parent.gameObject;
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
            targetPositions[i] = button.transform.localPosition;
            button.transform.localPosition = startPosition;
        }
        titleStartPos = title.transform.localPosition;
        buttonParentStartPos = buttonParent.transform.localPosition;
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
                buttons[i].transform.localPosition = Vector3.Lerp(
                    startPosition,
                    targetPositions[i],
                    EasingEquations.Easing.QuadEaseOut(
                        (timeElapsed - (i * staggerTime)) / duration));
            }
        }

        title.transform.localPosition = Vector3.Lerp(
            titleStartPos, 
            titleStartPos + (Vector3.up * titleOffset),
            EasingEquations.Easing.QuadEaseOut(
                Mathf.Min(1, timeElapsed / duration)));
        buttonParent.transform.localPosition = Vector3.Lerp(
            buttonParentStartPos, 
            buttonParentStartPos + (Vector3.up * buttonParentOffset),
            EasingEquations.Easing.QuadEaseOut(
                Mathf.Min(1, timeElapsed / duration)));

        if (timeElapsed >= totalDuration) SetStatus(TaskStatus.Success);
    }
}
