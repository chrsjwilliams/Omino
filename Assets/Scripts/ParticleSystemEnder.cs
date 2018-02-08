using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemEnder : MonoBehaviour {

    private ParticleSystem ps;
    private float lifetime;

	// Use this for initialization
	void Start () {
        ps = GetComponent<ParticleSystem>();
        lifetime = ps.main.startLifetime.constantMax;
        Destroy(gameObject, lifetime);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
