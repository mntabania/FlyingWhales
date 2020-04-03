using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UnityEngine.Assertions;

public class Quicksand : TileObject {

    private QuicksandMapObjectVisual _quicksandMapVisual;
    public override string neutralizer => "Earth Master";
    
    public Quicksand() {
        Initialize(TILE_OBJECT_TYPE.QUICKSAND, false);
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
    public override void OnPlacePOI() {
        base.OnPlacePOI();
        traitContainer.AddTrait(this, "Dangerous");
        traitContainer.RemoveTrait(this, "Flammable");
    }
    public override string ToString() {
        return "Quicksand";
    }
}
