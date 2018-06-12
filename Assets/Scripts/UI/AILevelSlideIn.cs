using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using System;

public class AILevelSlideIn : Task
{
    private const float duration = 0.3f;
    private float timeElapsed;
    private const float staggerTime = 0.05f;
    private float initialOffset = 900f;
    private float totalDuration;
    private GameObject[] objectsToSlideIn;
    private Vector3[] startPositions;
    private Vector3[] targetPositions;
    private bool player1;
    private bool exit;

    public AILevelSlideIn(TextMeshProUGUI levelText, Button[] levelButtons,
        bool player1_, bool exit_)
    {
        objectsToSlideIn = new GameObject[levelButtons.Length + 1];
        objectsToSlideIn[0] = levelText.gameObject;
        for (int i = 0; i < levelButtons.Length; i++)
        {
            objectsToSlideIn[i + 1] = levelButtons[i].gameObject;
        }
        player1 = player1_;
        exit = exit_;
    }

    protected override void Init()
    {
        initialOffset *= (Screen.width/ 1027f);
        timeElapsed = 0;
        totalDuration = duration + (objectsToSlideIn.Length * staggerTime);
        objectsToSlideIn[0].transform.parent.gameObject.SetActive(true);
        startPositions = new Vector3[objectsToSlideIn.Length];
        targetPositions = new Vector3[objectsToSlideIn.Length];
        for (int i = 0; i < objectsToSlideIn.Length; i++)
        {
            GameObject obj = objectsToSlideIn[i];
            targetPositions[i] = obj.transform.position;
            Vector3 offset;
            offset = player1 ? 
                initialOffset * Vector3.down:
                initialOffset * Vector3.up;
            startPositions[i] = obj.transform.position + offset;
            if (!exit) obj.transform.position = startPositions[i];
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
                    objectsToSlideIn[i].transform.position = Vector3.Lerp(
                    targetPositions[i],
                    startPositions[i],
                    EasingEquations.Easing.QuadEaseOut(
                        (timeElapsed - (i * staggerTime)) / duration));
                }
                else
                {
                    objectsToSlideIn[i].transform.position = Vector3.Lerp(
                    startPositions[i],
                    targetPositions[i],
                    EasingEquations.Easing.QuadEaseOut(
                        (timeElapsed - (i * staggerTime)) / duration));
                }
            }
        }        

        if (timeElapsed >= totalDuration) SetStatus(TaskStatus.Success);
    }
}
