using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.GameCenter;

namespace OminoNetwork
{
    public class GameCenterAuthenticator : MonoBehaviour
    {
        void Awake()
        {
            Services.GameCenter = new GameCenterPlatform();
            Services.GameCenter.localUser.Authenticate(ProcessAuthentication);
        }

        void ProcessAuthentication(bool success)
        {
            if (success)
                Debug.Log ("Authenticated.");
            
            else
                Debug.Log ("Failed to authenticate");
        }
    }
}