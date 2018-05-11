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
    private Transform iconTransform;
    private Vector3[] dustLocations;
    private const float shakeDur = 0.3f;
    private const float shakeMag = 0.2f;
    private const float shakeSpeed = 40f;
    private const float shakeStartTime = 0.2f;
    private bool shakeStarted;


    public BlueprintPlacementTask(Blueprint blueprint_)
    {
        blueprint = blueprint_;
        overlayTransform = blueprint.holder.spriteBottom.transform;
        iconTransform = blueprint.holder.icon.transform;
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
        blueprint.holder.spriteBottom.sortingLayerName = "Overlay";
        blueprint.holder.icon.sortingLayerName = "Overlay";
        Color color = blueprint.holder.spriteBottom.color;
        blueprint.holder.spriteBottom.color = new Color(color.r, color.g, color.b, 1);
        overlayTransform.position = startPos;
        iconTransform.position = startPos;
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
        iconTransform.position = Vector3.Lerp(startPos, targetPos,
            EasingEquations.Easing.BounceEaseOut(timeElapsed / duration));
        iconTransform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one,
            EasingEquations.Easing.QuadEaseOut(timeElapsed / duration));
        if (timeElapsed > dustTime && !dustDropped)
        {
            for (int i = 0; i < dustLocations.Length; i++)
            {
                GameObject.Instantiate(Services.Prefabs.DustCloud,
                    dustLocations[i], Quaternion.identity);
            }
            dustDropped = true;
        }
        if(timeElapsed >= shakeStartTime && !shakeStarted)
        {
            Services.CameraController.StartShake(shakeDur, shakeSpeed, shakeMag);
            shakeStarted = true;
            Services.AudioManager.CreateTempAudio(Services.Clips.BlueprintPlaced, 1);
        }
        if (timeElapsed >= duration) SetStatus(TaskStatus.Success);
    }

    protected override void OnSuccess()
    {
        foreach(Tile tile in blueprint.tiles)
        {
            tile.mainSr.enabled = false;
        }
        Services.AudioManager.CreateTempAudio(Services.Clips.ProdLevelUp, 1);
    }


}
