﻿using UnityEngine;
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

    public LevelSelectButtonEntranceTask(Button button, 
        GameObject playButton_ = null, bool moveDown_ = false)
    {
        buttons = new GameObject[1] { button.gameObject };
        playButton = playButton_;
        moveDown = moveDown_;
    }

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
            button.SetActive(true);
            //if (levelButton != null)
            //    button.SetActive(levelButton.unlocked);
            //else button.SetActive(true);
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
                    buttons[i].gameObject.SetActive(false);
                    
                }
            }
            else
            {
                for (int i = 0; i < buttons.Length; i++)
                {
                    buttons[i].transform.localPosition = targetPositions[i];
                }
            }
        }   
    }
}

public class LevelSelectTextEntrance: Task
{
    private bool enterFromTop;
    private bool exit;
    private const float duration = 0.2f;
    private float timeElapsed;
    private GameObject levelSelectText;
    private Vector3 startPos;
    private Vector3 targetPos;
    private const float initialOffset = 1200;

    public LevelSelectTextEntrance(GameObject levelSelectText_, bool enterFromTop_ = false, bool exit_ = false)
    {
        levelSelectText = levelSelectText_;
        exit = exit_;
        enterFromTop = enterFromTop_;
    }


    protected override void Init()
    {
        levelSelectText.SetActive(true);
        targetPos = levelSelectText.transform.localPosition;
        if (enterFromTop)
        {
            levelSelectText.transform.localPosition += initialOffset * Vector3.up;
        }
        else
        {
            levelSelectText.transform.localPosition += initialOffset * Vector3.down;
        }
        startPos = levelSelectText.transform.localPosition;
        if (Services.GameManager.CurrentDevice == DEVICE.IPHONE_X && levelSelectText.name.Contains("Objectives"))
        {

            targetPos = new Vector3(targetPos.x, targetPos.y - 150, 0);
        }
        timeElapsed = 0;
        if (!exit) levelSelectText.transform.localPosition = startPos;
    }

    internal override void Update()
    {
        timeElapsed += Time.deltaTime;
        if (exit)
        {
            levelSelectText.transform.localPosition = Vector3.Lerp(
            targetPos,
            startPos,
            EasingEquations.Easing.QuadEaseOut(timeElapsed / duration));
        }
        else
        {
            levelSelectText.transform.localPosition = Vector3.Lerp(
            startPos,
            targetPos,
            EasingEquations.Easing.QuadEaseOut(timeElapsed / duration));
        }

        if (timeElapsed >= duration)
        {
            SetStatus(TaskStatus.Success);
            if (exit)
            {
                levelSelectText.transform.localPosition = startPos;
            }
            else
            {
                levelSelectText.transform.localPosition = targetPos;
            }
        }
    }
}
