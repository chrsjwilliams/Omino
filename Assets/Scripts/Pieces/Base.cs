using UnityEngine;
using System.Collections;

public class Base : Structure
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
        owner = _player;
    }

    public override void ActivateStructureCheck() { }

    public override void Update()
    {
        drawMeter += drawRate * Time.deltaTime;
        if (drawMeter >= 1)
        {
            Debug.Log(owner.name);
            owner.DrawPieces(1, holder.transform.position);
            drawMeter -= 1;
        }
    }

    protected override void OnPlace()
    {
        CreateTimerUI();
    }

    public override void OnInputUp() { }
}
