using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DungeonRunInGameUIManager : MonoBehaviour {

    [SerializeField]
    private GameObject menu;
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

    private const float offset = 1800;
    private float timeElapsed;
    private const float dropDownDuration = 0.5f;
    private bool dropping;
    private Vector3 basePos;

    // Use this for initialization
    void Start()
    {
        menu.SetActive(false);
        basePos = menu.transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (dropping) Drop();
    }

    private void Drop()
    {
        timeElapsed += Time.deltaTime;
        menu.transform.localPosition = Vector3.Lerp(
            (Vector3.up * offset) + basePos,
            basePos, EasingEquations.Easing.QuadEaseOut(timeElapsed / dropDownDuration));
        if (timeElapsed >= dropDownDuration)
        {
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

        if(victory)
        {
            victoryText.text = "Victory";
            
            if(DungeonRunManager.dungeonRunData.challenegeNum == DungeonRunManager.MAX_DUNGEON_CHALLENGES)
            {
                nextChallengeButtonText.text = "Complete";
            }
            else
            {
                nextChallengeButtonText.text = "Next Challenge";
            }
        }
        else
        {
            victoryText.text = "Defeat";
            nextChallengeButtonText.text = "Try Again";
        }

        dungeonRunChangeText.text = "Challenge " + DungeonRunManager.dungeonRunData.challenegeNum +
                                "/" + DungeonRunManager.MAX_DUNGEON_CHALLENGES;
    }
}
