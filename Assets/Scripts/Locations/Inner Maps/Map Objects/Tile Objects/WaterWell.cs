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
        if(structureLocation.structureType == STRUCTURE_TYPE.POND || structureLocation.structureType == STRUCTURE_TYPE.OCEAN) {
            name = "a Lake";
            AddAdvertisedAction(INTERACTION_TYPE.FISH);
        } else {
            AddAdvertisedAction(INTERACTION_TYPE.WELL_JUMP);
            AddAdvertisedAction(INTERACTION_TYPE.REPAIR);
        }
        traitContainer.RemoveTrait(this, "Flammable");
        traitContainer.AddTrait(this, "Immovable");
        for (int i = 0; i < 10; i++) {
            traitContainer.AddTrait(this, "Wet", overrideDuration: 0);    
        }
        Messenger.AddListener(Signals.HOUR_STARTED, HourStarted);
    }
    public override void OnDestroyPOI() {
        base.OnDestroyPOI();
        Messenger.RemoveListener(Signals.HOUR_STARTED, HourStarted);
    }
    public override bool CanBeAffectedByElementalStatus(string traitName) {
        if (traitName == "Wet") {
            return true; //allow water well to be wet.
        }
        return structureLocation.structureType != STRUCTURE_TYPE.POND && 
               structureLocation.structureType != STRUCTURE_TYPE.OCEAN;
    }
    public override bool CanBeDamaged() {
        return structureLocation.structureType != STRUCTURE_TYPE.POND && 
               structureLocation.structureType != STRUCTURE_TYPE.OCEAN;
    }
    public override bool CanBeSelected() {
        return structureLocation.structureType != STRUCTURE_TYPE.POND && 
               structureLocation.structureType != STRUCTURE_TYPE.OCEAN;
    }
    public override string ToString() {
        return $"Well {id.ToString()}";
    }

    #region Listeners
    private void HourStarted() {
        if (traitContainer.stacks.ContainsKey("Wet") && traitContainer.stacks["Wet"] < 10) {
            traitContainer.AddTrait(this, "Wet", overrideDuration: 0);    
        }
        
    }
    #endregion
}
