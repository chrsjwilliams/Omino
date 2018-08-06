using System;
using System.Collections;
using System.Collections.Generic;
using Beat;
using UnityEngine;

public static class HyperModeManager
{
	private static TaskManager _pulseTM, _discoTM;
	
	// Use this for initialization
	public static void StartGame()
	{
		Services.Clips = Resources.Load<ClipLibrary>("Audio/HyperLibrary");
		Services.Clock.SetBPM(110);
		Services.AudioManager.RegisterStartLevelMusic();
		
		_InitializePulse();
		_InitializeDisco();
		Services.GameManager.MainCamera.backgroundColor =
			Color.Lerp(Color.black, Services.GameScene.backgroundColor,
				Services.Clock.BeatLength() - (float) Services.Clock.AtNextBeat() / Services.Clock.BeatLength());
	}

	// Update is called once per frame
	public static void Update()
	{
		_pulseTM.Update();
		_discoTM.Update();
	}

	public static void Exit()
	{
		Services.Clips = Resources.Load<ClipLibrary>("Audio/ClipLibrary");
		Services.Clock.SetBPM(110);
		Services.AudioManager.RegisterStartLevelMusic();
	}

	private static void _InitializePulse()
	{
		_pulseTM = new TaskManager();
		ContinuePulse();
	}

	private static void _InitializeDisco()
	{
		_discoTM = new TaskManager();
		ContinueDisco();
	}

	private static void ContinuePulse()
	{
		Services.Clock.SyncFunction(() =>
		{
			TaskTree beatTasks =
				new TaskTree(new EmptyTask(),
					new TaskTree(
						new Pulse(Services.Clock.EighthLength())), new TaskTree(new Shake(Services.Clock.EighthLength())));
			
			ActionTask redo = new ActionTask(ContinuePulse);

			beatTasks.Then(redo);
			
			_pulseTM.Do(beatTasks);
		}, Clock.BeatValue.Quarter);
	}
	
	private static void ContinueDisco()
	{
		Services.Clock.SyncFunction(() =>
		{
			TaskTree beatTasks =
				new TaskTree(new EmptyTask(),
					new TaskTree(
						new DiscoFloor(Services.Clock.EighthLength())));
			
			ActionTask redo = new ActionTask(ContinueDisco);

			beatTasks.Then(redo);
			
			_pulseTM.Do(beatTasks);
		}, Clock.BeatValue.Quarter);
	}
}

public class Pulse : Task
{
	private Color pulse1 = new Color(0.28f, 0.28f, 0.28f);
	private Color pulse2 = new Color(0.15f, 0.15f , 0.15f);
	private float timeElapsed = 0;
	private float duration;

	public Pulse(float duration_in)
	{
		duration = duration_in;
	}
	
	protected override void Init()
	{
		
	}

	internal override void Update()
	{
		timeElapsed += Time.deltaTime;

		Services.GameManager.MainCamera.backgroundColor = Color.Lerp(pulse1, pulse2, timeElapsed / duration);

		if (timeElapsed >= duration) SetStatus(TaskStatus.Success);
	}
}

public class Shake : Task
{
	private float shakeDur, shakeMag, shakeSpeed;

	public Shake(float duration, float magnitude = 0.2f, float speed = 40.0f)
	{
		shakeDur = duration;
		shakeMag = magnitude;
		shakeSpeed = speed;
	}

	protected override void Init()
	{
		Services.CameraController.StartShake(shakeDur, shakeSpeed, shakeMag);
		SetStatus(TaskStatus.Success);
	}
}

public class DiscoFloor : Task
{
	private Color pulse1 = new Color(1f, 1f, 1f, 1f);
	private Color pulse2 = new Color(0f, 0f, 0f, 0f);
	
	private float timeElapsed = 0;
	private float duration;

	public DiscoFloor(float duration_in)
	{
		duration = duration_in;
		for (int i = 0; i < Services.MapManager.MapWidth; i++)
		{
			for (int j = 0; j < Services.MapManager.MapHeight; j++)
			{
				Tile t = Services.MapManager.GetTile(i, j);
				switch (UnityEngine.Random.Range(0, 4))
				{
					case 0 :
						t.SetBpAssistColor(new Color(1, 0, 1, 1f));
						break;
					case 1 :
						t.SetBpAssistColor(new Color(1, 0.92f, 0.016f, 1f));
						break;
					case 2 :
						t.SetBpAssistColor(new Color(1, 1f, 1f, 1f));
						break;
					case 3 :
						t.SetBpAssistColor(new Color(0f, 1f, 1f, 1f));
						break;
					default :
						t.SetColor(Color.grey);
						t.SetBpAssistAlpha(0.0f);
						break;
				}
			}
		}
	}

	internal override void Update()
	{
		timeElapsed += Time.deltaTime;
		
		for (int i = 0; i < Services.MapManager.MapWidth; i++)
		{
			for (int j = 0; j < Services.MapManager.MapHeight; j++)
			{
				Tile t = Services.MapManager.GetTile(i, j);
				t.SetBpAssistAlpha(Mathf.Lerp(1.0f, 0.6f, timeElapsed / duration));

			}
		}
		
		if (timeElapsed >= duration) SetStatus(TaskStatus.Success);
	}
}

