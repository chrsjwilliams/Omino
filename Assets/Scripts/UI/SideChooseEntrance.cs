using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SideChooseEntrance : Task
{
    private const float duration = 0.3f;
    private float timeElapsed;
    private GameObject[] optionBars;
    private Vector3[] startPositions;
    private Vector3[] targetPositions;
    private const float initialOffset = 200;
    private bool exit;

    public SideChooseEntrance(Button[] joinButtons, bool exit_)
    {
        optionBars = new GameObject[joinButtons.Length];
        for (int i = 0; i < optionBars.Length; i++)
        {
            optionBars[i] = joinButtons[i].gameObject;
        }
        exit = exit_;
    }

    protected override void Init()
    {
        timeElapsed = 0;
        startPositions = new Vector3[optionBars.Length];
        targetPositions = new Vector3[optionBars.Length];
        for (int i = 0; i < optionBars.Length; i++)
        {
            GameObject optionBar = optionBars[i];
            optionBar.SetActive(true);
            targetPositions[i] = optionBar.transform.position;
            Vector3 offset;
            offset = i == 0 ? initialOffset * Vector3.left :
                initialOffset * Vector3.right;
            if (exit) offset *= -1;
            optionBar.transform.position += offset;
            startPositions[i] = optionBar.transform.position;
        }
    }

    internal override void Update()
    {
        timeElapsed += Time.deltaTime;

        for (int i = 0; i < optionBars.Length; i++)
        {
            optionBars[i].transform.position = Vector3.Lerp(
                startPositions[i],
                targetPositions[i],
                EasingEquations.Easing.QuadEaseOut(timeElapsed / duration));
        }

        if (timeElapsed >= duration) SetStatus(TaskStatus.Success);
    }
}
