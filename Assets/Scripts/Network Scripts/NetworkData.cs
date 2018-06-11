using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using WebSocketSharp;


public class NetworkData {
	
	#region Public Variables
	
	#endregion
	
	#region Static Definitions

	public readonly object READYFORMATCH = "ready_for_match";
	
	#endregion
	
	#region Private Variables

	private ExitGames.Client.Photon.Hashtable player_state;
	private PhotonPlayer opponent;
	private string titleScreenMessage = "";
	
	#endregion
	
	#region Constructors
	
	public NetworkData()
	{
		player_state = new ExitGames.Client.Photon.Hashtable();
		player_state[READYFORMATCH] = "false";
		PhotonNetwork.SetPlayerCustomProperties(player_state);
	}
	
	#endregion
	
	#region Public Functions

	public ExitGames.Client.Photon.Hashtable GetPlayerState()
	{
		return player_state;
	}

	public void ReadyForMatch(bool ready_check)
	{
		switch (ready_check)
		{
			case (true) :
				player_state[READYFORMATCH] = "true";
				break;
			case (false) :
				player_state[READYFORMATCH] = "false";
				break;
		}
		PhotonNetwork.SetPlayerCustomProperties(player_state);
	}

	public bool IsReadyForMatch()
	{
		switch (player_state[READYFORMATCH].ToString())
		{
			case ("true") :
				return true;
			case ("false") :
				return false;
		}
		return false;
	}

	public void ResetTitleScreenMessage()
	{
		titleScreenMessage = "";
	}

	public string GetTitleScreenMessage()
	{
		return titleScreenMessage;
	}

	public void SetTitleScreenMessage(string messageIn)
	{
		titleScreenMessage = messageIn;
	}
	
	public PhotonPlayer GetOpponent()
	{
		return opponent;
	}
	
	#endregion
}
