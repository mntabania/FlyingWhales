using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;
using Traits;

public class Bed : BaseBed {
    
    public Bed() : base(2) {
        Initialize(TILE_OBJECT_TYPE.BED);
        AddAdvertisedAction(INTERACTION_TYPE.SLEEP);
        AddAdvertisedAction(INTERACTION_TYPE.NAP);
    }
    public Bed(SaveDataTileObject data) : base(data, 2) { }

    #region Overrides
    public override string ToString() {
        return $"Bed {id.ToString()}";
    }
    public override void OnDoActionToObject(ActualGoapNode action) {
        base.OnDoActionToObject(action);
        switch (action.goapType) {
            case INTERACTION_TYPE.SLEEP:
            case INTERACTION_TYPE.NAP:
                AddUser(action.actor);
                break;
            case INTERACTION_TYPE.MAKE_LOVE:
                AddUser(action.actor);
                AddUser(action.poiTarget as Character);
                break;
        }
    }
    public override void OnDoneActionToObject(ActualGoapNode action) {
        base.OnDoneActionToObject(action);
        switch (action.goapType) {
            case INTERACTION_TYPE.SLEEP:
            case INTERACTION_TYPE.NAP:
                RemoveUser(action.actor);
                break;
            case INTERACTION_TYPE.MAKE_LOVE:
                RemoveUser(action.actor);
                RemoveUser(action.poiTarget as Character);
                break;
        }
    }
    public override void OnCancelActionTowardsObject(ActualGoapNode action) {
        base.OnCancelActionTowardsObject(action);
        switch (action.goapType) {
            case INTERACTION_TYPE.SLEEP:
            case INTERACTION_TYPE.NAP:
                RemoveUser(action.actor);
                break;
            case INTERACTION_TYPE.MAKE_LOVE:
                RemoveUser(action.actor);
                RemoveUser(action.poiTarget as Character);
                break;
        }
    }
    protected override bool AddUser(Character character) {
        if (base.AddUser(character)) {
            if (!IsSlotAvailable()) {
                SetPOIState(POI_STATE.INACTIVE); //if all slots in the bed are occupied, set it as inactive
            }
            return true;
        }
        return false;
    }
    public override bool RemoveUser(Character character) {
        if (base.RemoveUser(character)) {
            if (IsSlotAvailable()) {
                //Must not set as active when bed is burning
                if (!traitContainer.HasTrait("Burning")) {
                    SetPOIState(POI_STATE.ACTIVE); //if a slots in the bed is unoccupied, set it as active
                }
            }
            return true;
        }
        return false;
    }
    protected override void OnSetObjectAsUnbuilt() {
        base.OnSetObjectAsUnbuilt();
        AddAdvertisedAction(INTERACTION_TYPE.CRAFT_FURNITURE_STONE);
        AddAdvertisedAction(INTERACTION_TYPE.CRAFT_FURNITURE_WOOD);
    }
    protected override void OnSetObjectAsBuilt() {
        base.OnSetObjectAsBuilt();
        RemoveAdvertisedAction(INTERACTION_TYPE.CRAFT_FURNITURE_STONE);
        RemoveAdvertisedAction(INTERACTION_TYPE.CRAFT_FURNITURE_WOOD);
    }
    #endregion

    #region Inquiry
    public override bool CanUseBed(Character character) {
        bool canUseBed = base.CanUseBed(character);
        if (canUseBed) {
            for (int i = 0; i < users.Length; i++) {
                if (users[i] != null) {
                    Character user = users[i];
                    if(character.relationshipContainer.HasRelationshipWith(user) == false 
                       || character.relationshipContainer.IsEnemiesWith(user) 
                       || character.relationshipContainer.HasOpinionLabelWithCharacter(user, RelationshipManager.Acquaintance)) {
                        return false;
                    }
                }
            }    
        }
        return true;
    }
    #endregion
}
