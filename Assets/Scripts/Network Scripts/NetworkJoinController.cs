using System;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using WebSocketSharp;
using UnityEngine.SocialPlatforms.GameCenter;

namespace OminoNetwork
{
    public class NetworkJoinController : Photon.PunBehaviour
    {
        #region Public Variables

        public PhotonLogLevel Loglevel = PhotonLogLevel.Informational;
        public byte MaxPlayersPerRoom = 2;

        [Tooltip("The button to start searching for a game, with the \"Start Game\" Button.")]
        public GameObject findButton;
        [Tooltip("The button once both are connected, with the \"Start Game\" Button.")]
        public GameObject startButton;
        [Tooltip("The UI Label to inform the user of the status.")]
        public GameObject statusTextGameObject;
        [Tooltip("The game object holding the player text mesh pro.")]
        public GameObject playerNameBoxGameObject;
        [Tooltip("The game object with the input field for player name.")]
        public GameObject playerNameInputFieldGameObject;

        #endregion
        
        #region Static Definitions
        
        static string playerNamePrefKey = "PlayerName";
        
        #endregion

        #region Private Variables

        private string _gameVersion = "1.6";
        private TextMeshProUGUI statusText;
        private TextMeshProUGUI playerNameBox;
        private string player_name = "";

        #endregion

        #region MonoBehaviour CallBacks

        void Awake()
        {
            PhotonNetwork.autoJoinLobby = false;
            PhotonNetwork.automaticallySyncScene = true;
            PhotonNetwork.logLevel = Loglevel;
            Services.NetData = new NetworkData();
        }

        void Start()
        {
            statusText = statusTextGameObject.GetComponent<TextMeshProUGUI>();
            playerNameBox = playerNameBoxGameObject.GetComponentInChildren<TextMeshProUGUI>();
            
            findButton.SetActive(true);
            startButton.SetActive(false);
            statusText.text = "Set Name\nThen\nClick \"Find\"";
            
            if (PlayerPrefs.HasKey(playerNamePrefKey))
                playerNameBox.text = PlayerPrefs.GetString(playerNamePrefKey);
            else
                playerNameBox.text = "Type Name Here";
        }

        void Update()
        {
            if (PhotonNetwork.playerList.Length > 1)
            {
                if (!Services.NetData.IsReadyForMatch())
                {
                    ReadyForGame();
                }
                else
                {
                    Debug.Log("I am ready.");
                    PhotonPlayer opponent = PhotonNetwork.otherPlayers[0];
                    object value;
                    if (opponent.CustomProperties.TryGetValue(Services.NetData.READYFORMATCH, out value))
                    {
                        Debug.Log("Got here.");
                        if (value.ToString() == "false")
                        {
                            Debug.Log("Opponent not ready.");
                            WaitingForOpponent();
                        }
                        else
                        {
                            if (PhotonNetwork.isMasterClient)
                            {
                                LoadNetworkGame();
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Public Methods

        public void Connect()
        {
            PhotonNetwork.playerName = playerNameBox.text;
            
            PlayerPrefs.SetString(playerNamePrefKey, playerNameBox.text);
            PlayerPrefs.Save();
            
            playerNameBoxGameObject.SetActive(false);
            findButton.SetActive(false);
            startButton.SetActive(false);
            playerNameInputFieldGameObject.SetActive(false);
            statusText.text = "Connecting...";
            
            if (PhotonNetwork.connected)
            {
                PhotonNetwork.JoinRandomRoom();
            }
            else
            {
                PhotonNetwork.ConnectUsingSettings(_gameVersion);
            }
        }

        public void LoadNetworkGame()
        {
            PhotonNetwork.LoadLevel("_NetworkedScene");
        }

        public void ClickedReady()
        {
            Services.NetData.ReadyForMatch(true);
        }

        public void ReadyForGame()
        {
            findButton.SetActive(false);
            startButton.SetActive(true);
            statusText.text = "Found Player\n" + PhotonNetwork.otherPlayers[0].NickName + ".";
        }

        public void WaitingForOpponent()
        {
            findButton.SetActive(false);
            startButton.SetActive(false);
            statusText.text = "Waiting for\n" + PhotonNetwork.otherPlayers[0].NickName + ".";
        }

        public void Pop()
        {
            Services.Scenes.PopScene();
        }

    #endregion
        
        #region Photon.PunBehaviour CallBacks

        public override void OnConnectedToMaster()
        {
            Debug.Log("Omino/Launcher: OnConnectedToMaster() was called by PUN");
            PhotonNetwork.JoinRandomRoom();
        }
        
        public override void OnPhotonRandomJoinFailed (object[] codeAndMsg)
        {
            Debug.Log("Omino/Launcher:OnPhotonRandomJoinFailed() was called by PUN. No random room available, so we'll create one.\nCalling: PhotonNetwork.CreateRoom(null, new RoomOptions() {maxPlayers = 2}, null);");
            PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = MaxPlayersPerRoom }, null);
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("DemoAnimator/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");
            findButton.SetActive(false);
            startButton.SetActive(false);
            statusText.text = "Connected!\n\nSearching for\nopponent...";
        }

        public override void OnDisconnectedFromPhoton()
        {
            Debug.LogWarning("Omino/Launcher: OnDisconnectedFromPhoton() was called by PUN");       
            
            findButton.SetActive(true);
            startButton.SetActive(false);
            statusText.text = "Disconnected...";
        }

        private void OnPlayerDisconnected(NetworkPlayer player)
        {
            findButton.SetActive(false);
            startButton.SetActive(false);
            statusText.text = "Opponent\nDisconnected.";
        }

        #endregion
    }
}