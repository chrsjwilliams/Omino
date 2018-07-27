using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AIDifficultySceneScript : Scene<TransitionData>
{
    public bool[] humanPlayers { get; private set; }

    [SerializeField]
    private GameObject[] aiLevelButtonZones;
    private Button[][] aiLevelButtons;
    private TextMeshProUGUI[] aiLevelTexts;
    private TaskManager _tm = new TaskManager();

    [SerializeField]
    private HandicapSystem handicapSystem;

    [SerializeField]
    private GameObject backButton;
    [SerializeField]
    private GameObject optionButton;

    [SerializeField]
    private GameObject handicapOptions;

    // Use this for initialization
    void Start()
    {

    }


    internal override void OnEnter(TransitionData data)
    {
        aiLevelTexts = new TextMeshProUGUI[1];
        aiLevelButtons = new Button[1][];

        for (int i = 0; i < aiLevelButtonZones.Length; i++)
        {
            Button[] buttons = aiLevelButtonZones[i].GetComponentsInChildren<Button>();
            aiLevelButtons[i] = new Button[buttons.Length];
            for (int j = 0; j < buttons.Length; j++)
            {
                aiLevelButtons[i][j] = buttons[j];
            }
            aiLevelTexts[i] = aiLevelButtonZones[i].GetComponentInChildren<TextMeshProUGUI>();
            aiLevelButtonZones[i].SetActive(false);
        }

        TaskTree aiLevelSelect = new TaskTree(new EmptyTask(),
            new TaskTree(new AILevelSlideIn(aiLevelTexts[0], aiLevelButtons[0], true, false)),
            new TaskTree(new LevelSelectTextEntrance(backButton, true)),
            new TaskTree(new LevelSelectTextEntrance(optionButton)),
            new TaskTree(new LevelSelectTextEntrance(handicapOptions)));
        _tm.Do(aiLevelSelect);

        handicapSystem.UpdateHandicapText();
    }

    internal override void OnExit()
    {


    }

    internal override void ExitTransition()
    {
        TaskTree aiLevelSelectExit = new TaskTree(new EmptyTask(),
            new TaskTree(new AILevelSlideIn(aiLevelTexts[0],
            aiLevelButtons[0], true, true),
            new TaskTree(new LevelSelectTextEntrance(backButton, true, true)),
            new TaskTree(new LevelSelectTextEntrance(optionButton, false, true)),
            new TaskTree(new LevelSelectTextEntrance(handicapOptions, false, true))));
    }

    public void GoToMapSelectScene()
    {
        Services.Scenes.Swap<MapSelectSceneScript>();
    }

    private void SetAILevel(int playerNum, int level)
    {
        Services.GameManager.aiLevels[playerNum - 1] = AIPlayer.AiLevels[level];

        List<Task> continueToMapSelection = new List<Task>();
        continueToMapSelection.Add(new ActionTask(ExitTransition));
        continueToMapSelection.Add(new ActionTask(GoToMapSelectScene));

        _tm.Do(new TaskQueue(continueToMapSelection));
        
    }

    public void SetP1AILevel(int level)
    {
        SetAILevel(1, level);
    }

    public void SetP2AILevel(int level)
    {
        SetAILevel(2, level);
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
    void Update()
    {
        _tm.Update();
    }
}
