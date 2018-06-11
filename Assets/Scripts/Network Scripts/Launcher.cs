using UnityEngine;
using WebSocketSharp;
using UnityEngine.SocialPlatforms.GameCenter;

namespace OminoNetwork
{
    public class Launcher : Photon.PunBehaviour
    {
        #region Public Variables

        public PhotonLogLevel Loglevel = PhotonLogLevel.Informational;
        public byte MaxPlayersPerRoom = 2;

        #endregion

        #region Private Variables

        string _gameVersion = "1.6";

        #endregion

        #region MonoBehaviour CallBacks

        void Awake()
        {
            PhotonNetwork.autoJoinLobby = false;
            PhotonNetwork.automaticallySyncScene = true;
            PhotonNetwork.logLevel = Loglevel;
        }

        #endregion

        #region Public Methods


        /// <summary>
        /// Start the connection process. 
        /// - If already connected, we attempt joining a random room
        /// - if not yet connected, Connect this application instance to Photon Cloud Network
        /// </summary>
        public void Connect()
        {


            // we check if we are connected or not, we join if we are , else we initiate the connection to the server.
            if (PhotonNetwork.connected)
            {
                // #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnPhotonRandomJoinFailed() and we'll create one.
                PhotonNetwork.JoinRandomRoom();
            }
            else
            {
                // #Critical, we must first and foremost connect to Photon Online Server.
                PhotonNetwork.ConnectUsingSettings(_gameVersion);
            }
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
        }

        public override void OnDisconnectedFromPhoton()
        {
            Debug.LogWarning("Omino/Launcher: OnDisconnectedFromPhoton() was called by PUN");        
        }

        #endregion
    }
}