using UnityEngine;
using System.Collections;

public class BlueprintPlacementTask : BuildingDropAnimation
{
    public BlueprintPlacementTask(Polyomino building_) :base(building_)
    {

    }
    protected override void Init()
    {
        base.Init();
        building.holder.spriteBottom.sortingLayerName = "Overlay";
        building.holder.icon.sortingLayerName = "Overlay";
        Color color = building.holder.spriteBottom.color;
        building.holder.spriteBottom.color = new Color(color.r, color.g, color.b, 1);
        SetAudioToBlueprint();
    }

    protected override void OnSuccess()
    {
        base.OnSuccess();
        foreach (Tile tile in building.tiles)
        {
            tile.mainSr.enabled = false;
        }
        Services.AudioManager.PlaySoundEffect(Services.Clips.ProdLevelUp, 1);
    }


}
