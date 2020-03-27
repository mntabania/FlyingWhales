using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;

public class WaterWell : TileObject {

    public WaterWell() {
        Initialize(TILE_OBJECT_TYPE.WATER_WELL);
    }
    public WaterWell(SaveDataTileObject data) {
        Initialize(data);
    }
    public override void OnPlacePOI() {
        base.OnPlacePOI();
        if(structureLocation.structureType != STRUCTURE_TYPE.POND && structureLocation.structureType != STRUCTURE_TYPE.OCEAN) {
            AddAdvertisedAction(INTERACTION_TYPE.WELL_JUMP);
            AddAdvertisedAction(INTERACTION_TYPE.REPAIR);
        }
        traitContainer.RemoveTrait(this, "Flammable");
        traitContainer.AddTrait(this, "Wet", overrideDuration: 0);
    }
    public override bool CanBeAffectedByElementalStatus(string traitName) {
        if (traitName == "Wet") {
            return true; //allow water well to be wet.
        }
        return structureLocation.structureType != STRUCTURE_TYPE.POND && structureLocation.structureType != STRUCTURE_TYPE.OCEAN;
    }
    public override bool CanBeDamaged() {
        return structureLocation.structureType != STRUCTURE_TYPE.POND && structureLocation.structureType != STRUCTURE_TYPE.OCEAN;
    }
    public override string ToString() {
        return $"Well {id.ToString()}";
    }
}
