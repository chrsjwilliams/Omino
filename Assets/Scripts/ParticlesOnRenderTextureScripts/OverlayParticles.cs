using UnityEngine;

public static class OverlayParticles 
{
	private static ParticlesPlayer player;
	private static ParticlesDisplayer displayer;

	public static void IntializeCheck()
	{
		if(player == null)
		{
			player = GameObject.FindObjectOfType<ParticlesPlayer>();
		}

		if(displayer == null)
		{
			displayer = GameObject.FindObjectOfType<ParticlesDisplayer>();
		}
	}

	public static void ShowParticles(GameObject particlePrefab)
	{
		IntializeCheck();

		displayer.ResetPosition();
		player.ShowParticles(particlePrefab);
	}

	public static void ShowParticles(GameObject particlePrefab, Vector3 pos)
	{
		IntializeCheck();

		displayer.MoveToPosition(pos);
		player.ShowParticles(particlePrefab);
	}

	public static void ShowContinuousParticles()
	{
		IntializeCheck();

		displayer.ResetPosition();
		player.StartContinuousEmission();
	}


	public static void ShowContinuousParticles(Vector3 pos)
	{
		IntializeCheck();

		displayer.MoveToPosition(pos);
		player.StartContinuousEmission();
	}

	public static void StopContinuousParticles()
	{
		IntializeCheck();

		player.StopEmission();
		displayer.ResetPosition();
	}
}
