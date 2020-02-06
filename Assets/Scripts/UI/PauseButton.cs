
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PauseButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
    private const float pauseTime = 0.5f;
    private float timeHeld;
    private const float holdScale = 3f;
    public Image filledImage;
    private bool pressed;
    private readonly Vector3 offset = 50 * Vector3.left;
    private Vector3 basePos;

    private bool showCompleteionMenu;

   

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
        Services.AudioManager.PlaySoundEffect(Services.Clips.UIButtonPressed, 1.0f);
        pressed = true;
        transform.localScale = holdScale * Vector3.one;
        timeHeld = 0;
        transform.localPosition = basePos + offset;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ReturnToNeutral();
    }

    public void TogglePauseMenu()
    {
        if (Services.GameScene.gamePaused)
        {
            AudioListener.volume = 1.0f;
            Services.UIManager.UIMenu.pauseMenu.SetPauseMenuStatus(false);
            Services.GameScene.UnpauseGame();
        }
        else
        {
            AudioListener.volume = 0.55f;
            Services.UIManager.UIMenu.pauseMenu.SetPauseMenuStatus(true);
            Services.GameScene.PauseGame();
        }
    }

    public void ToggleOptionsMenu(bool state)
    {
        if (state)
        {
            Services.UIManager.UIMenu.optionsMenu.SetActive(true);
            Services.UIManager.UIMenu.pauseMenu.SetPauseMenuStatus(false);
            if (!Services.GameManager.MusicEnabled)
            {
                GameObject.Find("ToggleMusic").GetComponent<Image>().color = Services.GameManager.Player2ColorScheme[1];
            }
        }
        else
        {
            Services.UIManager.UIMenu.optionsMenu.SetActive(false);
            Services.UIManager.UIMenu.pauseMenu.SetPauseMenuStatus(true);
        }
    }

    private void Pause()
    {
        Services.AudioManager.PlaySoundEffect(Services.Clips.UIClick, 1.0f);
        if (!Services.GameScene.gameOver)
        {
            TogglePauseMenu();
        }
        else
        {
            ToggleCompletionMenu(Services.GameManager.mode);
        }
        ReturnToNeutral();
    }

    public void ToggleCompletionMenu(TitleSceneScript.GameMode mode)
    {
        switch (mode)
        {
            case TitleSceneScript.GameMode.Challenge:
                ToggleCompleteionMenu(Services.UIManager.UIMenu.eloUIManager.menu,
                                        true);
                break;
            case TitleSceneScript.GameMode.Tutorial:
                ToggleCompleteionMenu(Services.UIManager.UIMenu.tutorialLevelCompleteMenu.gameObject,
                                        Services.UIManager.UIMenu.tutorialLevelCompleteMenu.inPosition);
                break;
            case TitleSceneScript.GameMode.DungeonRun:
                ToggleCompleteionMenu(Services.UIManager.UIMenu.dungeonRunChallenegeCompleteMenu.menu,
                                        Services.UIManager.UIMenu.dungeonRunChallenegeCompleteMenu.inPosition);

                break;
            default:
                TogglePauseMenu();
                break;
        }
    }

    private void ToggleCompleteionMenu(GameObject menu, bool inPosition)
    {
        if (!inPosition) return;
        showCompleteionMenu = !showCompleteionMenu;
        menu.SetActive(showCompleteionMenu);
    }

    private void ReturnToNeutral()
    {
        pressed = false;
        transform.localScale = Vector3.one;
        filledImage.fillAmount = 0;
        transform.localPosition = basePos;
    }
}
