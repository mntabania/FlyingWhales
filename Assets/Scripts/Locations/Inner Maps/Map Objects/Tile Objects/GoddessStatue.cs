using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoddessStatue : TileObject {

    public GoddessStatue() {
        Initialize(TILE_OBJECT_TYPE.GODDESS_STATUE);
        AddAdvertisedAction(INTERACTION_TYPE.PRAY_TILE_OBJECT);
        traitContainer.RemoveTrait(this, "Flammable");
    }
    public GoddessStatue(SaveDataTileObject data) : base(data) {
    }

    public override void SetPOIState(POI_STATE state) {
        base.SetPOIState(state);
        if (gridTileLocation != null && mapVisual != null) {
            mapVisual.UpdateTileObjectVisual(this); //update visual based on state
        }

    }
    //public override void OnPlacePOI() {
    //    base.OnPlacePOI();
    //    SetPOIState(POI_STATE.INACTIVE);
    //}
    public override string ToString() {
        return $"Goddess Statue {id.ToString()}";
    }
}
