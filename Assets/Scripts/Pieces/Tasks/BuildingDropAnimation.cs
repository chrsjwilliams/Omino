using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Beat;

public class BuildingDropAnimation : Task
{
    private float duration = 0.5f;
    private const float dropHeight = 4f;
    private const float dustTime = 0.1f;
    private bool dustDropped;
    private float timeElapsed;
    private Vector3[] startPositions;
    private Vector3[] targetPositions;
    protected Polyomino building;
    protected List<Transform> transforms;
    private Vector3[] dustLocations;
    private const float shakeDur = 0.3f;
    private const float shakeMag = 0.2f;
    private const float shakeSpeed = 40f;
    private const float shakeStartTime = 0.2f;
    private bool shakeStarted;
    private bool buildingDropSound = true;

    public BuildingDropAnimation(Polyomino building_)
    {
        building = building_;
        dustLocations = new Vector3[building.tiles.Count];
        for (int i = 0; i < dustLocations.Length; i++)
        {
            dustLocations[i] = building.tiles[i].transform.position;
        }
        transforms = new List<Transform>();
    }

    protected void SetAudioToBlueprint()
    {
        buildingDropSound = false;
    }

    protected override void Init()
    {
        duration = Services.Clock.EighthLength();
        
        if (building.holder == null)
        {
            SetStatus(TaskStatus.Aborted);
            return;
        }

        building.holder.gameObject.SetActive(true);

        transforms.Add(building.holder.spriteBottom.transform);
        transforms.Add(building.holder.icon.transform);
        transforms.Add(building.holder.dropShadow.transform);

        startPositions = new Vector3[transforms.Count];
        targetPositions = new Vector3[transforms.Count];
        for (int i = 0; i < transforms.Count; i++)
        {
            targetPositions[i] = transforms[i].position;
            startPositions[i] = targetPositions[i] + (dropHeight * Vector3.up);
            transforms[i].position = startPositions[i];
        }
        timeElapsed = 0;
    }

    internal override void Update()
    {
        if (building.holder == null)
        {
            SetStatus(TaskStatus.Aborted);
            return;
        }
        timeElapsed += Time.deltaTime;

        for (int i = 0; i < transforms.Count; i++)
        {
            transforms[i].position = Vector3.Lerp(startPositions[i], targetPositions[i],
                EasingEquations.Easing.BounceEaseOut(timeElapsed / duration));
            transforms[i].localScale = Vector3.Lerp(Vector3.zero, Vector3.one,
                EasingEquations.Easing.QuadEaseOut(timeElapsed / duration));
        }

        if (timeElapsed > dustTime && !dustDropped)
        {
            for (int i = 0; i < dustLocations.Length; i++)
            {
                GameObject.Instantiate(Services.Prefabs.DustCloud,
                    dustLocations[i], Quaternion.identity);
            }
            dustDropped = true;
        }
        if (timeElapsed >= shakeStartTime && !shakeStarted)
        {
            Services.CameraController.StartShake(shakeDur, shakeSpeed, shakeMag);
            shakeStarted = true;
            if (buildingDropSound)
                Services.AudioManager.RegisterSoundEffect(Services.Clips.BuildingFall, 1, Clock.BeatValue.Sixteenth);
            else
                Services.AudioManager.RegisterSoundEffect(Services.Clips.BlueprintPlaced, 1, Clock.BeatValue.Sixteenth);
        }
        if (timeElapsed >= duration) SetStatus(TaskStatus.Success);
    }
}
