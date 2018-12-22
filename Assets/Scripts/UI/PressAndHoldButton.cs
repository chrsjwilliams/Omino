using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PressAndHoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{

    private const float pauseTime = 0.5f;
    private float timeHeld;
    private const float holdScale = 5f;
    public Image filledImage;
    private bool pressed;
    private readonly Vector3 offset = 50 * Vector3.left;
    private Vector3 basePos;
    private Level selectedLevel;

    private delegate void ButtonAction();
    ButtonAction handler;

    private void Start()
    {
        if(transform.name == "OverwriteMap")
        {
            handler = ToMenu;
        }
        else
        {
            handler = DeleteMap;
        }
        filledImage.fillAmount = 0;
        basePos = transform.localPosition;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Services.AudioManager.PlaySoundEffect(Services.Clips.UIButtonPressed, 1.0f);
        pressed = true;
        timeHeld = 0;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ReturnToNeutral();
    }

    public void SetSelectedLevel(Level level)
    {
        selectedLevel = level;
    }

    private void ReturnToNeutral()
    {
        pressed = false;
        transform.localScale = Vector3.one;
        filledImage.fillAmount = 0;
        transform.localPosition = basePos;
    }

    private void Update()
    {
        if (pressed)
        {
            timeHeld += Time.unscaledDeltaTime;
            filledImage.fillAmount = timeHeld / pauseTime;
            if (timeHeld >= pauseTime)
            {
                handler();
            }
        }
    }

    private void ToMenu()
    {
        ((EditSceneScript)Services.GameScene).SaveSuccessful();
    }

    private void DeleteMap()
    {
        LevelManager.RemoveLevel(selectedLevel.levelName, true);
        Services.GameEventManager.Fire(new RefreshLevelSelectSceneEvent());
    }
}
