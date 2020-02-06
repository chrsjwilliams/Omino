using UnityEngine;

public class BoardEntryAnimation : Task
{
    private float timeElapsed;
    private const float animDuration = 0.2f;
    private const float staggerTime = 0.015f;
    private float totalDuration;
    private MapTile[,] map { get { return Services.MapManager.Map; } }
    private int largerDimension;
    private int mapWidth;
    private int mapHeight;
    private int currentIndex;
    private bool[,] tilesOn;

    private static Vector3 offset = new Vector3(1f, 1f, 0);
    private Vector3[,] basePositions;

    protected override void Init()
    {
        timeElapsed = 0;
        currentIndex = 0;
        mapWidth = map.GetUpperBound(0)+1;
        mapHeight = map.GetUpperBound(1)+1;
        totalDuration = animDuration + (staggerTime * (mapHeight + mapWidth));
        tilesOn = new bool[mapWidth, mapHeight];
        basePositions = new Vector3[mapWidth, mapHeight];
        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                basePositions[i, j] = map[i, j].transform.position;
            }
        }
    }

    internal override void Update()
    {
        timeElapsed += Time.deltaTime;

        for (var i = 0; i < mapWidth; i++)
        {
            for (var j = 0; j < mapHeight; j++)
            {
                var mapTile = map[i, j];
                if (i + j > currentIndex) continue;
                if (!tilesOn[i, j])
                {
                    tilesOn[i, j] = true;
                    mapTile.gameObject.SetActive(true);
                }
                var progress = Mathf.Min((timeElapsed - ((i + j) * staggerTime)) / animDuration, 1);
                //mapTile.SetAlpha(Mathf.Lerp(0, targetAlpha,
                //    EasingEquations.Easing.QuadEaseOut(progress)));
                mapTile.transform.position = Vector3.Lerp(basePositions[i, j] + offset,
                    basePositions[i, j], EasingEquations.Easing.QuadEaseOut(progress));
            }
        }

        if (timeElapsed >= ((currentIndex + 1) * staggerTime))
        {
            currentIndex += 1;
        }
        if (timeElapsed >= totalDuration) SetStatus(TaskStatus.Success);
    }

    protected override void OnSuccess()
    {
        base.OnSuccess();
        for (var i = 0; i < mapWidth; i++)
        {
            for (var j = 0; j < mapHeight; j++)
            {
                map[i, j].gameObject.SetActive(true);
                map[i, j].transform.position = basePositions[i, j];
            }
        }
    }

}
