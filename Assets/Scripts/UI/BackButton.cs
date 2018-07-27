using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackButton : MonoBehaviour
{

    TaskManager _tm = new TaskManager();

	// Use this for initialization
	void Start () {
		
	}

    public void PopScene()
    {
        Services.Scenes.PopScene();
    }

    public void PlayExitTransition()
    {
        Services.Scenes.CurrentScene.ExitTransition();
    }

    public void Back()
    {
        if(Services.GameManager.mode == TitleSceneScript.GameMode.NONE ||
            Services.Scenes.CurrentScene is GameOptionsSceneScript)
        {
            Services.GameManager.mode = TitleSceneScript.GameMode.NONE;
            Services.Scenes.Swap<TitleSceneScript>();
        }
        else
        {
            List<Task> backButtonTasks = new List<Task>();
            backButtonTasks.Add(new ActionTask(PlayExitTransition));
            backButtonTasks.Add(new ActionTask(PopScene));

            TaskQueue backButtonTaskQueue = new TaskQueue(backButtonTasks);

            _tm.Do(backButtonTaskQueue);
        }

        // Pops scene off the stack

        //if (optionsMenuActive)
        //{
        //    ToggleOptionMenu();
        //    if (Services.GameManager.mode == TitleSceneScript.GameMode.TwoPlayers ||
        //                Services.GameManager.mode == TitleSceneScript.GameMode.PlayerVsAI ||
        //                Services.GameManager.mode == TitleSceneScript.GameMode.Demo)
        //    {
        //        SlideInLevelButtons();
        //    }
        //    else
        //    {
        //        SlideOutOptionsButton(false);
        //    }
        //    if (Services.GameManager.mode == TitleSceneScript.GameMode.Elo)
        //    {
        //        eloUI.gameObject.SetActive(true);
        //    }
        //}
        //else
        //{
        //    Services.Scenes.Swap<TitleSceneScript>();
        //}
    }

    public void UIClick()
    {
        Services.AudioManager.PlaySoundEffect(Services.Clips.UIClick, 0.55f);
    }

    public void UIButtonPressedSound()
    {
        Services.AudioManager.PlaySoundEffect(Services.Clips.UIButtonPressed, 0.55f);
    }

    // Update is called once per frame
    void Update () {
        _tm.Update();
	}
}
