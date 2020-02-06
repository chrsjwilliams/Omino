using UnityEngine;
using UnityEngine.UI;
using System;

public class CampaignLevelMenuEntranceTask : Task
{
    private const float panelScaleUpDur = 0.3f;
    private const float crownScaleUpDur = 0.2f;
    private const float crownScaleUpDelay = 0.3f;
    private const float wreathsScaleUpDur = 0.4f;
    private const float wreathsScaleUpDelay = 0.4f;
    private const float buttonDelay = 0.6f;
    private const float buttonStaggerTime = 0.15f;
    private const float buttonDropDur = 0.3f;
    private float timeElapsed;
    private Transform panel;
    private Transform crown;
    private Transform[] wreaths;
    private Vector3[] buttonTargetPositions;
    private Transform[] buttons;
    private Vector3[] buttonStartPos;

    public CampaignLevelMenuEntranceTask(Transform whole, Image crown_, 
        Image[] wreaths_, Button[] buttons_)
    {
        panel = whole;
        crown = crown_.transform;
        wreaths = new Transform[2]
        {
            wreaths_[0].transform,
            wreaths_[1].transform
        };
        buttons = new Transform[buttons_.Length];
        buttonStartPos = new Vector3[buttons_.Length];
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i] = buttons_[i].transform;
            buttonStartPos[i] = new Vector2(buttons[i].localPosition.x, 
                                            buttons[0].localPosition.y);
        }
        Array.Reverse(buttons);
        Array.Reverse(buttonStartPos);
    }

    protected override void Init()
    {
        timeElapsed = 0;
        buttonTargetPositions = new Vector3[buttons.Length];
        for (int i = 0; i < buttonTargetPositions.Length; i++)
        {
            buttonTargetPositions[i] = buttons[i].localPosition;
            buttons[i].localPosition = buttonStartPos[i];
        }

        buttons[buttons.Length - 1].localPosition = buttonTargetPositions[buttons.Length - 1];
        buttons[buttons.Length - 1].gameObject.SetActive(true);
    }

    internal override void Update()
    {
        timeElapsed += Time.deltaTime;

        if(timeElapsed <= panelScaleUpDur)
        {
            panel.localScale = Vector3.Lerp(Vector3.zero, Vector3.one,
                EasingEquations.Easing.QuadEaseOut(timeElapsed / panelScaleUpDur));
        }
        if(timeElapsed >= crownScaleUpDelay && 
            timeElapsed <= crownScaleUpDelay + crownScaleUpDur)
        {
            crown.localScale = Vector3.Lerp(Vector3.zero, Vector3.one,
                EasingEquations.Easing.QuadEaseOut(
                    (timeElapsed - crownScaleUpDelay) / crownScaleUpDur));
        }
        if (timeElapsed >= wreathsScaleUpDelay &&
            timeElapsed <= wreathsScaleUpDelay + wreathsScaleUpDur)
        {
            for (int i = 0; i < wreaths.Length; i++)
            {
                wreaths[i].localScale = Vector3.Lerp(Vector3.zero, Vector3.one,
                    EasingEquations.Easing.QuadEaseOut(
                        (timeElapsed - wreathsScaleUpDelay) / wreathsScaleUpDur));
            }
        }
        if(timeElapsed >= buttonDelay &&
            timeElapsed <= buttonDelay + (buttons.Length * buttonStaggerTime) + buttonDropDur)
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                if(timeElapsed >= buttonDelay + (i*buttonStaggerTime) &&
                    timeElapsed <= buttonDelay + (i*buttonStaggerTime) + buttonDropDur)
                {
                    buttons[i].gameObject.SetActive(true);
                    if (i == buttons.Length - 2) buttons[i + 1].gameObject.SetActive(true);
                    buttons[i].localPosition = Vector3.Lerp(buttonStartPos[i],
                        buttonTargetPositions[i],
                        EasingEquations.Easing.QuadEaseOut(
                            (timeElapsed - buttonDelay - (i * buttonStaggerTime)) / buttonDropDur));
                }
            }
        }
        if (timeElapsed >= buttonDelay + (buttons.Length*buttonStaggerTime) + buttonDropDur)
            SetStatus(TaskStatus.Success);
    }
}
