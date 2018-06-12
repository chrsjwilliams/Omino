using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClipSwitcher : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			Services.Clips = Resources.Load<ClipLibrary>("Audio/PreIncubatorClipLibrary");
		}
		else if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			Services.Clips = Resources.Load<ClipLibrary>("Audio/MaterialClipLibrary");
		}
		else if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			Services.Clips = Resources.Load<ClipLibrary>("Audio/SynthClipLibrary");
		}
	}
}
