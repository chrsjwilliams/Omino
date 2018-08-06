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

	private static void _InitializeTasks()
	{
		_tm = new TaskManager();
		OngoingTasks();
	}

	private static void OngoingTasks()
	{
		Services.Clock.SyncFunction(() =>
		{
			Pulse pulsar = new Pulse(Services.Clock.QuarterLength() / 2);
			ActionTask redo = new ActionTask(OngoingTasks);

			pulsar.Then(redo);
			
			_tm.Do(pulsar);
		}, Clock.BeatValue.Quarter);
	}
}

public class Pulse : Task
{
	private Color pulse1 = new Color(0.28f, 0.28f, 0.28f);
	private Color pulse2 = new Color(0.1f, 0.1f , 0.1f);
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
