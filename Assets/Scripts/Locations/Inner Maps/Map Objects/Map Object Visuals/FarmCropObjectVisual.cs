using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class FarmCropObjectVisual : TileObjectGameObject {
    
    [SerializeField] private Sprite horizontalEmpty;
    [SerializeField] private Sprite horizontalGrowing;
    [SerializeField] private Sprite horizontalHarvestable;
    
    [SerializeField] private Sprite verticalEmpty;
    [SerializeField] private Sprite verticalGrowing;
    [SerializeField] private Sprite verticalHarvestable;

    private bool _isHorizontal;
    
    public override void UpdateTileObjectVisual(TileObject tileObject) {
        Crops crops = tileObject as Crops;
        Assert.IsNotNull(crops, $"Given tile object for {gameObject.name} is not a crop! {tileObject}");
        int totalRipeningTicks = crops.GetRipeningTicks();
        if (crops.currentGrowthState == Crops.Growth_State.Growing) {
            // if (crops.remainingRipeningTicks == -1) {
            //     SetVisual(_isHorizontal ? horizontalEmpty : verticalEmpty);
            // } else {
            //     int ticksGrowing = totalRipeningTicks - crops.remainingRipeningTicks; 
            //     if (ticksGrowing < Mathf.FloorToInt(totalRipeningTicks * 0.4f)) {
            //         SetVisual(_isHorizontal ? horizontalEmpty : verticalEmpty);
            //     } else {
            //         SetVisual(_isHorizontal ? horizontalGrowing : verticalGrowing);    
            //     }       
            // }
            SetVisual(_isHorizontal ? horizontalGrowing : verticalGrowing);
        } else {
            SetVisual(_isHorizontal ? horizontalHarvestable : verticalHarvestable);
        }
    }
    public override void SetVisual(Sprite sprite) {
        base.SetVisual(sprite);
        _isHorizontal = sprite.name.Contains("#1");
    }
}
