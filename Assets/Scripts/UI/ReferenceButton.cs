
using UnityEngine;

public class ReferenceButton : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void LoadReferenceScene()
    {
        Services.Scenes.PushScene<ReferenceSceneScript>();
    }
}
