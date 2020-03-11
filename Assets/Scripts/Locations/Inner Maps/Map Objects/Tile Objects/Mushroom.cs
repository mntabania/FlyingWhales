using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Mushroom : TileObject {

    private const int Replenishment_Countdown = 96;

    public Mushroom() {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT, };
        Initialize(TILE_OBJECT_TYPE.MUSHROOM);
        traitContainer.AddTrait(this, "Edible");
    }

    public Mushroom(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT, };
        Initialize(data);
        traitContainer.AddTrait(this, "Edible");
    }

    #region Overrides
    public override void OnDoActionToObject(ActualGoapNode action) {
        base.OnDoActionToObject(action);
        //SetPOIState(POI_STATE.INACTIVE);
        //ScheduleCooldown(action);
    }
    public override void SetPOIState(POI_STATE state) {
        base.SetPOIState(state);
        if (gridTileLocation != null) {
            //Debug.Log(GameManager.Instance.TodayLogString() + "Set " + this.ToString() + "' state to " + state.ToString());
            if(mapVisual != null) {
                mapVisual.UpdateTileObjectVisual(this); //update visual based on state
            }
            if (!IsAvailable()) {
                ScheduleCooldown();
            }
        }

    }
    public override string ToString() {
        return $"Mushroom {id}";
    }
    #endregion

    private void ScheduleCooldown() {
        GameDate dueDate = GameManager.Instance.Today();
        dueDate.AddTicks(Replenishment_Countdown);
        //Debug.Log("Will set " + this.ToString() + " as active again on " + GameManager.Instance.ConvertDayToLogString(dueDate));
        SchedulingManager.Instance.AddEntry(dueDate, () => SetPOIState(POI_STATE.ACTIVE), this);
    }
}
