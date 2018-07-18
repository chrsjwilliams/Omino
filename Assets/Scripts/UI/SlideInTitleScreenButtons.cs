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
    private Button buttonNotHit;
    private Vector3 buttonNotHitStart;
    private Vector3 buttonNotHitTarget;

    public SlideInTitleScreenButtons(Button[] buttons_, Vector3 startPos, GameObject title_, 
        Button unselectedButton)
    {
        buttons = new GameObject[buttons_.Length];
        for (int i = 0; i < buttons_.Length; i++)
        {
            buttons[i] = buttons_[i].gameObject;
        }
        startPosition = startPos;
        title = title_;
        buttonParent = buttons[0].transform.parent.parent.gameObject;
        buttonNotHit = unselectedButton;
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
            targetPositions[i] = button.transform.localPosition;
            buttonsOn[i] = false;
            button.transform.localPosition = startPosition;

        }
        titleStartPos = title.transform.localPosition;
        buttonParentStartPos = buttonParent.transform.localPosition;
        buttonNotHitStart = buttonNotHit.transform.localPosition;
        buttonNotHitTarget = startPosition;
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
        float progress = EasingEquations.Easing.QuadEaseOut(
                Mathf.Min(1, timeElapsed / duration));

        title.transform.localPosition = Vector3.Lerp(
            titleStartPos,
            titleStartPos + (Vector3.up * titleOffset),
            progress);
        buttonParent.transform.localPosition = Vector3.Lerp(
            buttonParentStartPos, 
            buttonParentStartPos + (Vector3.up * buttonParentOffset),
            progress);
        buttonNotHit.transform.localPosition = Vector3.Lerp(
            buttonNotHitStart,
            buttonNotHitTarget,
            progress);

        if (timeElapsed >= totalDuration) SetStatus(TaskStatus.Success);
    }

    protected override void OnSuccess()
    {
        buttonNotHit.gameObject.SetActive(false);
    }
}
