﻿using UnityEngine;
using System.Collections;
using System;

public class Factory : Blueprint
{
    public const float drawRateBonus = 0.04f;

    public Factory(Player player_) : base(BuildingType.FACTORY, player_)
    {
        onGainText = "+1 Normal Piece Draw Level";
    }

    protected override string GetName()
    {
        return "Factory";
    }

    protected override string GetDescription()
    {
        //return "+<color=green>" + Math.Round((double)normalDrawRateBonus, 3) + 
        //    "</color> normal pieces per second";
        return "Normal Piece Draw Level +1";
    }
}
