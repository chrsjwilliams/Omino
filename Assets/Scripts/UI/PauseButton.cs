using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class PauseButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
    private const float pauseTime = 0.5f;
    private float timeHeld;
    private const float holdScale = 3f;
    public Image filledImage;
    private bool pressed;
    private readonly Vector3 offset = 50 * Vector3.left;
    private Vector3 basePos;

    private void Start()
    {
        filledImage.fillAmount = 0;
        basePos = transform.localPosition;
    }

    private void Update()
    {
        if (pressed)
        {
            timeHeld += Time.unscaledDeltaTime;
            filledImage.fillAmount = timeHeld / pauseTime;
            if(timeHeld >= pauseTime)
            {
                Pause();
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        //if (Services.GameScene.gamePaused)
        //{
        //    Services.UIManager.TogglePauseMenu();
        //    print("toggling pause menu");
        //}
        //else
        //{
            Services.AudioManager.PlaySoundEffect(Services.Clips.UIButtonPressed, 1.0f);
            pressed = true;
            transform.localScale = holdScale * Vector3.one;
            timeHeld = 0;
            transform.localPosition = basePos + offset;
        //}
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ReturnToNeutral();
    }

    private void Pause()
    {
        Services.AudioManager.PlaySoundEffect(Services.Clips.UIClick, 1.0f);
        //if (!Services.GameScene.gamePaused)
            Services.UIManager.TogglePauseMenu();
        //else Services.UIManager.TurnOnPauseMenu();
        ReturnToNeutral();
    }

    private void ReturnToNeutral()
    {
        pressed = false;
        transform.localScale = Vector3.one;
        filledImage.fillAmount = 0;
        transform.localPosition = basePos;
    }
}
