using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NetworkGameOverController : MonoBehaviour {

	#region Public Variables
	
	[Tooltip("This is the gameobject that holds the status message.")]
	public GameObject statusMessageGameObject;
	
	#endregion
	
	#region Private Variables

	private TextMeshProUGUI statusMessage;
	
	#endregion
	
	// Use this for initialization
	void Start ()
	{
		statusMessage = statusMessageGameObject.GetComponent<TextMeshProUGUI>();

		string message = Services.NetData.GetGameOverMessage();

		if (message == "")
		{
			Pop();
		}
		else
		{
			statusMessage.text = message;
			Services.NetData.ResetGameOverMessage();
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void Pop()
	{
		Services.Scenes.PopScene();
	}
}
