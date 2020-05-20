using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class CornCropObjectVisual : TileObjectGameObject {
    
    [SerializeField] private Sprite empty;
    [SerializeField] private Sprite growing;
    [SerializeField] private Sprite harvestable;
    
    public override void UpdateTileObjectVisual(TileObject tileObject) {
        Crops crops = tileObject as Crops;
        Assert.IsNotNull(crops, $"Given tile object for {gameObject.name} is not a crop! {tileObject}");
        int totalRipeningTicks = crops.GetRipeningTicks();
        int ticksGrowing = totalRipeningTicks - crops.remainingRipeningTicks; 
        if (ticksGrowing < Mathf.FloorToInt(totalRipeningTicks * 0.4f)) {
            SetVisual(empty);
        } else if (ticksGrowing < totalRipeningTicks) {
            SetVisual(growing);    
        } else {
            SetVisual(harvestable);
        }
    }
}
