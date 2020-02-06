
using UnityEngine;

public class TouchEffect : MonoBehaviour {

    private float duration = 5;
    private float timeElapsed;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        timeElapsed += Time.deltaTime;

        if(timeElapsed > duration)
        {
            Destroy(gameObject);
        }
        
	}
}
