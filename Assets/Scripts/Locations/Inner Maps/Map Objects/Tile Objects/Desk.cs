using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Desk : TileObject {
    //private Character[] users;
    public Desk() {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT, INTERACTION_TYPE.REPAIR, INTERACTION_TYPE.SIT };
        Initialize(TILE_OBJECT_TYPE.DESK);
        //users = new Character[1];
    }
    public Desk(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT, INTERACTION_TYPE.REPAIR, INTERACTION_TYPE.SIT };
        Initialize(data);
    }

    #region Overrides
    public override string ToString() {
        return $"Desk {id}";
    }
    public override void SetPOIState(POI_STATE state) {
        base.SetPOIState(state);
        if (gridTileLocation != null && mapVisual != null) {
           mapVisual.UpdateTileObjectVisual(this); //update visual based on state
        }
    }
    public override void OnDoActionToObject(ActualGoapNode action) {
        base.OnDoActionToObject(action);
        switch (action.goapType) {
            case INTERACTION_TYPE.SIT:
                AddUser(action.actor);
                break;

        }
    }
    public override void OnDoneActionToObject(ActualGoapNode action) {
        base.OnDoneActionToObject(action);
        switch (action.goapType) {
            case INTERACTION_TYPE.SIT:
                RemoveUser(action.actor);
                break;

        }
    }
    public override void OnCancelActionTowardsObject(ActualGoapNode action) {
        base.OnCancelActionTowardsObject(action);
        switch (action.goapType) {
            case INTERACTION_TYPE.SIT:
                RemoveUser(action.actor);
                break;

        }
    }
    public override bool CanBeReplaced() {
        return true;
    }
    //private bool IsSlotAvailable() {
    //    for (int i = 0; i < users.Length; i++) {
    //        if (users[i] == null) {
    //            return true; //there is an available slot
    //        }
    //    }
    //    return false;
    //}
    //protected override void OnDestroyTileObject() {
    //    base.OnDestroyTileObject();
    //    for (int i = 0; i < users.Length; i++) {
    //        Character character = users[i];
    //        if (character != null) {
    //            character.currentAction.StopAction();
    //        }
    //    }
    //}
    #endregion

    //#region Users
    //private void AddUser(Character character) {
    //    for (int i = 0; i < users.Length; i++) {
    //        if (users[i] == null) {
    //            users[i] = character;
    //            if (!IsSlotAvailable()) {
    //                SetPOIState(POI_STATE.INACTIVE); //if all slots in the table are occupied, set it as inactive
    //            }
    //            ////disable the character's marker
    //            //character.marker.SetActiveState(false);
    //            //place the character's marker his/her appropriate slot
    //            Vector3 pos = GetPositionForUser();
    //            character.marker.pathfindingAI.AdjustDoNotMove(1);

    //            Vector3 worldPos = character.marker.transform.TransformPoint(pos);
    //            character.marker.PlaceMarkerAt(pos, gridTileLocation.centeredWorldLocation);
    //            break;
    //        }
    //    }
    //}
    //private void RemoveUser(Character character) {
    //    for (int i = 0; i < users.Length; i++) {
    //        if (users[i] == character) {
    //            users[i] = null;
    //            if (IsSlotAvailable()) {
    //                SetPOIState(POI_STATE.ACTIVE); //if a slot in the table is unoccupied, set it as active
    //            }
    //            character.marker.pathfindingAI.AdjustDoNotMove(-1);
    //            break;
    //        }
    //    }
    //}
    //#endregion

    //private Vector3 GetPositionForUser() {
    //    Vector3 pos = gridTileLocation.localPlace;
    //    //concerned with rotation in the 1 slot variant
    //    Matrix4x4 m = structureLocation.location.areaMap.objectsTilemap.GetTransformMatrix(gridTileLocation.localPlace);
    //    int rotation = (int)m.rotation.eulerAngles.z;
    //    //if rotation is 0
    //    if (rotation == 0 || rotation == 360) {
    //        pos.x += 0.49f;
    //        pos.y += 0.2f;
    //    } else if (rotation == 90) {
    //        pos.x += 0.8f;
    //        pos.y += 0.5f;
    //    } else if (rotation == 180) {
    //        pos.x += 0.51f;
    //        pos.y += 0.8f;
    //    } else if (rotation == 270) {
    //        pos.x += 0.2f;
    //        pos.y += 0.51f;
    //    }

    //    pos.z = 0;

    //    return pos;
    //}
}