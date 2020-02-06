
using UnityEngine;
using BeatManagement;

public class Glower: MonoBehaviour
{
	public SpriteRenderer glowEffect;
	private TaskManager _pulseTM;
	

	void Start () {
		_pulseTM = new TaskManager();

		Services.Clock.eventManager.Register<Measure>(GlowEffect);
	}

	void GlowEffect(BeatEvent e)
	{
		_pulseTM = new TaskManager();
		_pulseTM.Do(new Glow(glowEffect, Services.Clock.MeasureLength() - Services.Clock.SixteenthLength()));
	}
	
	void Update()
	{
		_pulseTM.Update();
	}
	
	public class Glow : Task
	{
		private SpriteRenderer glower;
		private float duration, timeElapsed;

		public Glow(SpriteRenderer glower, float duration)
		{
			this.glower = glower;
			this.duration = duration;
			timeElapsed = 0.0f;
		}

		internal override void Update()
		{
			timeElapsed += Time.deltaTime;
			
			if (timeElapsed >= duration)
			{
				SetStatus(TaskStatus.Success);
			}

			if (timeElapsed <= duration / 2)
			{
				glower.color = new Color(1f, 1f, 1f,
					Mathf.Lerp(50f/255f, 120f/255f, timeElapsed / (duration / 2)));
			}
			else
			{
				glower.color = new Color(1f, 1f, 1f,
					Mathf.Lerp(120f/255f, 50f/255f, (timeElapsed - (duration/2) / (duration / 2))));
			}
		}
	}
}
