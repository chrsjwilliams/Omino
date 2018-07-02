using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideInOptionsMenuTask : Task
{

    private const float duration = 0.5f;
    private const float staggerTime = 0.5f;
    private float totalDuration;
    private float timeElapsed;
    private float initalOffset = 1500;
    private Vector3 startPosition;
    private Vector3 targetPosition;

    private GameObject optionsMenu;

    public SlideInOptionsMenuTask(GameObject optionsMenu_)
    {
        optionsMenu = optionsMenu_;
        
    }

    protected override void Init()
    {

        timeElapsed = 0;
        initalOffset *= (Screen.width / 1027f);
        totalDuration = duration;
        targetPosition = optionsMenu.transform.localPosition;
        optionsMenu.transform.localPosition += (initalOffset * Vector3.down);
        startPosition = optionsMenu.transform.localPosition;
        optionsMenu.transform.parent.gameObject.SetActive(true);

    }

    internal override void Update()
    {
        timeElapsed += Time.deltaTime;

        optionsMenu.transform.localPosition = Vector3.Lerp(
                        startPosition,
                        targetPosition,
                        EasingEquations.Easing.QuadEaseOut(
                            (timeElapsed / duration)));

        if(timeElapsed >= duration)
        {
            optionsMenu.transform.localPosition = targetPosition;
        }

        if (timeElapsed >= totalDuration) SetStatus(TaskStatus.Success);
    }
}
