using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ScrollOffReadyBanners : Task
{
    private Transform[] banners;
    private float timeElapsed;
    private float duration;
    private Vector3[] targetPositions;
    private Vector3[] startPositions;

    public ScrollOffReadyBanners(Button[] banners_)
    {
        banners = new Transform[2];
        for (int i = 0; i < 2; i++)
        {
            banners[i] = banners_[i].transform;
        }
    }

    protected override void Init()
    {
        timeElapsed = 0;
        duration = Services.UIManager.readyBannerScrollOffTime;
        startPositions = new Vector3[2];
        targetPositions = new Vector3[2];
        for (int i = 0; i < 2; i++)
        {
            startPositions[i] = banners[i].localPosition;
            Vector3 offset = banners[i].gameObject.GetComponent<RectTransform>().sizeDelta.y * Vector3.down;
            if (i == 1) offset *= -1;
            targetPositions[i] = startPositions[i] + offset;
        }
    }

    internal override void Update()
    {
        timeElapsed += Time.deltaTime;

        for (int i = 0; i < banners.Length; i++)
        {
            banners[i].transform.localPosition = Vector3.Lerp(startPositions[i], targetPositions[i],
                EasingEquations.Easing.QuadEaseIn(timeElapsed / duration));
        }

        if (timeElapsed >= duration) SetStatus(TaskStatus.Success);
    }
}
