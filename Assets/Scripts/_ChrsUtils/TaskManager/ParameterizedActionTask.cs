using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ParameterizedActionTask<T> : Task
{
    private readonly Action<T> action;
    private readonly T parameter;

    public ParameterizedActionTask(Action<T> act, T parameter_)
    {
        action = act;
        parameter = parameter_;
    }

    protected override void Init()
    {
        SetStatus(TaskStatus.Success);
        action(parameter);
    }
}
