using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EdiblePlant : TileObject {

    private const int Replenishment_Countdown = 96;

    public EdiblePlant() {
        //advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT, INTERACTION_TYPE.HARVEST_PLANT };
        Initialize(TILE_OBJECT_TYPE.EDIBLE_PLANT, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.HARVEST_PLANT);

        traitContainer.AddTrait(this, "Edible");
    }
    public EdiblePlant(SaveDataTileObject data) {
        //advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT, INTERACTION_TYPE.HARVEST_PLANT };
        Initialize(data, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.HARVEST_PLANT);
        traitContainer.AddTrait(this, "Edible");
    }

    #region Overrides
    public override void OnDoActionToObject(ActualGoapNode action) {
        base.OnDoActionToObject(action);
        //SetPOIState(POI_STATE.INACTIVE);
        //ScheduleCooldown(action);
    }
    //public override List<GoapAction> AdvertiseActionsToActor(Character actor, List<INTERACTION_TYPE> actorAllowedInteractions) {
    //    if (actor.GetTrait("Herbivore") != null) { //Herbivores only
    //        return base.AdvertiseActionsToActor(actor, actorAllowedInteractions);
    //    }
    //    return null;
    //}
    public override void SetPOIState(POI_STATE state) {
        base.SetPOIState(state);
        if (gridTileLocation != null) {
            if(mapVisual != null) {
                mapVisual.UpdateTileObjectVisual(this); //update visual based on state
            }
            if (!IsAvailable()) {
                ScheduleCooldown();
            }
        }
        
    }
    public override string ToString() {
        return $"Edible Plant {id}";
    }
    #endregion

    private void ScheduleCooldown() {
        GameDate dueDate = GameManager.Instance.Today();
        dueDate.AddTicks(Replenishment_Countdown);
        //Debug.Log("Will set " + this.ToString() + " as active again on " + GameManager.Instance.ConvertDayToLogString(dueDate));
        SchedulingManager.Instance.AddEntry(dueDate, () => SetPOIState(POI_STATE.ACTIVE), this);
    }
}
