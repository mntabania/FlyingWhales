using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class CropGameObject : TileObjectGameObject {

    [Tooltip("This is used to determine what growing sprite should be used given a ripe sprite")]
    [SerializeField] private SpriteSpriteDictionary growingSprites;
    [Tooltip("This is used to determine what ripe sprite should be used given a growing sprite")]
    [SerializeField] private SpriteSpriteDictionary ripeSprites;
    
    public override void UpdateTileObjectVisual(TileObject bed) {
        Crops crops = bed as Crops;
        if (crops.currentGrowthState == Crops.Growth_State.Growing) {
            if (growingSprites.Values.Contains(objectVisual.sprite)) { return; } //current sprite is already set to growing sprite
            Assert.IsTrue(growingSprites.ContainsKey(objectVisual.sprite), $"Growing sprite for {name} could not be found using sprite {objectVisual.sprite}");
            SetVisual(growingSprites[objectVisual.sprite]);
        } else if (crops.currentGrowthState == Crops.Growth_State.Ripe) {
            if (ripeSprites.Values.Contains(objectVisual.sprite)) { return; } //current sprite is already set to ripe sprite
            Assert.IsTrue(ripeSprites.ContainsKey(objectVisual.sprite), $"Ripe sprite for {name} could not be found using sprite {objectVisual.sprite}");
            SetVisual(ripeSprites[objectVisual.sprite]);
        }
    }
}
