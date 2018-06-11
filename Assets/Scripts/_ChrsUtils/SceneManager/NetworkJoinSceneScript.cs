using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class NetworkJoinSceneScript : Scene<TransitionData> {

	internal override void OnEnter(TransitionData data)
	{
	    Services.NetData = new NetworkData();
	}

    internal override void OnExit()
    {
        
    }
}
