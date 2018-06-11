using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ScrollReadyBanners : Task
{
    private Transform[] banners;
    private float timeElapsed;
    private const float duration = 0.3f;
    private Vector3[] targetPositions;
    private Vector3[] startPositions;
    private bool scrollOn;

    public ScrollReadyBanners(Button[] banners_, bool scrollOn_)
    {
        banners = new Transform[2];
        for (int i = 0; i < 2; i++)
        {
            banners[i] = banners_[i].transform;
        }
        scrollOn = scrollOn_;
    }

    protected override void Init()
    {
        timeElapsed = 0;
        startPositions = new Vector3[2];
        targetPositions = new Vector3[2];
        for (int i = 0; i < 2; i++)
        {
            startPositions[i] = banners[i].localPosition;
            Vector3 offset = banners[i].gameObject.GetComponent<RectTransform>().sizeDelta.y * Vector3.down;
            if (i == 1) offset *= -1;
            targetPositions[i] = startPositions[i] + offset;
            if (scrollOn)
            {
                banners[i].gameObject.SetActive(true);
                banners[i].transform.localPosition = targetPositions[i];
            }
        }
    }

    internal override void Update()
    {
        timeElapsed += Time.deltaTime;

        for (int i = 0; i < banners.Length; i++)
        {
            Vector3 start;
            Vector3 target;
            EasingEquations.Easing.Function easingFunction;
            if (scrollOn)
            {
                start = targetPositions[i];
                target = startPositions[i];
                easingFunction = EasingEquations.Easing.GetFunctionWithTypeEnum(
                    EasingEquations.Easing.FunctionType.QuadEaseOut);
            }
            else
            {
                start = startPositions[i];
                target = targetPositions[i];
                easingFunction = EasingEquations.Easing.GetFunctionWithTypeEnum(
                    EasingEquations.Easing.FunctionType.QuadEaseIn);
            }
            banners[i].transform.localPosition = Vector3.Lerp(
                start, target,
                easingFunction(timeElapsed / duration));
        }

        if (timeElapsed >= duration) SetStatus(TaskStatus.Success);
    }
}
