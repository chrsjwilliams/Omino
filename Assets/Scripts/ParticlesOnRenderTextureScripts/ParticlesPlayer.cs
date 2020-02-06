using UnityEngine;

public class ParticlesPlayer : MonoBehaviour 
{
	public ParticleSystem particles;

	public void ShowParticles(GameObject particlesPrefab)
	{
        particles = Instantiate(particlesPrefab, transform).GetComponent<ParticleSystem>();
        particles.Play();
	}

	public void StartContinuousEmission()
	{
		particles.loop = true;
		particles.Play();
	}

	public void StopEmission()
	{
		particles.loop = false;
		particles.Stop();
	}
}
