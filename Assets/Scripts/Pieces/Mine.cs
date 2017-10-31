using UnityEngine;
using System.Collections;

public class Mine : Blueprint
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
    private const float baseDrawPeriod = 30f;
    private float drawRate { get { return 1 / baseDrawPeriod; } }

    public Mine(Player player_) : base(BuildingType.MINE, player_) { }

    public override void Remove()
    {
        base.Remove();
        RemoveTimerUI();
    }

    protected override void OnPlace()
    {
        base.OnPlace();
        CreateTimerUI();
    }

    public override void Update()
    {
        drawMeter += drawRate * Time.deltaTime;
        if (drawMeter >= 1)
        {
            owner.DrawPieces(1, holder.transform.position);
            drawMeter -= 1;
        }
    }
}
