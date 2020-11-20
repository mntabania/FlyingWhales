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
