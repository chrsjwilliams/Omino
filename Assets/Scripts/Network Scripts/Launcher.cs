using System;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using WebSocketSharp;
using UnityEngine.SocialPlatforms.GameCenter;

namespace OminoNetwork
{
    public class Launcher : Photon.PunBehaviour
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

        #endregion

        #region Private Variables

        private string _gameVersion = "1.6";
        private TextMeshProUGUI statusText;

        #endregion

        #region MonoBehaviour CallBacks

        void Awake()
        {
            PhotonNetwork.autoJoinLobby = false;
            PhotonNetwork.automaticallySyncScene = true;
            PhotonNetwork.logLevel = Loglevel;
        }

        void Start()
        {
            statusText = statusTextGameObject.GetComponent<TextMeshProUGUI>();
            
            findButton.SetActive(true);
            startButton.SetActive(false);
            statusText.text = "Click \"Find\"\nto search for\na game.";
        }

        void Update()
        {
            if (PhotonNetwork.playerList.Length > 1)
                ReadyForGame();
        }

        #endregion

        #region Public Methods

        public void Connect()
        {
            findButton.SetActive(false);
            startButton.SetActive(false);
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

        public void ReadyForGame()
        {
            findButton.SetActive(false);
            startButton.SetActive(true);
            statusText.text = "Found Player!";
        }

        public void StartGame()
        {
            
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