using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UnityEngine.Assertions;

public class Quicksand : TileObject {

    private QuicksandMapObjectVisual _quicksandMapVisual;
    
    public Quicksand() {
        Initialize(TILE_OBJECT_TYPE.QUICKSAND, false);
        traitContainer.AddTrait(this, "Dangerous");
        traitContainer.RemoveTrait(this, "Flammable");
    }
    protected override void CreateMapObjectVisual() {
        GameObject obj = InnerMapManager.Instance.mapObjectFactory.CreateNewTileObjectMapVisual(tileObjectType);
        _quicksandMapVisual = obj.GetComponent<QuicksandMapObjectVisual>();
        mapVisual = _quicksandMapVisual;
        Assert.IsNotNull(_quicksandMapVisual, $"Map Object Visual of {this} is null!");
    }
    public override void Neutralize() {
        _quicksandMapVisual.Expire();
    }
    public override string ToString() {
        return "Quicksand";
    }
}
