using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;
using Traits;

public class Bed : TileObject {
    private Character[] bedUsers; //array of characters, currently using the bed

    public override Character[] users {
        get { return bedUsers.Where(x => x != null).ToArray(); }
    }

    public Bed() {
        //advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.SLEEP, INTERACTION_TYPE.ASSAULT, INTERACTION_TYPE.NAP, INTERACTION_TYPE.REPAIR };
        Initialize(TILE_OBJECT_TYPE.BED);
        AddAdvertisedAction(INTERACTION_TYPE.SLEEP);
        AddAdvertisedAction(INTERACTION_TYPE.NAP);
        bedUsers = new Character[2];
    }
    public Bed(SaveDataTileObject data) {
        //advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.SLEEP, INTERACTION_TYPE.ASSAULT, INTERACTION_TYPE.NAP, INTERACTION_TYPE.REPAIR };
        Initialize(data);
        AddAdvertisedAction(INTERACTION_TYPE.SLEEP);
        AddAdvertisedAction(INTERACTION_TYPE.NAP);
        bedUsers = new Character[2];
    }

    #region Overrides
    public override string ToString() {
        return $"Bed {id.ToString()}";
    }
    public override void SetPOIState(POI_STATE state) {
        base.SetPOIState(state);
        if (IsSlotAvailable()) {
            if (GetActiveUserCount() > 0) {
                UpdateUsedBedAsset();
            } else {
                if (gridTileLocation != null && mapVisual != null) {
                    mapVisual.UpdateTileObjectVisual(this);
                }
            }
        }
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
    public override void OnTileObjectGainedTrait(Trait trait) {
        base.OnTileObjectGainedTrait(trait);
        if (trait.name == "Burning") {
            //for (int i = 0; i < bedUsers.Length; i++) {
            //    if (bedUsers[i] != null) {
            //        Character currUser = bedUsers[i];
            //    }
            //}
            Messenger.Broadcast(Signals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, this as IPointOfInterest, "bed is burning");
        }
    }
    public virtual bool CanBeReplaced() {
        return true;
    }
    #endregion

    #region Users
    public bool IsSlotAvailable() {
        for (int i = 0; i < bedUsers.Length; i++) {
            if (bedUsers[i] == null) {
                return true; //there is an available slot
            }
        }
        return false;
    }
    public override void AddUser(Character character) {
        for (int i = 0; i < bedUsers.Length; i++) {
            if (bedUsers[i] == null) {
                bedUsers[i] = character;
                character.SetTileObjectLocation(this);
                UpdateUsedBedAsset();
                if (!IsSlotAvailable()) {
                    SetPOIState(POI_STATE.INACTIVE); //if all slots in the bed are occupied, set it as inactive
                }
                //disable the character's marker
                character.marker.SetVisualState(false);
                Messenger.Broadcast(Signals.ADD_TILE_OBJECT_USER, GetBase(), character);
                break;
            }
        }
    }
    public override bool RemoveUser(Character character) {
        for (int i = 0; i < bedUsers.Length; i++) {
            if (bedUsers[i] == character) {
                bedUsers[i] = null;
                character.SetTileObjectLocation(null);
                UpdateUsedBedAsset();
                if (IsSlotAvailable()) {
                    SetPOIState(POI_STATE.ACTIVE); //if a slots in the bed is unoccupied, set it as active
                }
                //enable the character's marker
                character.marker.SetVisualState(true);
                if (character.gridTileLocation != null && character.traitContainer.HasTrait("Paralyzed")) {
                    //When a paralyzed character awakens, place it on a nearby adjacent empty tile in the same Structure
                    LocationGridTile gridTile = character.gridTileLocation.GetNearestUnoccupiedTileFromThis();
                    character.marker.PlaceMarkerAt(gridTile);
                }
                Messenger.Broadcast(Signals.REMOVE_TILE_OBJECT_USER, GetBase(), character);
                return true;
            }
        }
        return false;
    }
    public int GetActiveUserCount() {
        int count = 0;
        for (int i = 0; i < bedUsers.Length; i++) {
            if (bedUsers[i] != null) {
                count++;
            }
        }
        return count;
    }
    private bool IsInThisBed(Character character) {
        for (int i = 0; i < bedUsers.Length; i++) {
            if (bedUsers[i] == character) {
                return true;
            }
        }
        return false;
    }
    private Character GetNextCharacterInCycle(Character startingPoint) {
        int startingIndex = 0;
        int currIndex = 0;
        if (startingPoint != null) {
            for (int i = 0; i < bedUsers.Length; i++) {
                Character currUser = bedUsers[i];
                if (currUser == startingPoint) {
                    startingIndex = i;
                    break;
                }
            }
            currIndex = startingIndex + 1;
        }
       
        
        while (true) {
            if (currIndex == bedUsers.Length) {
                currIndex = 0;
            }
            if (bedUsers[currIndex] != null) {
                return bedUsers[currIndex];
            }
            currIndex++;
        }
    }
    #endregion

    #region Inquiry
    public bool CanSleepInBed(Character character) {
        for (int i = 0; i < users.Length; i++) {
            if (users[i] != null) {
                Character user = users[i];
                RELATIONSHIP_EFFECT relEffect = character.relationshipContainer.GetRelationshipEffectWith(user);
                if(character.relationshipContainer.HasRelationshipWith(user) == false 
                   || character.relationshipContainer.IsEnemiesWith(user) 
                   || character.relationshipContainer.HasOpinionLabelWithCharacter(user, RelationshipManager.Acquaintance)) {
                    return false;
                }
            }
        }
        return true;
    }
    #endregion

    private void UpdateUsedBedAsset() {
        if (gridTileLocation == null) {
            return;
        }
        mapVisual.UpdateTileObjectVisual(this);
        //int userCount = GetActiveUserCount();
        //if (userCount == 1) {
        //    gridTileLocation.parentAreaMap.UpdateTileObjectVisual(this, gridTileLocation.parentAreaMap.bed1SleepingVariant);
        //} else if (userCount == 2) {
        //    gridTileLocation.parentAreaMap.UpdateTileObjectVisual(this, gridTileLocation.parentAreaMap.bed2SleepingVariant);
        //}
        //the asset will revert to no one sleeping once the bed is set to active again
    }
}
