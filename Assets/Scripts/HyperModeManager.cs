
using BeatManagement;
using UnityEngine;

public static class HyperModeManager
{
	private static TaskManager _pulseTM, _discoTM, _slowmoTM;
	private static Color[][] _previousScheme;
	private static Color[,] _hyperModeColors;
	private static ShuffleBag<DiscoFloor> _discoTiles;
	
	// Use this for initialization
	public static void StartGame()
	{
		_slowmoTM = new TaskManager();
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
			{new Color(1f, 0.5f, 0.0f, 1f), new Color(.6f, 0.2f, 0.0f, 1f)},
		};
		
		Color[][] colorScheme = new Color[2][];

		int x = 0;
		int y = 0;

		x = UnityEngine.Random.Range(0, _hyperModeColors.Length/2);
		y = UnityEngine.Random.Range(0, _hyperModeColors.Length/2);
		while (x == y)
		{
			y = UnityEngine.Random.Range(0, _hyperModeColors.Length/2);
		}

		colorScheme[0] = new Color[] { _hyperModeColors[x,0], _hyperModeColors[x,1] };
		colorScheme[1] = new Color[] { _hyperModeColors[y,0], _hyperModeColors[y,1] };
		 
		Services.GameManager.SetColorScheme(colorScheme); 
		
		_discoTiles = new ShuffleBag<DiscoFloor>(new DiscoFloor[] { 
			new DiscoStripes(), new DiscoCheckers(), new DiscoWave(), new DiscoBlocks(), new DiscoWindmill() } );
		
		_pulseTM = new TaskManager();
		_discoTM = new TaskManager();
		Pulse(new Quarter(0));
		Disco(new Measure(0));
		Services.Clock.eventManager.Register<Beat>(Pulse);
		Services.Clock.eventManager.Register<Measure>(Disco);
        #if UNITY_IOS
        Services.Clock.eventManager.Register<Measure>((e) => { Handheld.Vibrate(); });
        #endif
		Services.GameManager.MainCamera.backgroundColor =
			Color.Lerp(Color.black, Services.GameScene.backgroundColor,
				Services.Clock.BeatLength() - (float) Services.Clock.AtNextBeat() / Services.Clock.BeatLength());
	}

	// Update is called once per frame
	public static void Update()
	{
		_pulseTM.Update();
		_discoTM.Update();
		_slowmoTM.Update();
	}

	public static void Exit()
	{
		Services.AudioManager.FadeOutLevelMusic();
		Services.Clips = Resources.Load<ClipLibrary>("Audio/ClipLibrary");
		Services.Clock.SetBPM(110);
		Services.AudioManager.RegisterStartLevelMusic();
		Services.GameManager.SetColorScheme(_previousScheme);
		Services.Clock.ClearEvents();
	}

	public static void Placement(Color color, Vector3 location)
	{
		Services.CameraController.StartShake(Services.Clock.EighthLength(), 80f, 10.0f, true);
		ConfettiSplosion(color, location);
	}

	public static void Pulse(BeatEvent e)
	{
		_pulseTM.Do(new Pulse(Services.Clock.EighthLength()));
		_pulseTM.Do(new Shake(Services.Clock.SixteenthLength()));
	}

	public static void Disco(BeatEvent e)
	{
		_discoTM.Do(_discoTiles.Next());
	}
	
	public static void ConfettiSplosion(Color color, Vector3 location)
	{
		GameObject particles = GameObject.Instantiate(Services.Prefabs.Placementfetti);
		ParticleSystem ps = particles.GetComponent<ParticleSystem>();
		var main = ps.main;
		main.startColor = color;
		particles.transform.position = location;
		GameObject.Destroy(particles, 5f);
	}

	public static void Touch(Vector3 location)
	{
		GameObject particles = GameObject.Instantiate(Services.Prefabs.Starsplosion, location, Quaternion.identity) as GameObject;
		GameObject.Destroy(particles, 5f);
	}

	public static void SlowMo(float duration, Vector3 location)
	{
		TaskTree to_do = new TaskTree(new EmptyTask(),
			Services.AudioManager.SlowMo(duration), Services.CameraController.SlowMo(duration, location), Services.CameraController.SlowTimeScale(duration));
		
		_slowmoTM.Do(to_do);
	}

	public static void StructureClaimed(Color color, Vector3 location)
	{
		AudioClip airhornclip = Resources.Load<AudioClip>("Audio/HyperFX/airhorn");
		ParticleSystem confetti1 = StructureConfetti(color, location, Quaternion.Euler(45f, -90, -90));
		ParticleSystem confetti2 = StructureConfetti(color, location, Quaternion.Euler(135f, -90, -90));
		ParticleSystem confetti3 = StructureConfetti(color, location, Quaternion.Euler(225f, -90, -90));
		ParticleSystem confetti4 = StructureConfetti(color, location, Quaternion.Euler(315f, -90, -90));
		
		ActionTask airhorn1 = new ActionTask(() => { Services.AudioManager.RegisterSoundEffect(airhornclip, 1f, Clock.BeatValue.Sixteenth); Services.Clock.SyncFunction(() => { confetti1.Play(); }, Clock.BeatValue.Sixteenth); });
		Wait wait1 = new Wait(Services.Clock.SixteenthLength());
		ActionTask airhorn2 = new ActionTask(() => { Services.AudioManager.RegisterSoundEffect(airhornclip, 1f, Clock.BeatValue.Sixteenth); Services.Clock.SyncFunction(() => { confetti2.Play(); }, Clock.BeatValue.Sixteenth); });
		Wait wait2 = new Wait(Services.Clock.SixteenthLength());
		ActionTask airhorn3 = new ActionTask(() => { Services.AudioManager.RegisterSoundEffect(airhornclip, 1f, Clock.BeatValue.Sixteenth); Services.Clock.SyncFunction(() => { confetti3.Play(); }, Clock.BeatValue.Sixteenth); });
		Wait wait3 = new Wait(Services.Clock.SixteenthLength());
		ActionTask airhorn4 = new ActionTask(() => { Services.AudioManager.RegisterSoundEffect(airhornclip, 1f, Clock.BeatValue.Sixteenth); Services.Clock.SyncFunction(() => { confetti4.Play(); }, Clock.BeatValue.Sixteenth); });

		TaskTree to_do = new TaskTree(airhorn1, new TaskTree(wait1, new TaskTree(airhorn2, new TaskTree(wait2, new TaskTree(airhorn3, new TaskTree(wait3, new TaskTree(airhorn4)))))));
		_slowmoTM.Do(to_do);
	}

	public static ParticleSystem StructureConfetti(Color color, Vector3 location, Quaternion rotation)
	{
		ParticleSystem to_return = GameObject.Instantiate(Services.Prefabs.Structfetti, location, rotation).GetComponent<ParticleSystem>();
		var main = to_return.main;
		main.startColor = color;
		return to_return;
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
				MapTile t = Services.MapManager.GetTile(i, j);
				t.SetBackgroundAlpha(Mathf.Lerp(1.0f, 0.4f, (timeElapsed / duration)));
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

public abstract class DiscoFloor : Task
{
	public int num_switches = 0;
	public Color[] colors =
		{new Color(1, 0.92f, 0.016f, 1f), new Color(0f, 1f, 1f, 1f), new Color(1, 0, 1, 1f), new Color(1, 1f, 1f, 1f)};
	public float timeElapsed;
	public Color color1, color2, color3, color4;
	public bool _started = false;

	public DiscoFloor()
	{
		timeElapsed = 0;
		num_switches = 0;
		_RandomizeColors();
	}
	
	internal override void Update()
	{
		if (!_started)
		{
			SetColors(new Beat(0));
			Services.Clock.eventManager.Register<Beat>(SetColors);
			_started = true;
		}
		timeElapsed += Time.deltaTime;
		if (timeElapsed > Services.Clock.BeatLength() * (Services.Clock.beatsPerMeasure - 1) + Services.Clock.BeatLength()/2)
			_Reset();

		if (num_switches >= Services.Clock.beatsPerMeasure)
		 	_Reset();
	}

	private void _Reset()
	{
		Services.Clock.eventManager.Unregister<Beat>(SetColors);
		timeElapsed = 0;
		num_switches = 0;
		_RandomizeColors();
		_started = false;
		SetStatus(TaskStatus.Success);
	}

	abstract protected void SetColors(BeatEvent e);
	
	private void _RandomizeColors()
	{
		int offset = UnityEngine.Random.Range(0, 4);

		color1 = colors[offset % colors.Length];
		color2 = colors[(offset + 1) % colors.Length];
		color3 = colors[(offset + 2) % colors.Length];
		color4 = colors[(offset + 3) % colors.Length];
	}
}

public class DiscoCheckers : DiscoFloor
{
	private int[,] grid;
	
	/*private int[,] grid =
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
	}; */

	public DiscoCheckers()
	{
		bool double_wide = ((Services.MapManager.MapHeight % 2 == 0) && (Services.MapManager.MapWidth % 2 == 0));
		_EstablishGrid(double_wide);
	}

	internal override void Update()
	{
		base.Update();
	}

	private void _EstablishGrid(bool double_wide = true)
	{
		grid = new int[Services.MapManager.MapWidth, Services.MapManager.MapHeight];
		
		if (double_wide) {
			for (int i = 0; i < Services.MapManager.MapWidth; i += 2)
			{
				for (int j = 0; j < Services.MapManager.MapHeight; j +=2)
				{
					int to_set = ((j / 2) + (i / 2) % 2) % 2;
					grid[i, j] = to_set;
					grid[i + 1, j] = to_set;
					grid[i, j + 1] = to_set;
					grid[i + 1, j + 1] = to_set;
				}
			}
		}
		else
		{
			for (int i = 0; i < Services.MapManager.MapWidth; i++)
			{
				for (int j = 0; j < Services.MapManager.MapHeight; j++)
				{
					grid[i, j] = (j + (i % 2)) % 2;
				}
			}
		}
	}
	
	protected override void SetColors(BeatEvent e)
	{
		num_switches++;
		
		for (int i = 0; i < Services.MapManager.MapWidth; i++)
		{
			for (int j = 0; j < Services.MapManager.MapHeight; j++)
			{
				MapTile t = Services.MapManager.GetTile(i, j);
				if ((grid[i, j] + num_switches) % 2 == 0)
				{
					t.SetBackgroundColor(color1);
				}
				else
				{
					t.SetBackgroundColor(color2);
				}
			}
		}
	}
}

public class DiscoStripes : DiscoFloor
{
	private int[,] grid;
	/*private readonly int[,] gridLR =
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
	}; */

	public DiscoStripes()
	{
		_EstablishGrid();
	}

	internal override void Update()
	{
		base.Update();
	}

	protected override void SetColors(BeatEvent e)
	{
		num_switches++;
		
		for (int i = 0; i < Services.MapManager.MapWidth; i++)
		{
			for (int j = 0; j < Services.MapManager.MapHeight; j++)
			{
				MapTile t = Services.MapManager.GetTile(i, j);
				if ((grid[i, j] + num_switches) % 4 == 0)
				{
					t.SetBackgroundColor(color1);
				}
				else if ((grid[i, j] + num_switches) % 4 == 1)
				{
					t.SetBackgroundColor(color2);
				}
				else if ((grid[i, j] + num_switches) % 4 == 2)
				{
					t.SetBackgroundColor(color3);
				}
				else 
				{
					t.SetBackgroundColor(color4);
				}
			}
		}
	}

	private void _EstablishGrid()
	{
		bool pointing_left = (UnityEngine.Random.Range(0, 2) == 0);

		int num_colors = 2;

		if ((Services.MapManager.MapWidth % 4 == 0) && (Services.MapManager.MapHeight % 4 == 0))
		{
			num_colors = 4;
		}
		else if ((Services.MapManager.MapWidth % 3 == 0) && (Services.MapManager.MapHeight % 3 == 0))
		{
			num_colors = 3;
		}
		else
		{
			num_colors = 2;
		}
		
		grid = new int[Services.MapManager.MapWidth,Services.MapManager.MapHeight];

		for (int i = 0; i < Services.MapManager.MapWidth; i++)
		{
			for (int j = 0; j < Services.MapManager.MapHeight; j++)
			{
				if (pointing_left)
					grid[i, j] = (i + (j % num_colors)) % num_colors;
				else
					grid[i, j] = (i - (j % num_colors)) % num_colors;
			}
		}
	}
}

public class DiscoWave : DiscoFloor
{
	private int[,] grid;
	private readonly int[,] full_grid =
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
		_EstablishGrid();
	}

	internal override void Update()
	{
		base.Update();
	}

	protected override void SetColors(BeatEvent e)
	{
		num_switches++;
		
		for (int i = 0; i < Services.MapManager.MapWidth; i++)
		{
			for (int j = 0; j < Services.MapManager.MapHeight; j++)
			{
				MapTile t = Services.MapManager.GetTile(i, j);
				if ((grid[i, j] + num_switches) % 4 == 0)
				{
					t.SetBackgroundColor(color1);
				}
				else if ((grid[i, j] + num_switches) % 4 == 1)
				{
					t.SetBackgroundColor(color2);
				}
				else if ((grid[i, j] + num_switches) % 4 == 2)
				{
					t.SetBackgroundColor(color3);
				}
				else 
				{
					t.SetBackgroundColor(color4);
				}
			}
		}
	}

	private void _EstablishGrid()
	{
		int x_offset = (20 - Services.MapManager.MapWidth) / 2;
		int y_offset = (20 - Services.MapManager.MapHeight) / 2;

		grid = new int[Services.MapManager.MapWidth, Services.MapManager.MapHeight];

		for (int i = 0; i < Services.MapManager.MapWidth; i++)
		{
			for (int j = 0; j < Services.MapManager.MapHeight; j++)
			{
				grid[i, j] = full_grid[i + x_offset, j + y_offset];
			}
		}

		/*bool even = (Services.MapManager.MapWidth % 2 == 0) && (Services.MapManager.MapHeight % 2 == 0);

		grid = new int[Services.MapManager.MapWidth,Services.MapManager.MapHeight];
		
		for (int i = 0; i < Services.MapManager.MapWidth; i++)
		{
			for (int j = 0; j < Services.MapManager.MapHeight; j++)
			{
				if ((i < Services.MapManager.MapWidth / 2) && (j < Services.MapManager.MapHeight / 2))
					grid[i, j] = ((i + (j % 4)) + 8) % 4;
				else if ((i >= Services.MapManager.MapWidth / 2) && (j >= Services.MapManager.MapHeight / 2))
					grid[i, j] = ((-i - (j - 2 % 4)) + 8) % 4;
				else if ((i >= Services.MapManager.MapWidth/2 ) && (j < Services.MapManager.MapHeight / 2))
					grid[i, j] = ((-i + (j - 1 % 4)) + 8) % 4;
				else 
					grid[i, j] = ((i - (j -3 % 4)) + 8) % 4;
			}
		} */
	}
}

public class DiscoBlocks : DiscoFloor
{
	private int[,] grid;
	private readonly int[,] full_grid =
	{
		{0, 0, 3, 3, 0, 0, 3, 3, 0, 0, 1, 1, 2, 2, 1, 1, 2, 2, 1, 1},
		{0, 0, 3, 3, 0, 0, 3, 3, 0, 0, 1, 1, 2, 2, 1, 1, 2, 2, 1, 1},
		{3, 3, 3, 3, 0, 0, 3, 3, 0, 0, 1, 1, 2, 2, 1, 1, 2, 2, 2, 2},
		{3, 3, 3, 3, 0, 0, 3, 3, 0, 0, 1, 1, 2, 2, 1, 1, 2, 2, 2, 2},
		{0, 0, 0, 0, 0, 0, 3, 3, 0, 0, 1, 1, 2, 2, 1, 1, 1, 1, 1, 1},
		{0, 0, 0, 0, 0, 0, 3, 3, 0, 0, 1, 1, 2, 2, 1, 1, 1, 1, 1, 1},
		{3, 3, 3, 3, 3, 3, 3, 3, 0, 0, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2},
		{3, 3, 3, 3, 3, 3, 3, 3, 0, 0, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2},
		{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
		{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
		{2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3},
		{2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3},
		{1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 3, 3, 0, 0, 0, 0, 0, 0, 0, 0},
		{1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 3, 3, 0, 0, 0, 0, 0, 0, 0, 0},
		{2, 2, 2, 2, 2, 2, 1, 1, 2, 2, 3, 3, 0, 0, 3, 3, 3, 3, 3, 3},
		{2, 2, 2, 2, 2, 2, 1, 1, 2, 2, 3, 3, 0, 0, 3, 3, 3, 3, 3, 3},
		{1, 1, 1, 1, 2, 2, 1, 1, 2, 2, 3, 3, 0, 0, 3, 3, 0, 0, 0, 0},
		{1, 1, 1, 1, 2, 2, 1, 1, 2, 2, 3, 3, 0, 0, 3, 3, 0, 0, 0, 0},
		{2, 2, 1, 1, 2, 2, 1, 1, 2, 2, 3, 3, 0, 0, 3, 3, 0, 0, 3, 3},
		{2, 2, 1, 1, 2, 2, 1, 1, 2, 2, 3, 3, 0, 0, 3, 3, 0, 0, 3, 3}
	};

	public DiscoBlocks()
	{
		_EstablishGrid();
	}

	internal override void Update()
	{
		base.Update();
	}

	protected override void SetColors(BeatEvent e)
	{
		num_switches++;
		
		for (int i = 0; i < Services.MapManager.MapWidth; i++)
		{
			for (int j = 0; j < Services.MapManager.MapHeight; j++)
			{
				MapTile t = Services.MapManager.GetTile(i, j);
				if ((grid[i, j] + num_switches) % 4 == 0)
				{
					t.SetBackgroundColor(color1);
				}
				else if ((grid[i, j] + num_switches) % 4 == 1)
				{
					t.SetBackgroundColor(color2);
				}
				else if ((grid[i, j] + num_switches) % 4 == 2)
				{
					t.SetBackgroundColor(color3);
				}
				else 
				{
					t.SetBackgroundColor(color4);
				}
			}
		}
	}

	private void _EstablishGrid()
	{
		int x_offset = (20 - Services.MapManager.MapWidth) / 2;
		int y_offset = (20 - Services.MapManager.MapHeight) / 2;

		grid = new int[Services.MapManager.MapWidth, Services.MapManager.MapHeight];

		for (int i = 0; i < Services.MapManager.MapWidth; i++)
		{
			for (int j = 0; j < Services.MapManager.MapHeight; j++)
			{
				grid[i, j] = full_grid[i + x_offset, j + y_offset];
			}
		}
	}
}

public class DiscoWindmill : DiscoFloor
{
	private int[,] grid;
	private readonly int[,] full_grid =
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
		_EstablishGrid();
	}

	internal override void Update()
	{
		base.Update();
	}

	protected override void SetColors(BeatEvent e)
	{
		num_switches++;
		
		for (int i = 0; i < Services.MapManager.MapWidth; i++)
		{
			for (int j = 0; j < Services.MapManager.MapHeight; j++)
			{
				MapTile t = Services.MapManager.GetTile(i, j);
				if ((grid[i, j] + num_switches) % 4 == 0)
				{
					t.SetBackgroundColor(color1);
				}
				else if ((grid[i, j] + num_switches) % 4 == 1)
				{
					t.SetBackgroundColor(color2);
				}
				else if ((grid[i, j] + num_switches) % 4 == 2)
				{
					t.SetBackgroundColor(color3);
				}
				else 
				{
					t.SetBackgroundColor(color4);
				}
			}
		}
	}
	
	private void _EstablishGrid()
	{
		int x_offset = (20 - Services.MapManager.MapWidth) / 2;
		int y_offset = (20 - Services.MapManager.MapHeight) / 2;

		grid = new int[Services.MapManager.MapWidth, Services.MapManager.MapHeight];

		for (int i = 0; i < Services.MapManager.MapWidth; i++)
		{
			for (int j = 0; j < Services.MapManager.MapHeight; j++)
			{
				grid[i, j] = full_grid[i + x_offset, j + y_offset];
			}
		}
	}
}