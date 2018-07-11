using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using System;

public class AILevelSlideIn : Task
{
    private bool hasExternalText;
    private const float duration = 0.3f;
    private float timeElapsed;
    private const float staggerTime = 0.05f;
    private float initialOffset = 1000;
    private float totalDuration;
    private GameObject[] objectsToSlideIn;
    private Vector3[] startPositions;
    private Vector3[] targetPositions;
    private bool player1;
    private bool exit;

    public AILevelSlideIn(TextMeshProUGUI levelText, Button[] levelButtons,
        bool player1_, bool exit_)
    {
        hasExternalText = levelText ? true: false;

        if (hasExternalText)
        {
            
            objectsToSlideIn = new GameObject[levelButtons.Length + 1];
            objectsToSlideIn[0] = levelText.gameObject;
            for (int i = 0; i < levelButtons.Length; i++)
            {
                objectsToSlideIn[i + 1] = levelButtons[i].gameObject;
            }
        }
        else
        {
            objectsToSlideIn = new GameObject[levelButtons.Length];
            for (int i = 0; i < levelButtons.Length; i++)
            {
                objectsToSlideIn[i] = levelButtons[i].gameObject;
                objectsToSlideIn[i].SetActive(true);
            }
        }
        player1 = player1_;
        exit = exit_;
    }

    protected override void Init()
    {
        initialOffset *= (Screen.height/ 1027f);
        timeElapsed = 0;
        totalDuration = duration + (objectsToSlideIn.Length * staggerTime);

        if (hasExternalText)
        {
            objectsToSlideIn[0].transform.parent.gameObject.SetActive(true);
        }

        startPositions = new Vector3[objectsToSlideIn.Length];
        targetPositions = new Vector3[objectsToSlideIn.Length];
        for (int i = 0; i < objectsToSlideIn.Length; i++)
        {
            GameObject obj = objectsToSlideIn[i];
            targetPositions[i] = obj.transform.localPosition;
            Vector3 offset;
            offset = player1 ? 
                initialOffset * Vector3.down:
                initialOffset * Vector3.up;
            startPositions[i] = obj.transform.localPosition + offset;
            if (!exit) obj.transform.localPosition = startPositions[i];
        }
        if (exit)
        {
            Array.Reverse(objectsToSlideIn);
            Array.Reverse(startPositions);
            Array.Reverse(targetPositions);
        }
    }

    internal override void Update()
    {
        timeElapsed += Time.deltaTime;

        for (int i = 0; i < objectsToSlideIn.Length; i++)
        {
            if (timeElapsed >= i * staggerTime &&
                timeElapsed <= duration + (i * staggerTime))
            {
                if (exit)
                {
                    objectsToSlideIn[i].transform.localPosition = Vector3.Lerp(
                    targetPositions[i],
                    startPositions[i],
                    EasingEquations.Easing.QuadEaseOut(
                        (timeElapsed - (i * staggerTime)) / duration));
                }
                else
                {
                    objectsToSlideIn[i].transform.localPosition = Vector3.Lerp(
                    startPositions[i],
                    targetPositions[i],
                    EasingEquations.Easing.QuadEaseOut(
                        (timeElapsed - (i * staggerTime)) / duration));
                }
            }
        }

        if (timeElapsed >= totalDuration) SetStatus(TaskStatus.Success);

    }

    protected override void OnSuccess()
    {
        for (int i = 0; i < objectsToSlideIn.Length; i++)
        {
            if (exit)
            {
                objectsToSlideIn[i].transform.localPosition = startPositions[i];
            }
            else
            {
                objectsToSlideIn[i].transform.localPosition = targetPositions[i];
            }
        }
    }
}
