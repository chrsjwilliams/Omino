using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionButton : MonoBehaviour
{

    [SerializeField]
    private Button optionButton;


    private TaskManager _tm = new TaskManager();

    // Use this for initialization
    void Start () {
		
	}

    public void SlideOutOptionsButton(bool slideOut)
    {
        LevelSelectButtonEntranceTask slideOptionButtonTask =
                new LevelSelectButtonEntranceTask(optionButton, null, slideOut);
        _tm.Do(slideOptionButtonTask);

        //  Transfer to Option Menu
    }

    public void ToggleOptionMenu()
    {
        //optionsMenuActive = !optionsMenuActive;
        //// optionButtonParent.SetActive(false);
        //levelButtonParent.SetActive(false);
        //if (Services.GameManager.mode == TitleSceneScript.GameMode.Elo)
        //{
        //    eloUI.gameObject.SetActive(!optionsMenuActive);
        //}
        //if (Services.GameManager.mode == TitleSceneScript.GameMode.Tutorial)
        //{
        //    campaignLevelButtonParent.SetActive(!optionsMenuActive);
        //}
        //if (Services.GameManager.mode == TitleSceneScript.GameMode.DungeonRun)
        //{
        //    if (DungeonRunManager.dungeonRunData.selectingNewTech)
        //    {
        //        techSelectMenu.SetActive(!optionsMenuActive);
        //    }
        //    else
        //    {
        //        dungeonRunMenu.SetActive(!optionsMenuActive);
        //    }
        //}
        //optionMenu.SetActive(optionsMenuActive);
        //if (optionsMenuActive)
        //{
        //}

        Services.Scenes.PushScene<OptionMenuSceneScript>();
    }

    public void UIClick()
    {
        Services.AudioManager.PlaySoundEffect(Services.Clips.UIClick, 0.55f);
    }

    public void UIButtonPressedSound()
    {
        Services.AudioManager.PlaySoundEffect(Services.Clips.UIButtonPressed, 0.55f);
    }
}
