
using UnityEngine;

public class DangerEffect : MonoBehaviour {

    [SerializeField]
    private float timeToGrow;
    private const float timeToShrink = 0.2f;
    private const float timeAtMax = 0.3f;
    private float timeElapsed;
    private bool particleStarted;
    //private ParticleSystem ps;

	// Use this for initialization
	void Start () {
        //ps = GetComponentInChildren<ParticleSystem>();
        timeElapsed = 0;
        transform.localScale = Vector3.zero;
        Services.AudioManager.RegisterSoundEffect(Services.Clips.Warning);
	}

    // Update is called once per frame
    void Update()
    {
        timeElapsed += Time.deltaTime;
        transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one,
            EasingEquations.Easing.QuadEaseOut(timeElapsed / timeToGrow));
        if(timeElapsed >= timeToGrow + timeAtMax - timeToShrink)
        {
            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero,
                EasingEquations.Easing.QuadEaseIn(
                    (timeElapsed - (timeToGrow + timeAtMax - timeToShrink)) 
                    / timeToShrink));
        }
        //if (!particleStarted && timeElapsed >= timeToGrow)
        //{
        //    ps.Play();
        //    particleStarted = true;
        //}
        if (timeElapsed >= timeToGrow + timeAtMax + timeToShrink)
        {
            Destroy(gameObject);
        }
    }
}
