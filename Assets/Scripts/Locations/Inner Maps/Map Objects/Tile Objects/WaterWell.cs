using System.Collections;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UnityEngine;
using Traits;

public class WaterWell : TileObject {

    public WaterWell() {
        Initialize(TILE_OBJECT_TYPE.WATER_WELL);
        traitContainer.RemoveTrait(this, "Flammable");
        traitContainer.AddTrait(this, "Immovable");
        traitContainer.AddTrait(this, "Frozen Immune");
        for (int i = 0; i < 10; i++) {
            traitContainer.AddTrait(this, "Wet", overrideDuration: 0);
        }
    }
    public WaterWell(SaveDataTileObject data) {
        
    }
    public override void OnPlacePOI() {
        base.OnPlacePOI();
        if(structureLocation.structureType == STRUCTURE_TYPE.POND || structureLocation.structureType == STRUCTURE_TYPE.OCEAN) {
            traitContainer.AddTrait(this, "Indestructible");
            name = "a Lake";
            AddAdvertisedAction(INTERACTION_TYPE.FISH);
        } else {
            AddAdvertisedAction(INTERACTION_TYPE.WELL_JUMP);
            AddAdvertisedAction(INTERACTION_TYPE.REPAIR);
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
        return structureLocation.structureType != STRUCTURE_TYPE.POND && structureLocation.structureType != STRUCTURE_TYPE.OCEAN;
    }
    public override bool CanBeDamaged() {
        return structureLocation.structureType != STRUCTURE_TYPE.POND && structureLocation.structureType != STRUCTURE_TYPE.OCEAN;
    }
    public override bool CanBeSelected() {
        return structureLocation != null && structureLocation.structureType != STRUCTURE_TYPE.POND && structureLocation.structureType != STRUCTURE_TYPE.OCEAN;
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

    #region Map Object State
    protected override void OnSetObjectAsUnbuilt() {
        if (structureLocation is CityCenter) {
            mapVisual.SetVisualAlpha(0f / 255f);
            SetSlotAlpha(0f / 255f);
            SetPOIState(POI_STATE.INACTIVE);
            AddAdvertisedAction(INTERACTION_TYPE.CRAFT_TILE_OBJECT);
            UnsubscribeListeners();
            //only difference with base is that this doesn't listen to check if it is no longer valid. Since we want water wells in the city center to be permanent.
        } else {
            base.OnSetObjectAsUnbuilt();
        }
    }
    #endregion
}
