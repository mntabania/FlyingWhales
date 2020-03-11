using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;

public class WaterWell : TileObject {

    public WaterWell() {
        Initialize(TILE_OBJECT_TYPE.WATER_WELL);
        traitContainer.RemoveTrait(this, "Flammable");
        //Wet wet = new Wet {ticksDuration = 0};
        traitContainer.AddTrait(this, "Wet", overrideDuration: 0);
    }
    public WaterWell(SaveDataTileObject data) {
        Initialize(data);
    }
    public override void OnPlacePOI() {
        base.OnPlacePOI();
        advertisedActions = structureLocation.structureType != STRUCTURE_TYPE.POND && structureLocation.structureType != STRUCTURE_TYPE.OCEAN ? 
            new List<INTERACTION_TYPE>() { INTERACTION_TYPE.WELL_JUMP, INTERACTION_TYPE.REPAIR } : new List<INTERACTION_TYPE>();
    }
    public override bool CanBeDamaged() {
        return structureLocation.structureType != STRUCTURE_TYPE.POND && structureLocation.structureType != STRUCTURE_TYPE.OCEAN;
    }
    public override string ToString() {
        return $"Well {id.ToString()}";
    }
}
