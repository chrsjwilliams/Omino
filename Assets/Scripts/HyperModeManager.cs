using System;
using System.Collections;
using System.Collections.Generic;
using Beat;
using UnityEngine;

public static class HyperModeManager
{
	private static TaskManager _pulseTM, _discoTM;
	private static Color[][] _previousScheme;
	private static Color[,] _hyperModeColors;
	
	// Use this for initialization
	public static void StartGame()
	{
		Services.Clips = Resources.Load<ClipLibrary>("Audio/HyperLibrary");
		Services.Clock.SetBPM(110);
		Services.AudioManager.RegisterStartLevelMusic();
		
		PlayerHandicap[] handicaps = new PlayerHandicap[2];
		handicaps[0].SetEnergyHandicapLevel(2.5f);
		handicaps[0].SetHammerHandicapLevel(4f);
		handicaps[0].SetPieceHandicapLevel(2.5f);
		handicaps[1].SetEnergyHandicapLevel(2.5f);
		handicaps[1].SetHammerHandicapLevel(4f);
		handicaps[1].SetPieceHandicapLevel(2.5f);
                
		Services.GameManager.SetHandicapValues(handicaps);
		
		_previousScheme = Services.GameManager.GetColorScheme();

		_hyperModeColors = new Color[,]
		{
			{new Color(0.5f, 1, 0.5f, 1f), new Color(0.4f, 0.6f, 0.4f, 1f)},
			{new Color(1f, 0.2f, 0.5f, 1f), new Color(0.6f, 0.1f, 0.4f, 1f)},
			{new Color(0.5f, 0.5f, 1f, 1f), new Color(0.4f, 0.4f, 0.6f, 1f)},
			{new Color(.2f, 0.8f, 1f, 1f), new Color(.1f, 0.4f, 0.6f, 1f)},
			{new Color(0f, 0f, 0f, 1f), new Color(.3f, 0.3f, 0.3f, 1f)},
			{new Color(1, 0.92f, 0.016f, 1f), new Color(0.7f, 0.62f, 0f, 1f)},
			{new Color(1f, 0.2f, 0.7f, 1f), new Color(0.7f, 0.1f, 0.4f, 1f)},
			{new Color(1f, 0.5f, 0.0f, 1f), new Color(.6f, 0.2f, 0.0f, 1f)},
		};
		
		Color[][] colorScheme = new Color[2][];

		int x = 0;
		int y = 0;

		x = UnityEngine.Random.Range(0, _hyperModeColors.Length/2);
		y = UnityEngine.Random.Range(0, _hyperModeColors.Length/2);
		while (x == y)
		{
			y = UnityEngine.Random.Range(0, _hyperModeColors.Length);
		}

		colorScheme[0] = new Color[] { _hyperModeColors[x,0], _hyperModeColors[x,1] };
		colorScheme[1] = new Color[] { _hyperModeColors[y,0], _hyperModeColors[y,1] };
		 
		Services.GameManager.SetColorScheme(colorScheme); 
		
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
		Services.GameManager.SetColorScheme(_previousScheme);
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

	public static void Placement()
	{
		Services.CameraController.StartShake(Services.Clock.EighthLength(), 80f, 10.0f, true);
	}

	private static void ContinuePulse()
	{
		Services.Clock.SyncFunction(() =>
		{
			TaskTree beatTasks =
				new TaskTree(new EmptyTask(),
					new TaskTree(
						new Pulse(Services.Clock.EighthLength())), new TaskTree(new Shake(Services.Clock.SixteenthLength())));
			
			ActionTask redo = new ActionTask(ContinuePulse);

			beatTasks.Then(redo);
			
			_pulseTM.Do(beatTasks);
		}, Clock.BeatValue.Quarter);
	}
	
	private static void ContinueDisco()
	{
		Debug.Log("ContinueDisco");
		Services.Clock.SyncFunction(() =>
		{
			DiscoFloor beatTasks = new DiscoRandom();

			
			switch (UnityEngine.Random.Range(0, 5))
			{
				case (0) :
					beatTasks = new DiscoStripes();
					break;
				case (1) :
					beatTasks = new DiscoCheckers();
					break;
				case (2) :
					beatTasks = new DiscoWave();
					break;
				case (3) : 
					beatTasks = new DiscoBlocks();
					break;
				case (4) :
					beatTasks = new DiscoWindmill();
					break;
				default :
					beatTasks = new DiscoRandom();
					break;
			}
			
			TaskQueue to_do = new TaskQueue(new List<Task>(new Task[] { beatTasks, new ActionTask(ContinueDisco) }));
			
			_pulseTM.Do(to_do);
		});
	}
	
	public static void ConfettiSplosion(Color color, Vector3 location)
	{
		GameObject particles = GameObject.Instantiate(Resources.Load("Prefabs/Confetti/Placementfetti")) as GameObject;
		ParticleSystem ps = particles.GetComponent<ParticleSystem>();
		var main = ps.main;
		main.startColor = color;
		particles.transform.position = location;
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

		for (int i = 0; i < Services.MapManager.MapWidth; i++)
		{
			for (int j = 0; j < Services.MapManager.MapHeight; j++)
			{
				Tile t = Services.MapManager.GetTile(i, j);
				t.SetBpAssistAlpha(Mathf.Lerp(1.0f, 0.4f, (timeElapsed / duration)));
			}
		}
		
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
	public Color[] colors =
		{new Color(1, 0.92f, 0.016f, 1f), new Color(0f, 1f, 1f, 1f), new Color(1, 0, 1, 1f), new Color(1, 1f, 1f, 1f)};

	public DiscoFloor()
	{
		SetStatus(TaskStatus.Success);
	}
}

public class DiscoRandom : DiscoFloor
{
	private int num_switches = 0;
	private TaskManager _colorSwitcher;

	public DiscoRandom()
	{
		_colorSwitcher = new TaskManager();
		
		TaskQueue switchColors = new TaskQueue(new List<Task>(new Task[] {
			new ActionTask(_SetColors),
			new Wait(Services.Clock.BeatLength() / 2),
			new ActionTask(() => { Services.Clock.SyncFunction(_SetColors, Clock.BeatValue.Quarter); }),
			new Wait(Services.Clock.BeatLength()),
			new ActionTask(() => { Services.Clock.SyncFunction(_SetColors, Clock.BeatValue.Quarter); }),
			new Wait(Services.Clock.BeatLength()),
			new ActionTask(() => { Services.Clock.SyncFunction(_SetColors, Clock.BeatValue.Quarter); })
		}));
		
		_colorSwitcher.Do(switchColors);
	}

	internal override void Update()
	{
		if (num_switches >= 4) SetStatus(TaskStatus.Success);
	}

	private void _SetColors()
	{
		num_switches++;
		
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
}

public class DiscoCheckers : DiscoFloor
{
	private int num_switches = 0;
	private Color color1, color2;
	private TaskManager _colorSwitcher;
	
	private int[,] grid =
	{
		{0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1},
		{0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1},
		{1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0},
		{1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0},
		{0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1},
		{0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1},
		{1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0},
		{1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0},
		{0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1},
		{0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1},
		{1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0},
		{1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0},
		{0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1},
		{0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1},
		{1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0},
		{1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0},
		{0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1},
		{0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1},
		{1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0},
		{1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0},
	};

	public DiscoCheckers()
	{
		_colorSwitcher = new TaskManager();
		
		switch (UnityEngine.Random.Range(0, 4))
		{
			case 0 :
				color1 = new Color(1, 0, 1, 1f);
				color2 = new Color(0f, 1f, 1f, 1f);
				break;
			case 1 :
				color1 = new Color(1, 0.92f, 0.016f, 1f);
				color2 = new Color(1, 0, 1, 1f);
				break;
			case 2 :
				color1 = new Color(1, 1f, 1f, 1f);
				color2 = new Color(1, 0, 1, 1f);
				break;
			case 3 :
				color1 = new Color(0f, 1f, 1f, 1f);
				color2 = new Color(1, 0.92f, 0.016f, 1f);
				break;
			default :
				color1 = Color.white;
				color2 = Color.black;
				break;
		}

		TaskQueue switchColors = new TaskQueue(new List<Task>(new Task[] {
			new ActionTask(_SetColors),
			new Wait(Services.Clock.BeatLength() / 2),
			new ActionTask(() => { Services.Clock.SyncFunction(_SetColors, Clock.BeatValue.Quarter); }),
			new Wait(Services.Clock.BeatLength()),
			new ActionTask(() => { Services.Clock.SyncFunction(_SetColors, Clock.BeatValue.Quarter); }),
			new Wait(Services.Clock.BeatLength()),
			new ActionTask(() => { Services.Clock.SyncFunction(_SetColors, Clock.BeatValue.Quarter); })
			}));
		
		_colorSwitcher.Do(switchColors);
	}

	internal override void Update()
	{
		_colorSwitcher.Update();
		if (num_switches >= 4) SetStatus(TaskStatus.Success);
	}

	private void _SetColors()
	{
		num_switches++;
		
		for (int i = 0; i < Services.MapManager.MapWidth; i++)
		{
			for (int j = 0; j < Services.MapManager.MapHeight; j++)
			{
				Tile t = Services.MapManager.GetTile(i, j);
				if ((grid[i, j] + num_switches) % 2 == 0)
				{
					t.SetBpAssistColor(color1);
				}
				else
				{
					t.SetBpAssistColor(color2);
				}
			}
		}
	}
}

public class DiscoStripes : DiscoFloor
{
	private int num_switches = 0;
	private Color color1, color2, color3, color4;
	private TaskManager _colorSwitcher;

	private readonly int[,] grid;
	private readonly int[,] gridLR =
	{
		{0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3},
		{3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2},
		{2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1},
		{1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0},
		{0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3},
		{3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2},
		{2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1},
		{1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0},
		{0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3},
		{3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2},
		{2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1},
		{1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0},
		{0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3},
		{3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2},
		{2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1},
		{1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0},
		{0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3},
		{3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2},
		{2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1},
		{1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0},
	};
	private int[,] gridRL =
	{
		{0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3},
		{1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0},
		{2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1},
		{3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2},
		{0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3},
		{1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0},
		{2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1},
		{3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2},
		{0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3},
		{1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0},
		{2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1},
		{3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2},
		{0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3},
		{1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0},
		{2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1},
		{3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2},
		{0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3},
		{1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0},
		{2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1},
		{3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2},
	};

	public DiscoStripes()
	{
		_colorSwitcher = new TaskManager();

		grid = UnityEngine.Random.Range(0, 2) % 2 == 0 ? gridLR : gridRL;

		
		int offset = UnityEngine.Random.Range(0, 4);

		color1 = colors[offset % colors.Length];
		color2 = colors[(offset + 1) % colors.Length];
		color3 = colors[(offset + 2) % colors.Length];
		color4 = colors[(offset + 3) % colors.Length];

		TaskQueue switchColors = new TaskQueue(new List<Task>(new Task[] {
			new ActionTask(_SetColors),
			new Wait(Services.Clock.BeatLength() / 2),
			new ActionTask(() => { Services.Clock.SyncFunction(_SetColors, Clock.BeatValue.Quarter); }),
			new Wait(Services.Clock.BeatLength()),
			new ActionTask(() => { Services.Clock.SyncFunction(_SetColors, Clock.BeatValue.Quarter); }),
			new Wait(Services.Clock.BeatLength()),
			new ActionTask(() => { Services.Clock.SyncFunction(_SetColors, Clock.BeatValue.Quarter); })
			}));
		
		_colorSwitcher.Do(switchColors);
	}

	internal override void Update()
	{
		_colorSwitcher.Update();
		
		if (num_switches >= 4) SetStatus(TaskStatus.Success);
	}

	private void _SetColors()
	{
		num_switches++;
		
		for (int i = 0; i < Services.MapManager.MapWidth; i++)
		{
			for (int j = 0; j < Services.MapManager.MapHeight; j++)
			{
				Tile t = Services.MapManager.GetTile(i, j);
				if ((grid[i, j] + num_switches) % 4 == 0)
				{
					t.SetBpAssistColor(color1);
				}
				else if ((grid[i, j] + num_switches) % 4 == 1)
				{
					t.SetBpAssistColor(color2);
				}
				else if ((grid[i, j] + num_switches) % 4 == 2)
				{
					t.SetBpAssistColor(color3);
				}
				else 
				{
					t.SetBpAssistColor(color4);
				}
			}
		}
	}
}

public class DiscoWave : DiscoFloor
{
	private int num_switches = 0;
	private Color color1, color2, color3, color4;
	private TaskManager _colorSwitcher;

	private readonly int[,] grid =
	{
		{0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 1, 0, 3, 2, 1, 0, 3, 2, 1, 0},
		{1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 2, 1, 0, 3, 2, 1, 0, 3, 2, 1},
		{2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 3, 2, 1, 0, 3, 2, 1, 0, 3, 2},
		{3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 0, 3, 2, 1, 0, 3, 2, 1, 0, 3},
		{0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 1, 0, 3, 2, 1, 0, 3, 2, 1, 0},
		{1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 2, 1, 0, 3, 2, 1, 0, 3, 2, 1},
		{2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 3, 2, 1, 0, 3, 2, 1, 0, 3, 2},
		{3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 0, 3, 2, 1, 0, 3, 2, 1, 0, 3},
		{0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 1, 0, 3, 2, 1, 0, 3, 2, 1, 0},
		{1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 2, 1, 0, 3, 2, 1, 0, 3, 2, 1},
		{1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 2, 1, 0, 3, 2, 1, 0, 3, 2, 1},
		{0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 1, 0, 3, 2, 1, 0, 3, 2, 1, 0},
		{3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 0, 3, 2, 1, 0, 3, 2, 1, 0, 3},
		{2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 3, 2, 1, 0, 3, 2, 1, 0, 3, 2},
		{1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 2, 1, 0, 3, 2, 1, 0, 3, 2, 1},
		{0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 1, 0, 3, 2, 1, 0, 3, 2, 1, 0},
		{3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 0, 3, 2, 1, 0, 3, 2, 1, 0, 3},
		{2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 3, 2, 1, 0, 3, 2, 1, 0, 3, 2},
		{1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 2, 1, 0, 3, 2, 1, 0, 3, 2, 1},
		{0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 1, 0, 3, 2, 1, 0, 3, 2, 1, 0},
	};

	public DiscoWave()
	{
		_colorSwitcher = new TaskManager();

		int offset = UnityEngine.Random.Range(0, 4);

		color1 = colors[offset % colors.Length];
		color2 = colors[(offset + 1) % colors.Length];
		color3 = colors[(offset + 2) % colors.Length];
		color4 = colors[(offset + 3) % colors.Length];

		TaskQueue switchColors = new TaskQueue(new List<Task>(new Task[] {
			new ActionTask(_SetColors),
			new Wait(Services.Clock.BeatLength() / 2),
			new ActionTask(() => { Services.Clock.SyncFunction(_SetColors, Clock.BeatValue.Quarter); }),
			new Wait(Services.Clock.BeatLength()),
			new ActionTask(() => { Services.Clock.SyncFunction(_SetColors, Clock.BeatValue.Quarter); }),
			new Wait(Services.Clock.BeatLength()),
			new ActionTask(() => { Services.Clock.SyncFunction(_SetColors, Clock.BeatValue.Quarter); })
			}));
		
		_colorSwitcher.Do(switchColors);
	}

	internal override void Update()
	{
		_colorSwitcher.Update();
		
		if (num_switches >= 4) SetStatus(TaskStatus.Success);
	}

	private void _SetColors()
	{
		num_switches++;
		
		for (int i = 0; i < Services.MapManager.MapWidth; i++)
		{
			for (int j = 0; j < Services.MapManager.MapHeight; j++)
			{
				Tile t = Services.MapManager.GetTile(i, j);
				if ((grid[i, j] + num_switches) % 4 == 0)
				{
					t.SetBpAssistColor(color1);
				}
				else if ((grid[i, j] + num_switches) % 4 == 1)
				{
					t.SetBpAssistColor(color2);
				}
				else if ((grid[i, j] + num_switches) % 4 == 2)
				{
					t.SetBpAssistColor(color3);
				}
				else 
				{
					t.SetBpAssistColor(color4);
				}
			}
		}
	}
}

public class DiscoBlocks : DiscoFloor
{
	private int num_switches = 0;
	private Color color1, color2, color3, color4;
	private TaskManager _colorSwitcher;

	private int[,] grid =
	{
		{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
		{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
		{0, 0, 3, 3, 3, 3, 3, 3, 0, 0, 1, 1, 2, 2, 2, 2, 2, 2, 1, 1},
		{0, 0, 3, 3, 3, 3, 3, 3, 0, 0, 1, 1, 2, 2, 2, 2, 2, 2, 1, 1},
		{0, 0, 3, 3, 0, 0, 3, 3, 0, 0, 1, 1, 2, 2, 1, 1, 2, 2, 1, 1},
		{0, 0, 3, 3, 0, 0, 3, 3, 0, 0, 1, 1, 2, 2, 1, 1, 2, 2, 1, 1},
		{0, 0, 3, 3, 3, 3, 3, 3, 0, 0, 1, 1, 2, 2, 2, 2, 2, 2, 1, 1},
		{0, 0, 3, 3, 3, 3, 3, 3, 0, 0, 1, 1, 2, 2, 2, 2, 2, 2, 1, 1},
		{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
		{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
		{2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3},
		{2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3},
		{2, 2, 1, 1, 1, 1, 1, 1, 2, 2, 3, 3, 0, 0, 0, 0, 0, 0, 3, 3},
		{2, 2, 1, 1, 1, 1, 1, 1, 2, 2, 3, 3, 0, 0, 0, 0, 0, 0, 3, 3},
		{2, 2, 1, 1, 2, 2, 1, 1, 2, 2, 3, 3, 0, 0, 3, 3, 0, 0, 3, 3},
		{2, 2, 1, 1, 2, 2, 1, 1, 2, 2, 3, 3, 0, 0, 3, 3, 0, 0, 3, 3},
		{2, 2, 1, 1, 1, 1, 1, 1, 2, 2, 3, 3, 0, 0, 0, 0, 0, 0, 3, 3},
		{2, 2, 1, 1, 1, 1, 1, 1, 2, 2, 3, 3, 0, 0, 0, 0, 0, 0, 3, 3},
		{2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3},
		{2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3}
	};

	public DiscoBlocks()
	{
		_colorSwitcher = new TaskManager();
		
		int offset = UnityEngine.Random.Range(0, 4);

		color1 = colors[offset % colors.Length];
		color2 = colors[(offset + 1) % colors.Length];
		color3 = colors[(offset + 2) % colors.Length];
		color4 = colors[(offset + 3) % colors.Length];

		TaskQueue switchColors = new TaskQueue(new List<Task>(new Task[] {
			new ActionTask(_SetColors),
			new Wait(Services.Clock.BeatLength() / 2),
			new ActionTask(() => { Services.Clock.SyncFunction(_SetColors, Clock.BeatValue.Quarter); }),
			new Wait(Services.Clock.BeatLength()),
			new ActionTask(() => { Services.Clock.SyncFunction(_SetColors, Clock.BeatValue.Quarter); }),
			new Wait(Services.Clock.BeatLength()),
			new ActionTask(() => { Services.Clock.SyncFunction(_SetColors, Clock.BeatValue.Quarter); })
			}));
		
		_colorSwitcher.Do(switchColors);
	}

	internal override void Update()
	{
		_colorSwitcher.Update();
		
		if (num_switches >= 4) SetStatus(TaskStatus.Success);
	}

	private void _SetColors()
	{
		num_switches++;
		
		for (int i = 0; i < Services.MapManager.MapWidth; i++)
		{
			for (int j = 0; j < Services.MapManager.MapHeight; j++)
			{
				Tile t = Services.MapManager.GetTile(i, j);
				if ((grid[i, j] + num_switches) % 4 == 0)
				{
					t.SetBpAssistColor(color1);
				}
				else if ((grid[i, j] + num_switches) % 4 == 1)
				{
					t.SetBpAssistColor(color2);
				}
				else if ((grid[i, j] + num_switches) % 4 == 2)
				{
					t.SetBpAssistColor(color3);
				}
				else 
				{
					t.SetBpAssistColor(color4);
				}
			}
		}
	}
}

public class DiscoWindmill : DiscoFloor
{
	private int num_switches = 0;
	private Color color1, color2, color3, color4;
	private TaskManager _colorSwitcher;

	private int[,] grid =
	{
		{1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3},
		{1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 3, 3, 3, 3, 3, 3, 3, 3, 3, 2},
		{1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 3, 3, 3, 3, 3, 3, 3, 3, 2, 2},
		{1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 3, 3, 3, 3, 3, 3, 3, 2, 2, 2},
		{1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 3, 3, 3, 3, 3, 3, 2, 2, 2, 2},
		{1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 3, 3, 3, 3, 3, 2, 2, 2, 2, 2},
		{1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 3, 3, 3, 3, 2, 2, 2, 2, 2, 2},
		{1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 3, 3, 3, 2, 2, 2, 2, 2, 2, 2},
		{1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 3, 3, 2, 2, 2, 2, 2, 2, 2, 2},
		{1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 3, 2, 2, 2, 2, 2, 2, 2, 2, 2},
		{2, 2, 2, 2, 2, 2, 2, 2, 2, 3, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
		{2, 2, 2, 2, 2, 2, 2, 2, 3, 3, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1},
		{2, 2, 2, 2, 2, 2, 2, 3, 3, 3, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1},
		{2, 2, 2, 2, 2, 2, 3, 3, 3, 3, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1},
		{2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1},
		{2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1},
		{2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1},
		{2, 2, 3, 3, 3, 3, 3, 3, 3, 3, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1},
		{2, 3, 3, 3, 3, 3, 3, 3, 3, 3, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1},
		{3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1}
	}; 

	/* private int[,] grid = {
		{1, 0, 0, 0, 0, 3, 3, 3, 3, 3, 1, 0, 0, 0, 0, 3, 3, 3, 3, 3},
		{1, 1, 0, 0, 0, 3, 3, 3, 3, 2, 1, 1, 0, 0, 0, 3, 3, 3, 3, 2},
		{1, 1, 1, 0, 0, 3, 3, 3, 2, 2, 1, 1, 1, 0, 0, 3, 3, 3, 2, 2},
		{1, 1, 1, 1, 0, 3, 3, 2, 2, 2, 1, 1, 1, 1, 0, 3, 3, 2, 2, 2},
		{1, 1, 1, 1, 1, 3, 2, 2, 2, 2, 1, 1, 1, 1, 1, 3, 2, 2, 2, 2},
		{2, 2, 2, 2, 3, 1, 1, 1, 1, 1, 2, 2, 2, 2, 3, 1, 1, 1, 1, 1},
		{2, 2, 2, 3, 3, 0, 1, 1, 1, 1, 2, 2, 2, 3, 3, 0, 1, 1, 1, 1},
		{2, 2, 3, 3, 3, 0, 0, 1, 1, 1, 2, 2, 3, 3, 3, 0, 0, 1, 1, 1},
		{2, 3, 3, 3, 3, 0, 0, 0, 1, 1, 2, 3, 3, 3, 3, 0, 0, 0, 1, 1},
		{3, 3, 3, 3, 3, 0, 0, 0, 0, 1, 3, 3, 3, 3, 3, 0, 0, 0, 0, 1},
		{1, 0, 0, 0, 0, 3, 3, 3, 3, 3, 1, 0, 0, 0, 0, 3, 3, 3, 3, 3},
		{1, 1, 0, 0, 0, 3, 3, 3, 3, 2, 1, 1, 0, 0, 0, 3, 3, 3, 3, 2},
		{1, 1, 1, 0, 0, 3, 3, 3, 2, 2, 1, 1, 1, 0, 0, 3, 3, 3, 2, 2},
		{1, 1, 1, 1, 0, 3, 3, 2, 2, 2, 1, 1, 1, 1, 0, 3, 3, 2, 2, 2},
		{1, 1, 1, 1, 1, 3, 2, 2, 2, 2, 1, 1, 1, 1, 1, 3, 2, 2, 2, 2},
		{2, 2, 2, 2, 3, 1, 1, 1, 1, 1, 2, 2, 2, 2, 3, 1, 1, 1, 1, 1},
		{2, 2, 2, 3, 3, 0, 1, 1, 1, 1, 2, 2, 2, 3, 3, 0, 1, 1, 1, 1},
		{2, 2, 3, 3, 3, 0, 0, 1, 1, 1, 2, 2, 3, 3, 3, 0, 0, 1, 1, 1},
		{2, 3, 3, 3, 3, 0, 0, 0, 1, 1, 2, 3, 3, 3, 3, 0, 0, 0, 1, 1},
		{3, 3, 3, 3, 3, 0, 0, 0, 0, 1, 3, 3, 3, 3, 3, 0, 0, 0, 0, 1}
	}; */

	public DiscoWindmill()
	{
		_colorSwitcher = new TaskManager();
		
		int offset = UnityEngine.Random.Range(0, 4);

		color1 = colors[offset % colors.Length];
		color2 = colors[(offset + 1) % colors.Length];
		color3 = colors[(offset + 2) % colors.Length];
		color4 = colors[(offset + 3) % colors.Length];

		TaskQueue switchColors = new TaskQueue(new List<Task>(new Task[] {
			new ActionTask(_SetColors),
			new Wait(Services.Clock.BeatLength() / 2),
			new ActionTask(() => { Services.Clock.SyncFunction(_SetColors, Clock.BeatValue.Quarter); }),
			new Wait(Services.Clock.BeatLength()),
			new ActionTask(() => { Services.Clock.SyncFunction(_SetColors, Clock.BeatValue.Quarter); }),
			new Wait(Services.Clock.BeatLength()),
			new ActionTask(() => { Services.Clock.SyncFunction(_SetColors, Clock.BeatValue.Quarter); })
			}));
		
		_colorSwitcher.Do(switchColors);
	}

	internal override void Update()
	{
		_colorSwitcher.Update();
		
		if (num_switches >= 4) SetStatus(TaskStatus.Success);
	}

	private void _SetColors()
	{
		num_switches++;
		
		for (int i = 0; i < Services.MapManager.MapWidth; i++)
		{
			for (int j = 0; j < Services.MapManager.MapHeight; j++)
			{
				Tile t = Services.MapManager.GetTile(i, j);
				if ((grid[i, j] + num_switches) % 4 == 0)
				{
					t.SetBpAssistColor(color1);
				}
				else if ((grid[i, j] + num_switches) % 4 == 1)
				{
					t.SetBpAssistColor(color2);
				}
				else if ((grid[i, j] + num_switches) % 4 == 2)
				{
					t.SetBpAssistColor(color3);
				}
				else 
				{
					t.SetBpAssistColor(color4);
				}
			}
		}
	}
}
