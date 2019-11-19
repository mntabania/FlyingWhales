﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Table : TileObject {
    //private Character[] users;
    public TileBase usedAsset { get; private set; }
    public int food { get; private set; }
    //private int slots {
    //    get { return users.Length;}
    //}

    public Table(LocationStructure location) {
        SetStructureLocation(location);
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.DRINK, INTERACTION_TYPE.ASSAULT, INTERACTION_TYPE.DROP_FOOD, INTERACTION_TYPE.REPAIR, INTERACTION_TYPE.SIT };
        Initialize(TILE_OBJECT_TYPE.TABLE);
        SetFood(UnityEngine.Random.Range(20, 81));
        traitContainer.AddTrait(this, "Edible");
    }

    public Table(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.DRINK, INTERACTION_TYPE.ASSAULT, INTERACTION_TYPE.DROP_FOOD, INTERACTION_TYPE.REPAIR, INTERACTION_TYPE.SIT };
        Initialize(data);
        traitContainer.AddTrait(this, "Edible");
    }
    public void SetUsedAsset(TileBase usedAsset) {
        this.usedAsset = usedAsset;
    }

    #region Overrides
    public override void SetPOIState(POI_STATE state) {
        base.SetPOIState(state);
        //if (IsSlotAvailable()) {
        //    //if (GetActiveUserCount() > 0) {
        //    UpdateUsedTableAsset();
        //    //} else {
        //    //    gridTileLocation.parentAreaMap.UpdateTileObjectVisual(this); //update visual based on state
        //    //}
        //}
    }
    public override string ToString() {
        return "Table " + id.ToString();
    }
    public override void OnDoActionToObject(ActualGoapNode action) {
        base.OnDoActionToObject(action);
        switch (action.goapType) {
            case INTERACTION_TYPE.EAT:
            case INTERACTION_TYPE.DRINK:
            case INTERACTION_TYPE.SIT:
                AddUser(action.actor);
                break;

        }
    }
    public override void OnDoneActionToObject(ActualGoapNode action) {
        base.OnDoneActionToObject(action);
        switch (action.goapType) {
            case INTERACTION_TYPE.EAT:
            case INTERACTION_TYPE.DRINK:
            case INTERACTION_TYPE.SIT:
                RemoveUser(action.actor);
                break;

        }
    }
    public override void OnCancelActionTowardsObject(ActualGoapNode action) {
        base.OnCancelActionTowardsObject(action);
        switch (action.goapType) {
            case INTERACTION_TYPE.EAT:
            case INTERACTION_TYPE.DRINK:
            case INTERACTION_TYPE.SIT:
                RemoveUser(action.actor);
                break;

        }
    }
    public override bool CanBeReplaced() {
        return true;
    }
    protected override void OnPlaceObjectAtTile(LocationGridTile tile) {
        base.OnPlaceObjectAtTile(tile);
        if (tile.parentAreaMap.objectsTilemap.GetTile(tile.localPlace).name.Contains("Bartop")) {
            CreateNewGUS(new Vector2(0f, 0.5f), new Vector2(1f, 0.5f));
        } else {
            CreateNewGUS(Vector2.zero, new Vector2(0.5f, 0.5f));
        }
    }
    protected override void OnRemoveTileObject(Character removedBy, LocationGridTile removedFrom) {
        base.OnRemoveTileObject(removedBy, removedFrom);
        DestroyExistingGUS();
    }
    #endregion

    #region Users
    //private void AddUser(Character character) {
    //    for (int i = 0; i < users.Length; i++) {
    //        if (users[i] == null) {
    //            users[i] = character;
    //            UpdateUsedTableAsset();
    //            if (!IsSlotAvailable()) {
    //                SetPOIState(POI_STATE.INACTIVE); //if all slots in the table are occupied, set it as inactive
    //            }
    //            ////disable the character's marker
    //            //character.marker.SetActiveState(false);
    //            //place the character's marker his/her appropriate slot
    //            Vector3 pos = GetPositionForUser(GetActiveUserCount());
    //            character.marker.pathfindingAI.AdjustDoNotMove(1);

    //            Vector3 worldPos = character.marker.transform.TransformPoint(pos);
    //            //Debug.Log("Setting " + character.marker.name + "'s position to " + pos.ToString() + " world pos: " + worldPos.ToString());
    //            if (usedAsset.name.Contains("Bartop")) {
    //                character.marker.PlaceMarkerAt(pos, tile.parentAreaMap.objectsTilemap.GetTransformMatrix(tile.localPlace).rotation);
    //            } else {
    //                character.marker.PlaceMarkerAt(pos, gridTileLocation.centeredWorldLocation);
    //            }
    //            //Debug.Log(character.marker.name + "'s position is " + character.marker.transform.position.ToString());
    //            //character.marker.LookAt(this.gridTileLocation.worldLocation);
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
    //            ////enable the character's marker
    //            //character.marker.SetActiveState(true);
    //            if (GetActiveUserCount() > 0) {
    //                UpdateAllActiveUsersPosition();
    //            }
    //            UpdateUsedTableAsset();
    //            break;
    //        }
    //    }
    //}
    //private int GetActiveUserCount() {
    //    int count = 0;
    //    for (int i = 0; i < users.Length; i++) {
    //        if (users[i] != null) {
    //            count++;
    //        }
    //    }
    //    return count;
    //}
    //private Vector3 GetPositionForUser(int positionInTable) {
    //    Vector3 pos = gridTileLocation.localPlace;
    //    if (slots == 1) {
    //        //concerned with rotation in the 1 slot variant
    //        Matrix4x4 m = structureLocation.location.areaMap.objectsTilemap.GetTransformMatrix(gridTileLocation.localPlace);
    //        int rotation = (int)m.rotation.eulerAngles.z;
    //        if (usedAsset.name.Contains("Bartop")) {
    //            if (usedAsset.name.Contains("Left")) {
    //                if (rotation == 0 || rotation == 360) {
    //                    pos.x += 0.55f;
    //                    pos.y += 0.5f;
    //                } else if (rotation == 90) {
    //                    pos.x += 0.5f;
    //                    pos.y += 0.55f;
    //                } else if (rotation == 180) {
    //                    pos.x += 0.45f;
    //                    pos.y += 0.5f;
    //                } else if (rotation == 270 || rotation == -90) {
    //                    pos.x += 0.5f;
    //                    pos.y += 0.5f;
    //                }
    //            } else {
    //                if (rotation == 0 || rotation == 360) {
    //                    pos.x += 0.45f;
    //                    pos.y += 0.5f;
    //                } else if (rotation == 90) {
    //                    pos.x += 0.5f;
    //                    pos.y += 0.45f;
    //                } else if (rotation == 180) {
    //                    pos.x += 0.55f;
    //                    pos.y += 0.5f;
    //                } else if (rotation == 270 || rotation == -90) {
    //                    pos.x += 0.5f;
    //                    pos.y += 0.55f;
    //                }
    //            }
    //        } else {
    //            if (rotation == 0 || rotation == 360) {
    //                pos.x += 0.49f;
    //                pos.y += 0.2f;
    //            } else if (rotation == 90) {
    //                pos.x += 0.8f;
    //                pos.y += 0.5f;
    //            } else if (rotation == 180) {
    //                pos.x += 0.51f;
    //                pos.y += 0.8f;
    //            } else if (rotation == 270) {
    //                pos.x += 0.2f;
    //                pos.y += 0.51f;
    //            }
    //        }
    //    } else if (slots == 2) {
    //        //concerned with rotation in the 2 slot variant
    //        Matrix4x4 m = structureLocation.location.areaMap.objectsTilemap.GetTransformMatrix(gridTileLocation.localPlace);
    //        float rotation = m.rotation.eulerAngles.z / 90f;
    //        if (Utilities.IsEven((int)rotation)) {
    //            //table is vertical, I assume that if the table is vertical, it has a rotation of 0 degrees
    //            if (positionInTable == 1) {
    //                pos.x += 0.48f;
    //            } else {
    //                pos.x += 0.45f;
    //                pos.y += 1f;
    //            }
    //        } else {
    //            //table is horizontal, I assume that if the table is horizontal, it only has a rotation of 90 degrees
    //            if (positionInTable == 1) {
    //                pos.x += 1f;
    //                pos.y += 0.45f;
    //            } else {
    //                pos.y += 0.48f;
    //            }
    //        }
    //    } else if (slots == 4) {
    //        switch (positionInTable) {
    //            case 1:
    //                //left side
    //                pos.y += 0.53f;
    //                break;
    //            case 2:
    //                //right side
    //                pos.y += 0.53f;
    //                pos.x += 1f;
    //                break;
    //            case 3:
    //                //top
    //                pos.y += 1f;
    //                pos.x += 0.48f;
    //                break;
    //            case 4:
    //                //bottom
    //                pos.x += 0.48f;
    //                break;
    //            default:
    //                break;
    //        }
    //    }

    //    pos.z = 0;

    //    return pos;
    //}
    //private void UpdateAllActiveUsersPosition() {
    //    if (gridTileLocation == null) {
    //        return;
    //    }
    //    int userCount = 0;
    //    for (int i = 0; i < users.Length; i++) {
    //        Character currUser = users[i];
    //        if (currUser != null) {
    //            userCount++;
    //            Vector3 pos = GetPositionForUser(userCount);
    //            currUser.marker.PlaceMarkerAt(pos, gridTileLocation.centeredWorldLocation);
    //            //currUser.marker.LookAt(this.gridTileLocation.worldLocation);
    //        }
    //    }
    //}
    #endregion

    #region Food
    public void AdjustFood(int amount) {
        food += amount;
        if (food < 20) {
            traitContainer.RemoveTrait(this, "Edible"); //to stop advertising eat
        } else {
            traitContainer.AddTrait(this, "Edible"); //to advertise eat
        }
        if (food < 0) {
            food = 0;
        }
    }
    public void SetFood(int amount) {
        food = amount;
        if (food < 20) {
            traitContainer.RemoveTrait(this, "Edible"); //to stop advertising eat
        } else {
            traitContainer.AddTrait(this, "Edible"); //to advertise eat
        }
        if (food < 0) {
            food = 0;
        }
    }
    #endregion
}

public class SaveDataTable : SaveDataTileObject {
    public int food;

    public override void Save(TileObject tileObject) {
        base.Save(tileObject);
        Table obj = tileObject as Table;
        food = obj.food;
    }

    public override TileObject Load() {
        Table obj = base.Load() as Table;
        obj.SetFood(food);
        return obj;
    }
}