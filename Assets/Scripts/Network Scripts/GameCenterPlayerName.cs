using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SocialPlatforms.GameCenter;

namespace OminoNetwork
{
	public class GameCenterPlayerName : MonoBehaviour
	{
		#region Private Variables
		
		static string playerNamePrefKey = "PlayerName";
		private string player_name = "";

		#endregion

		#region MonoBehaviour CallBacks

		void Start () {
			string defaultName = "Player " + Random.Range(0, 100);
			
			/*if (Services.GameCenter.localUser.userName != null)
			{
				defaultName = Services.GameCenter.localUser.userName;
			} */

			PhotonNetwork.playerName =  defaultName;
		}

		#endregion

		#region Public Methods

		#endregion
	}
}