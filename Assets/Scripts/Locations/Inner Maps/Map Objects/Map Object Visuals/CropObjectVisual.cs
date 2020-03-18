using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CropObjectVisual : TileObjectGameObject {

    [SerializeField] private Sprite growingSprite;
    [SerializeField] private Sprite ripeSprite;
    
    public override void UpdateTileObjectVisual(TileObject obj) {
        Crops crops = obj as Crops;
        if (crops.currentGrowthState == Crops.Growth_State.Growing) {
            SetVisual(growingSprite);
        } else if (crops.currentGrowthState == Crops.Growth_State.Ripe) {
            SetVisual(ripeSprite);
        }
    }
}
