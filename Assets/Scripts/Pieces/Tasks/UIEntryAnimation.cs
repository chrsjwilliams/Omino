using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIEntryAnimation : Task
{
    private const float animDuration = 0.4f;
    private const float staggerTime = 0.1f;
    private float timeElapsed;
    private GameObject[] meters;
    private List<Blueprint> blueprints;
    private Vector3[] meterStartPositions;
    private Vector3[] meterTargetPositions;
    private Vector3[] blueprintStartPositions;
    private Vector3[] blueprintTargetPositions;
    private const float meterOffset = 200f;
    private const float blueprintOffset = 10f;
    private bool[] metersOn;
    private bool[] blueprintsOn;

    public UIEntryAnimation(GameObject[] meters_, List<Blueprint> blueprints_)
    {
        meters = meters_;
        blueprints = blueprints_;
    }

    protected override void Init()
    {
        timeElapsed = 0;

        metersOn = new bool[meters.Length];
        blueprintsOn = new bool[blueprints.Count];

        meterStartPositions = new Vector3[meters.Length];
        meterTargetPositions = new Vector3[meters.Length];
        blueprintStartPositions = new Vector3[blueprints.Count];
        blueprintTargetPositions = new Vector3[blueprints.Count];

        for (int i = 0; i < meters.Length; i++)
        {
            GameObject meter = meters[i];
            meterTargetPositions[i] = meter.transform.localPosition;
            meter.transform.localPosition += meterOffset * Vector3.down;
            meterStartPositions[i] = meter.transform.localPosition;
        }

        for (int i = 0; i < blueprints.Count; i++)
        {
            Blueprint blueprint = blueprints[i];
            blueprintTargetPositions[i] = blueprint.holder.transform.localPosition;
            if (blueprint.owner.playerNum == 1)
            {
                blueprint.holder.transform.localPosition += blueprintOffset * Vector3.down;
            }
            else
            {
                blueprint.holder.transform.localPosition += blueprintOffset * Vector3.up;
            }
            blueprintStartPositions[i] = blueprint.holder.transform.localPosition;
        }
    }

    internal override void Update()
    {
        timeElapsed += Time.deltaTime;

        for (int i = 0; i < meters.Length; i++)
        {
            if (timeElapsed >= i * staggerTime)
            {
                GameObject meter = meters[i];

                if (!metersOn[i])
                {
                    meter.SetActive(true);
                    metersOn[i] = true;
                }

                meters[i].transform.localPosition = Vector3.Lerp(
                    meterStartPositions[i],
                    meterTargetPositions[i],
                    EasingEquations.Easing.QuadEaseOut(
                        Mathf.Min(1,(timeElapsed - (i * staggerTime)) / animDuration)));
            }
        }

        for (int i = 0; i < blueprints.Count; i++)
        {
            Blueprint blueprint = blueprints[i];

            if (!blueprintsOn[i])
            {
                blueprint.holder.gameObject.SetActive(true);
                blueprintsOn[i] = true;
            }

            blueprint.holder.transform.localPosition = Vector3.Lerp(
                blueprintStartPositions[i],
                blueprintTargetPositions[i],
                EasingEquations.Easing.QuadEaseOut(
                    Mathf.Min(1,(timeElapsed - (i * staggerTime)) / animDuration)));
        }

        if (timeElapsed >= animDuration + meters.Length * staggerTime) SetStatus(TaskStatus.Success);
    }


}
