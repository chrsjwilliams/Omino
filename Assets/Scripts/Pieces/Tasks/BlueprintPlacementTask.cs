using UnityEngine;
using System.Collections;

public class BlueprintPlacementTask : Task
{
    private const float duration = 0.5f;
    private const float dropHeight = 4f;
    private const float dustTime = 0.1f;
    private bool dustDropped;
    private float timeElapsed;
    private Vector3 startPos;
    private Vector3 targetPos;
    private Blueprint blueprint;
    private Transform overlayTransform;
    private Vector3[] dustLocations;


    public BlueprintPlacementTask(Blueprint blueprint_)
    {
        blueprint = blueprint_;
        overlayTransform = blueprint.spriteOverlay.transform;
        dustLocations = new Vector3[blueprint.tiles.Count];
        for (int i = 0; i < dustLocations.Length; i++)
        {
            dustLocations[i] = blueprint.tiles[i].transform.position;
        }
    }

    protected override void Init()
    {
        if (blueprint.holder == null)
        {
            SetStatus(TaskStatus.Aborted);
            return;
        }
        targetPos = overlayTransform.position;
        startPos = targetPos + (dropHeight * Vector3.up);
        timeElapsed = 0;
        //blueprint.spriteOverlay.enabled = true;
        blueprint.spriteOverlay.sortingLayerName = "Overlay";
        Color color = blueprint.spriteOverlay.color;
        blueprint.spriteOverlay.color = new Color(color.r, color.g, color.b, 1);
        overlayTransform.position = startPos;
    }

    internal override void Update()
    {
        if (blueprint.holder == null)
        {
            SetStatus(TaskStatus.Aborted);
            return;
        }
        timeElapsed += Time.deltaTime;

        overlayTransform.position = Vector3.Lerp(startPos, targetPos,
            EasingEquations.Easing.BounceEaseOut(timeElapsed / duration));
        overlayTransform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one,
            EasingEquations.Easing.QuadEaseOut(timeElapsed / duration));
        if(timeElapsed > dustTime && !dustDropped)
        {
            for (int i = 0; i < dustLocations.Length; i++)
            {
                GameObject.Instantiate(Services.Prefabs.DustCloud,
                    dustLocations[i], Quaternion.identity);
            }
            dustDropped = true;
        }
        if (timeElapsed >= duration) SetStatus(TaskStatus.Success);
    }

    protected override void OnSuccess()
    {
        foreach(Tile tile in blueprint.tiles)
        {
            tile.mainSr.enabled = false;
        }
    }


}
