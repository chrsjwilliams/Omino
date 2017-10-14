using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITabs : MonoBehaviour
{
    [SerializeField] private Player player;

	// Use this for initialization
	void Start ()
    {
	   if(name.Contains("P1"))
        {
            player = Services.GameManager.Player[0];
        }
       else
        {
            player = Services.GameManager.Player[1];
        }
	}

    public void ToggleHandZoneView(bool isViewable)
    {
        player.ToggleHandZoneView(isViewable);
    }
}
