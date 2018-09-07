using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DungeonRunInGameUIManager : MonoBehaviour {

    public bool inPosition { get; private set; }

    public GameObject menu;
    [SerializeField]
    private GameObject defeatSymbol;
    [SerializeField]
    private GameObject victorySymbol;
    [SerializeField]
    private TextMeshProUGUI victoryText;
    [SerializeField]
    private TextMeshProUGUI dungeonRunChangeText;
    [SerializeField]
    private TextMeshProUGUI nextChallengeButtonText;

    [SerializeField]
    private Image[] progressNodes;

    private float progressBarFill;

    private const float offset = 1800;
    private float timeElapsed;
    private const float dropDownDuration = 0.5f;
    private bool dropping;
    private Vector3 basePos;

    private int challengeIndex;

    // Use this for initialization
    void Start()
    {
        inPosition = false;
        menu.SetActive(false);
        basePos = menu.transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (dropping) Drop();
    }

    private void SetFilledProgressNodes(int challengeNum)
    {
        if (challengeNum == 1) return;
        for (int i = 2; i < challengeNum; i++)
        {
            progressNodes[i - 2].fillAmount = 1;
        }
    }

    private void Drop()
    {
        timeElapsed += Time.deltaTime;
        menu.transform.localPosition = Vector3.Lerp(
            (Vector3.up * offset) + basePos,
            basePos, EasingEquations.Easing.QuadEaseOut(timeElapsed / dropDownDuration));
        if (timeElapsed >= dropDownDuration)
        {
            Services.GeneralTaskManager.Do(new LERPProgressBar(progressNodes[challengeIndex], progressBarFill, 1.0f));
            inPosition = true;
            dropping = false;
            menu.transform.localPosition = basePos;
        }
    }

    public void OnGameEnd(bool victory)
    {
        dropping = true;
        menu.transform.localPosition = basePos + (offset * Vector3.up);
        menu.SetActive(true);
        defeatSymbol.SetActive(!victory);
        victorySymbol.SetActive(victory);
        challengeIndex = DungeonRunManager.dungeonRunData.challengeNum - 1;
        SetFilledProgressNodes(DungeonRunManager.dungeonRunData.challengeNum + 1);
        if (victory)
        {
            victoryText.text = "victory";
            progressBarFill = 1;
            if(DungeonRunManager.dungeonRunData.challengeNum == DungeonRunManager.MAX_DUNGEON_CHALLENGES)
            {
                nextChallengeButtonText.text = "complete";
            }
            else
            {
                nextChallengeButtonText.text = "next challenge";
            }
        }
        else
        {
            progressBarFill = 0;
            victoryText.text = "defeat";
            nextChallengeButtonText.text = "try again";
        }

        dungeonRunChangeText.text = "challenge " + DungeonRunManager.dungeonRunData.challengeNum +
                                "/" + DungeonRunManager.MAX_DUNGEON_CHALLENGES;

       
    }
}
