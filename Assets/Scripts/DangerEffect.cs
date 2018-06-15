using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DangerEffect : MonoBehaviour {

    [SerializeField]
    private float timeToGrow;
    private float timeElapsed;
    private bool particleStarted;
    private ParticleSystem ps;

	// Use this for initialization
	void Start () {
        ps = GetComponentInChildren<ParticleSystem>();
        timeElapsed = 0;
        transform.localScale = Vector3.zero;
        Services.AudioManager.PlaySoundEffect(Services.Clips.Warning, 1);
	}

    // Update is called once per frame
    void Update()
    {
        timeElapsed += Time.deltaTime;
        transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one,
            EasingEquations.Easing.QuadEaseOut(timeElapsed / timeToGrow));
        //if (!particleStarted && timeElapsed >= timeToGrow)
        //{
        //    ps.Play();
        //    particleStarted = true;
        //}
        if (timeElapsed >= timeToGrow + ps.main.duration)
        {
            Destroy(gameObject);
        }
    }
}
