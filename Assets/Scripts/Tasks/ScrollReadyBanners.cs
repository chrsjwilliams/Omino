using UnityEngine;

public class ScrollReadyBanners : Task
{
    private ReadyBanner[] banners;
    private float timeElapsed;
    private const float duration = 0.3f;
    private Vector3[] targetPositions;
    private Vector3[] startPositions;
    private bool scrollOn;
    private bool editMode;

    public ScrollReadyBanners(ReadyBanner[] banners_, bool scrollOn_, bool editMode_ = false)
    {
        banners = banners_;
        scrollOn = scrollOn_;
        editMode = editMode_;
    }

    protected override void Init()
    {
        timeElapsed = 0;
        startPositions = new Vector3[2];
        targetPositions = new Vector3[2];
        for (int i = 0; i < 2; i++)
        {
            startPositions[i] = banners[i].transform.localPosition;
            Vector3 offset = banners[i].gameObject.GetComponent<RectTransform>().sizeDelta.y * Vector3.down;
            if (i == 1) offset *= -1;
            targetPositions[i] = startPositions[i] + offset;
            if (scrollOn)
            {
                banners[i].gameObject.SetActive(true);
                banners[i].transform.localPosition = targetPositions[i];
                banners[i].Init();
            }
        }
        if(Services.GameManager.CurrentDevice == DEVICE.IPHONE_X)
        {

            startPositions[1] = new Vector3(startPositions[1].x, startPositions[1].y - 150, 0);
            targetPositions[1] = new Vector3(targetPositions[1].x, targetPositions[1].y + 150, 0);

        }

        if (editMode || Services.GameManager.Players[1] is AIPlayer)
        {
            startPositions[1] = targetPositions[1];
            banners[1].transform.localPosition = startPositions[1];
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
