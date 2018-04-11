using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LevelSelectButtonEntranceTask : Task
{
    private const float duration = 0.25f;
    private const float staggerTime = 0.05f;
    private float totalDuration;
    private float timeElapsed;
    private GameObject[] buttons;
    private Vector3[] startPositions;
    private Vector3[] targetPositions;
    private const float initialOffset = 600;

    public LevelSelectButtonEntranceTask(Button[] buttons_)
    {
        buttons = new GameObject[buttons_.Length];
        for (int i = 0; i < buttons_.Length; i++)
        {
            buttons[i] = buttons_[i].gameObject;
        }
    }

    protected override void Init()
    {
        timeElapsed = 0;
        totalDuration = duration + (staggerTime * buttons.Length);
        startPositions = new Vector3[buttons.Length];
        targetPositions = new Vector3[buttons.Length];
        for (int i = 0; i < buttons.Length; i++)
        {
            GameObject button = buttons[i];
            button.SetActive(true);
            Vector3 offset;
            if (button.GetComponent<RectTransform>().anchoredPosition.x < 0)
            {
                offset = initialOffset * Vector3.left;
            }
            else
            {
                offset = initialOffset * Vector3.right;
            }

            targetPositions[i] = button.transform.localPosition;
            button.transform.localPosition += offset;
            startPositions[i] = button.transform.localPosition;
        }
    }

    internal override void Update()
    {
        timeElapsed += Time.deltaTime;

        for (int i = 0; i < buttons.Length; i++)
        {
            if (timeElapsed >= i * staggerTime && 
                timeElapsed <= duration + (i*staggerTime))
            {
                buttons[i].transform.localPosition = Vector3.Lerp(
                    startPositions[i],
                    targetPositions[i],
                    EasingEquations.Easing.QuadEaseOut(
                        (timeElapsed - (i * staggerTime)) / duration));
            }
        }

        if (timeElapsed >= totalDuration) SetStatus(TaskStatus.Success);
    }
}

public class LevelSelectTextEntrance: Task
{
    private const float duration = 0.2f;
    private float timeElapsed;
    private GameObject levelSelectText;
    private Vector3 startPos;
    private Vector3 targetPos;
    private const float initialOffset = 200;

    public LevelSelectTextEntrance(GameObject levelSelectText_)
    {
        levelSelectText = levelSelectText_;
    }


    protected override void Init()
    {
        levelSelectText.SetActive(true);
        targetPos = levelSelectText.transform.position;

            levelSelectText.transform.position += initialOffset * Vector3.up;
        startPos = levelSelectText.transform.position;
        timeElapsed = 0;
    }

    internal override void Update()
    {
        timeElapsed += Time.deltaTime;

        levelSelectText.transform.position = Vector3.Lerp(
            startPos,
            targetPos,
            EasingEquations.Easing.QuadEaseOut(timeElapsed / duration));

        if (timeElapsed >= duration) SetStatus(TaskStatus.Success);
    }
}
