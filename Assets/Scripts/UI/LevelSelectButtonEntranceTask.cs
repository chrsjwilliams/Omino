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
    private float initialOffset = 1000;
    private GameObject playButton;
    private Vector3 playStartPos;
    private Vector3 playTargetPos;

    private bool moveDown;

    public LevelSelectButtonEntranceTask(LevelButton[] buttons_, 
        GameObject playButton_ = null, bool moveDown_ = false)
    {
        buttons = new GameObject[buttons_.Length];
        for (int i = 0; i < buttons_.Length; i++)
        {
            buttons[i] = buttons_[i].gameObject;
        }
        playButton = playButton_;
        moveDown = moveDown_;
    }

    protected override void Init()
    {
        timeElapsed = 0;
        initialOffset *= (Screen.width / 1027f);
        totalDuration = duration + (staggerTime * (buttons.Length + 1));
        startPositions = new Vector3[buttons.Length];
        targetPositions = new Vector3[buttons.Length];
        for (int i = 0; i < buttons.Length; i++)
        {
            GameObject button = buttons[i];
            LevelButton levelButton = button.GetComponent<LevelButton>();
            button.SetActive(levelButton.unlocked);
            Vector3 offset;
            if (moveDown)
            {
                offset = initialOffset * Vector3.up;
                startPositions[i] = button.transform.localPosition;
                //button.transform.localPosition += offset;
                targetPositions[i] = startPositions[i] - offset;// button.transform.localPosition;

            }
            else
            {
                offset = initialOffset * Vector3.down;
                targetPositions[i] = button.transform.localPosition;
                button.transform.localPosition += offset;
                startPositions[i] = button.transform.localPosition;
            }

            
        }
        if (playButton != null)
        {
            playButton.SetActive(true);
            playTargetPos = playButton.transform.localPosition;
            playButton.transform.localPosition += (initialOffset * Vector3.down);
            playStartPos = playButton.transform.localPosition;
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

            if(timeElapsed >= duration + (i * staggerTime))
            {
                buttons[i].transform.localPosition = targetPositions[i];
            }
        }

        if(timeElapsed >= (buttons.Length-1) * staggerTime && playButton != null)
        {
            playButton.transform.localPosition = Vector3.Lerp(
                playStartPos, playTargetPos,
                EasingEquations.Easing.QuadEaseOut(
                    Mathf.Min(1,(timeElapsed - ((buttons.Length - 1) * staggerTime)) 
                    / duration)));
        }

        if (timeElapsed >= totalDuration)
        {
            SetStatus(TaskStatus.Success);
            if(moveDown)
            {
                for (int i = 0; i < buttons.Length; i++)
                {

                    buttons[i].transform.localPosition = startPositions[i];
                    
                }
            }
        }   
    }
}

public class LevelSelectTextEntrance: Task
{
    private const float duration = 0.25f;
    private float timeElapsed;
    private GameObject levelSelectText;
    private Vector3 startPos;
    private Vector3 targetPos;
    private const float initialOffset = 1000;

    public LevelSelectTextEntrance(GameObject levelSelectText_)
    {
        levelSelectText = levelSelectText_;
    }


    protected override void Init()
    {
        levelSelectText.SetActive(true);
        targetPos = levelSelectText.transform.localPosition;
        levelSelectText.transform.localPosition += initialOffset * Vector3.down;
        startPos = levelSelectText.transform.localPosition;
        timeElapsed = 0;
    }

    internal override void Update()
    {
        timeElapsed += Time.deltaTime;

        levelSelectText.transform.localPosition = Vector3.Lerp(
            startPos,
            targetPos,
            EasingEquations.Easing.QuadEaseOut(timeElapsed / duration));

        if (timeElapsed >= duration) SetStatus(TaskStatus.Success);
    }

    protected override void OnSuccess()
    {
        levelSelectText.transform.localPosition = targetPos;
    }
}
