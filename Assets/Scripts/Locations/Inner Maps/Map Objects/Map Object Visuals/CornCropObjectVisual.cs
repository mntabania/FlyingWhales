using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class CornCropObjectVisual : TileObjectGameObject {

    [Tooltip("This is used to determine what growing sprite should be used given a ripe sprite")]
    [SerializeField] private SpriteSpriteDictionary growingSprites;
    [Tooltip("This is used to determine what ripe sprite should be used given a growing sprite")]
    [SerializeField] private SpriteSpriteDictionary ripeSprites;

    [SerializeField] private Sprite empty;
    [SerializeField] private Sprite growing;
    [SerializeField] private Sprite harvestable;
    
    public override void UpdateTileObjectVisual(TileObject tileObject) {
        Crops crops = tileObject as Crops;
        Assert.IsNotNull(crops, $"Given tile object for {gameObject.name} is not a crop! {tileObject}");
        int totalRipeningTicks = crops.GetRipeningTicks();
        int ticksGrowing = totalRipeningTicks - crops.remainingRipeningTicks; 
        if (ticksGrowing < Mathf.FloorToInt(totalRipeningTicks * 0.5f)) {
            SetVisual(empty);
        } else if (ticksGrowing < totalRipeningTicks) {
            SetVisual(growing);    
        } else {
            SetVisual(harvestable);
        }
        
        
        // if (crops.currentGrowthState == Crops.Growth_State.Growing) {
        //     if (growingSprites.Values.Contains(objectVisual.sprite)) { return; } //current sprite is already set to growing sprite
        //     Assert.IsTrue(growingSprites.ContainsKey(objectVisual.sprite), $"Growing sprite for {name} could not be found using sprite {objectVisual.sprite}");
        //     SetVisual(growingSprites[objectVisual.sprite]);
        // } else if (crops.currentGrowthState == Crops.Growth_State.Ripe) {
        //     if (ripeSprites.Values.Contains(objectVisual.sprite)) { return; } //current sprite is already set to ripe sprite
        //     Assert.IsTrue(ripeSprites.ContainsKey(objectVisual.sprite), $"Ripe sprite for {name} could not be found using sprite {objectVisual.sprite}");
        //     SetVisual(ripeSprites[objectVisual.sprite]);
        // }
    }
}
