
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour {

    [SerializeField]
    private Image resumeButton;
    [SerializeField]
    private Image replayButton;
    [SerializeField]
    private Color defaultColor;
    [SerializeField]
    private Color highlightedColor;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetPauseMenuStatus(bool status)
    {
        gameObject.SetActive(status);
        if (status)
        {
            if (Services.GameScene.gameOver)
            {
                resumeButton.color = defaultColor;
                replayButton.color = highlightedColor;
            }
            else
            {
                resumeButton.color = highlightedColor;
                if(replayButton)
                    replayButton.color = defaultColor;
            }
        }
    }
}
