using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideInHandicapUITask : Task
{

    private const float duration = 0.25f;
    private const float staggerTime = 0.05f;
    private float totalDuration;
    private float timeElapsed;
    private float initalOffset = 1000;
    private Vector3 startPosition;
    private Vector3 targetPosition;

    private GameObject handicapUI;

    public SlideInHandicapUITask(GameObject handicapUI_)
    {
        handicapUI = handicapUI_;
    }

    protected override void Init()
    {
        timeElapsed = 0;
        initalOffset *= (Screen.width / 1027f);
        totalDuration = duration;
        targetPosition = handicapUI.transform.localPosition;
        handicapUI.transform.localPosition += (initalOffset * Vector3.down);
        startPosition = handicapUI.transform.localPosition;
    }

    internal override void Update()
    {
        timeElapsed += Time.deltaTime;

        handicapUI.transform.localPosition = Vector3.Lerp(
                        startPosition,
                        targetPosition,
                        EasingEquations.Easing.QuadEaseOut(
                            (timeElapsed / duration)));

        if(timeElapsed >= duration)
        {
            handicapUI.transform.localPosition = targetPosition;
        }

        if (timeElapsed >= totalDuration) SetStatus(TaskStatus.Success);
    }
}
