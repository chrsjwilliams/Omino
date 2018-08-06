using System;
using System.Collections;
using System.Collections.Generic;
using Beat;
using UnityEngine;

public static class HyperModeManager
{
	private static TaskManager _tm;
	
	// Use this for initialization
	public static void StartGame()
	{
		Services.Clips = Resources.Load<ClipLibrary>("Audio/HyperLibrary");
		Services.Clock.SetBPM(110);
		Services.AudioManager.RegisterStartLevelMusic();
		
		_InitializeTasks();
		Services.GameManager.MainCamera.backgroundColor =
			Color.Lerp(Color.black, Services.GameScene.backgroundColor,
				Services.Clock.BeatLength() - (float) Services.Clock.AtNextBeat() / Services.Clock.BeatLength());
	}

	// Update is called once per frame
	public static void Update()
	{
		_tm.Update();
	}

	public static void Exit()
	{
		Services.Clips = Resources.Load<ClipLibrary>("Audio/ClipLibrary");
		Services.Clock.SetBPM(110);
		Services.AudioManager.RegisterStartLevelMusic();
	}

	private static void _InitializeTasks()
	{
		_tm = new TaskManager();
		OngoingTasks();
	}

	private static void OngoingTasks()
	{
		Services.Clock.SyncFunction(() =>
		{
			TaskTree beatTasks =
				new TaskTree(new EmptyTask(),
					new TaskTree(
						new Pulse(Services.Clock.EighthLength())), new TaskTree(new Shake(Services.Clock.EighthLength())));
			
			ActionTask redo = new ActionTask(OngoingTasks);

			beatTasks.Then(redo);
			
			_tm.Do(beatTasks);
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

