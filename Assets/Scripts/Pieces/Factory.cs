using UnityEngine;
using System.Collections;

public class Factory : Blueprint
{
    public Factory(Player player_) : base(BuildingType.FACTORY, player_) { }
}
