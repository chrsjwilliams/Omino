using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InitialBuildingEntryAnimation : Task
{
    private TaskManager subtaskManager;
    private const float baseStaggerTime = 0.5f;
    private const float structStaggerTime = 0.2f;

    protected override void Init()
    {
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
    }

    internal override void Update()
    {
        subtaskManager.Update();
        if (!subtaskManager.hasActiveTasks) SetStatus(TaskStatus.Success);
    }

}
