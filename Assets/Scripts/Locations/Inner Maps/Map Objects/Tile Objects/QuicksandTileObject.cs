using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UnityEngine.Assertions;

public class QuicksandTileObject : TileObject {

    private QuicksandMapObjectVisual _quicksandMapVisual;
    
    public QuicksandTileObject() {
        Initialize(TILE_OBJECT_TYPE.QUICKSAND, false);
        traitContainer.AddTrait(this, "Dangerous");
        traitContainer.RemoveTrait(this, "Flammable");
    }
    protected override void CreateMapObjectVisual() {
        base.CreateMapObjectVisual();
        _quicksandMapVisual = mapVisual as QuicksandMapObjectVisual;
        Assert.IsNotNull(_quicksandMapVisual, $"Map Object Visual of {this} is null!");
    }
    public override void Neutralize() {
        _quicksandMapVisual.Expire();
    }
    public override string ToString() {
        return "Quicksand";
    }
}
