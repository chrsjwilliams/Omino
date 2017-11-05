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
    private const float baseResourceIncrementPeriod = 3f;
    private const int baseResourcesPerIncrement = 10;
    private float resourceGainMeter;
    private float resourceIncrementRate { get { return 1 / baseResourceIncrementPeriod; } }
    private int resourcesPerIncrement { get { return baseResourcesPerIncrement; } }

    public Base(int _units, int _index, Player _player) : base(_units, _index, _player)
    {
    }

    public override void Update()
    {
        drawMeter += drawRate * Time.deltaTime;
        if (drawMeter >= 1)
        {
            owner.DrawPieces(1, holder.transform.position);
            drawMeter -= 1;
        }

        resourceGainMeter += resourceIncrementRate * Time.deltaTime;
        if(resourceGainMeter >= 1)
        {
            owner.GainResources(resourcesPerIncrement);
            resourceGainMeter -= 1;
            Services.GeneralTaskManager.Do(new FloatText("+" + resourcesPerIncrement,
                GetCenterpoint(), owner, 3, 0.75f));
        }
    }

    protected override void OnPlace()
    {
        CreateTimerUI();
        ToggleCostUIStatus(false);
    }
}
