using UnityEngine;
using System.Collections;

public class Base : Polyomino
{
    private float drawMeter_;
    private float drawMeter
    {
        get { return drawMeter_; }
        set
        {
            drawMeter_ = value;
            ringTimer.fillAmount = Mathf.Min(1, drawMeter_);
        }
    }
    private const float baseDrawPeriod = 15f;
    private float drawRate { get { return 1 / baseDrawPeriod; } }

    public Base(int _units, int _index, Player _player) : base(_units, _index, _player)
    {
    }

    public override void Update()
    {
        drawMeter += drawRate * Time.deltaTime;
        if (drawMeter >= 1)
        {
            owner.DrawPieces(1);
            drawMeter -= 1;
        }
    }

    protected override void OnPlace()
    {
        base.OnPlace();
        CreateTimerUI();
    }
}
