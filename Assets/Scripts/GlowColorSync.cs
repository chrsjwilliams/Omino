
using UnityEngine;
using SpriteGlowSystem;

public class GlowColorSync : MonoBehaviour {

    private SpriteGlow spriteGlow;
    private SpriteRenderer sr;
	// Use this for initialization
	void Start () {
        spriteGlow = GetComponent<SpriteGlow>();
        sr = GetComponent<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
        if (spriteGlow.GlowColor != sr.color)
            spriteGlow.GlowColor = sr.color;
	}
}
