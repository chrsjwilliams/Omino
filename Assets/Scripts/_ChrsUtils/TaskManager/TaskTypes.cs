using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

////////////////////////////////////////////////////////////////////////
// GENERAL PURPOSE TASKS
////////////////////////////////////////////////////////////////////////

// Simple action task
public class ActionTask : Task
{

    public Action Action { get; private set; }

    public ActionTask(Action action)
    {
        Action = action;
    }

    protected override void Init()
    {
        Action();
        SetStatus(TaskStatus.Success);
    }
}


// A base class for tasks that track time. Use it to make things like
// Wait, ScaleUpOverTime, etc. tasks
public abstract class TimedTask : Task
{
    public float Duration { get; private set; }
    public float StartTime { get; private set; }

    protected TimedTask(float duration)
    {
        Debug.Assert(duration >= 0, "Cannot create a timed task with duration less than 0");
        Duration = duration;
    }

    protected override void Init()
    {
        StartTime = Time.time;
    }

    internal override void Update()
    {
        var now = Time.time;
        var elapsed = now - StartTime;
        var t = elapsed / Duration;
        if (t > 1)
        {
            OnElapsed();
        }
        else
        {
            OnTick(t);
        }
    }

    // t is the normalized time for the task. E.g. if half the task's duration has elapsed then t == 0.5
    // This is where subclasses will do most of their work
    protected virtual void OnTick(float t) { }

    // Default to being successful if we get to the end of the duration
    protected virtual void OnElapsed()
    {
        SetStatus(TaskStatus.Success);
    }

}


public class WaitUnscaled : Task
{
    private float duration;
    private float timeElapsed;

    public WaitUnscaled(float dur)
    {
        timeElapsed = 0;
        duration = dur;
    }

    internal override void Update()
    {
        timeElapsed += Time.unscaledDeltaTime;
        if (timeElapsed >= duration) SetStatus(TaskStatus.Success);
    }
}

// A VERY simple wait task
public class Wait : TimedTask
{
    public Wait(float duration) : base(duration) { }

    internal override void Update()
    {
        base.Update();
    }
}


////////////////////////////////////////////////////////////////////////
// GAME OBJECT TASKS
////////////////////////////////////////////////////////////////////////

// Base classes for tasks that operate on a game object.
// Since C# doesn't allow multiple inheritance we'll make two versions - one timed and one untimed
public abstract class GOTask : Task
{
    protected readonly GameObject gameObject;

    protected GOTask(GameObject gameObject)
    {
        this.gameObject = gameObject;
    }
}


public abstract class TimedGOTask : TimedTask
{
    protected readonly GameObject gameObject;

    protected TimedGOTask(GameObject gameObject, float duration) : base(duration)
    {
        this.gameObject = gameObject;
    }
}


// A task to teleport a gameobject
public class SetPos : GOTask
{
    private readonly Vector3 _pos;

    public SetPos(GameObject gameObject, Vector3 pos) : base(gameObject)
    {
        _pos = pos;
    }

    protected override void Init()
    {
        gameObject.transform.position = _pos;
        SetStatus(TaskStatus.Success);
    }
}


// A task to lerp a gameobject's position
public class LERP : TimedGOTask
{
    public Vector3 Start { get; private set; }
    public Vector3 End { get; private set; }

    public LERP(GameObject gameObject, Vector3 start, Vector3 end, float duration) : base(gameObject, duration)
    {
        Start = start;
        End = end;
    }

    protected override void OnTick(float t)
    {
        gameObject.transform.localPosition = Vector3.Lerp(Start, End, t);
    }
}


public class LERPProgressBar : TimedTask
{
    public Image progressBar { get; private set; }
    public float endAmount { get; private set; }
    public float duration { get; private set; }

    public LERPProgressBar(Image _progressBar, float _endAmount, float _duration) : base(_duration)
    {
        progressBar = _progressBar;
        endAmount = _endAmount;
        duration = _duration;
    }

    protected override void OnTick(float t)
    {
        progressBar.fillAmount = Mathf.Lerp(progressBar.fillAmount, endAmount, EasingEquations.Easing.BackEaseIn(t));
    }
}

public class LERPColor : TimedTask
{
    public Color _Color;
    public Color Start { get; private set; }
    public Color End { get; private set; }
    public SpriteRenderer Sprite { get; private set; }
    public Text Text { get; private set; }
    public TextMeshProUGUI[] TMP { get; private set; }
    public Image Image { get; private set; }
    public Camera Camera;

    public LERPColor(Color color, Color start, Color end, float duration) : base(duration)
    {
        Start = start;
        End = end;
        _Color = color;
    }

    public LERPColor(SpriteRenderer sprite, Color start, Color end, float duration) : base(duration)
    {
        Start = start;
        End = end;
        Sprite = sprite;
    }

    public LERPColor(Text text, Color start, Color end, float duration) : base(duration)
    {
        Start = start;
        End = end;
        Text = text;
    }

    public LERPColor(Image image, Color start, Color end, float duration) : base(duration)
    {
        Start = start;
        End = end;
        Image = image;
    }
    
    public LERPColor(TextMeshProUGUI[] text, Color start, Color end, float duration) : base(duration)
    {
        Start = start;
        End = end;
        TMP = text;
    }
    
    public LERPColor(Camera camera,Color start, Color end, float duration) : base(duration)
    {
        Start = start;
        End = end;
        Camera = camera;
    }

    protected override void OnTick(float t)
    {
        if (Sprite) Sprite.color = Color.Lerp(Start, End, t);
        else if (Text) Text.color = Color.Lerp(Start, End, t);
        else if (Image) Image.color = Color.Lerp(Start, End, t);
        else if (Camera) Camera.backgroundColor = Color.Lerp(Start, End, t);
        else if (TMP.Length >= 0)
        {
            for (int i = 0; i < TMP.Length; i++)
                TMP[i].color = Color.Lerp(Start, End, t);
        }
        else _Color = Color.Lerp(Start, End, t);
    }
}


// A task to lerp a gameobject's scale
public class Scale : TimedGOTask
{
    public Vector3 Start { get; private set; }
    public Vector3 End { get; private set; }

    public Scale(GameObject gameObject, Vector3 start, Vector3 end, float duration) : base(gameObject, duration)
    {
        Start = start;
        End = end;
    }

    protected override void OnTick(float t)
    {
        gameObject.transform.localScale = Vector3.Lerp(Start, End, t);
    }
}

// A task to lerp a gameobject's scale
public class Rotate : TimedGOTask
{
    public Vector3 Start { get; private set; }
    public Vector3 End { get; private set; }

    public Rotate(GameObject gameObject, Vector3 start, Vector3 end, float duration) : base(gameObject, duration)
    {
        Start = start;
        End = end;
    }

    protected override void OnTick(float t)
    {
        gameObject.transform.localRotation = Quaternion.Euler(Vector3.Lerp(Start, End, t));
    }
}

