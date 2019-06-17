using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BeatManagement;

public class InitialBuildingEntryAnimation : Task
{
    private TaskManager subtaskManager;
    private float baseStaggerTime;
    private float structStaggerTime;
    private float terrainStaggerTime;
    private bool run_tasks = false;

    protected override void Init()
    {
        baseStaggerTime = Services.Clock.EighthLength();
        structStaggerTime = Services.Clock.SixteenthLength();
        terrainStaggerTime = Services.Clock.ThirtySecondLength();
        subtaskManager = new TaskManager();
        
        for (int i = 0; i < 2; i++)
        {
            Task dropTask = new Wait(baseStaggerTime * i);
            dropTask.Then(new BuildingDropAnimation(Services.GameManager.Players[i].mainBase));
            subtaskManager.Do(dropTask);
        }
        for (int i = 0; i < Services.MapManager.structuresOnMap.Count; i++)
        {
            Task dropTask = new Wait((structStaggerTime * i) + (baseStaggerTime * 2));
            dropTask.Then(new BuildingDropAnimation(Services.MapManager.structuresOnMap[i]));
            subtaskManager.Do(dropTask);
        }

        for (int i = 0; i < Services.MapManager.terrainOnMap.Count; i++)
        {
            Task dropTask = new Wait((terrainStaggerTime * i) + (baseStaggerTime * 2));
            dropTask.Then(new BuildingDropAnimation(Services.MapManager.terrainOnMap[i]));
            subtaskManager.Do(dropTask);
        }

        // Task waitTask = new Wait((structStaggerTime * Services.MapManager.structuresOnMap.Count) + (baseStaggerTime * 2));
        // waitTask.Then(new ActionTask(() => { SetStatus(TaskStatus.Success); }));

        //subtaskManager.Do(waitTask);

        Services.Clock.SyncFunction(() => { run_tasks = true; }, Clock.BeatValue.Quarter);
    }
    
    internal override void Update()
    {
        if (run_tasks)
        {
            subtaskManager.Update();
            if (subtaskManager.tasksInProcessCount == 0)
                SetStatus(TaskStatus.Success);
        }
    }

}
