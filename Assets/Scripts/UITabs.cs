using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITabs : MonoBehaviour
{
    [SerializeField] private Player player;
    private Image handZoneUI;
    [SerializeField]
    private Image handTab;
    [SerializeField]
    private Image blueprintTab;

    // Use this for initialization
    void Start ()
    {
	   if(name.Contains("P1"))
        {
            player = Services.GameManager.Players[0];
        }
       else
        {
            player = Services.GameManager.Players[1];
        }
        player.InitializeUITabs(this);
        handZoneUI = GetComponent<Image>();
	}

    public void ToggleHandZoneView(bool isViewable)
    {
        player.ToggleHandZoneView(isViewable);
        if(isViewable)
        {
            handZoneUI.color = handTab.color;
        }
        else
        {
            handZoneUI.color = blueprintTab.color;
        }
    }
}
