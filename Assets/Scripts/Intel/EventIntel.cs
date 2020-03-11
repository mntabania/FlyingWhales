using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

//public class EventIntel : Intel {

//	public Character actor { get; private set; }
//    public IPointOfInterest target { get; private set; }
//    public GoapAction action { get; private set; }
//    public GoapPlan plan { get; private set; }

//    public EventIntel(Character actor, GoapAction action) : base(actor, action) {
//        this.actor = actor;
//        //target = action.poiTarget;
//        //this.action = action;
//        //plan = action.parentPlan;
//        //SetIntelLog(action.currentState.descriptionLog);
//    }

//    public EventIntel(SaveDataEventIntel data) : base(data) {
//        if(data.actorID != -1) {
//            actor = CharacterManager.Instance.GetCharacterByID(data.actorID);
//        }
//        if (data.targetID != -1) {
//            if(data.targetPOIType == POINT_OF_INTEREST_TYPE.CHARACTER) {
//                target = CharacterManager.Instance.GetCharacterByID(data.targetID);
//            } else if (data.targetPOIType == POINT_OF_INTEREST_TYPE.ITEM) {
//                target = TokenManager.Instance.GetSpecialTokenByID(data.targetID);
//            } else if (data.targetPOIType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
//                target = InnerMapManager.Instance.GetTileObject(data.targetTileObjectType, data.targetID);
//            }
//        }
//    }
//}

//public class SaveDataEventIntel : SaveDataIntel {
//    public int actorID;
//    public int targetID;
//    public POINT_OF_INTEREST_TYPE targetPOIType;
//    public TILE_OBJECT_TYPE targetTileObjectType;

//    public override void Save(Intel intel) {
//        base.Save(intel);
//        EventIntel derivedIntel = intel as EventIntel;
//        if(derivedIntel.actor != null) {
//            actorID = derivedIntel.actor.id;
//        } else {
//            actorID = -1;
//        }

//        if(derivedIntel.target != null) {
//            targetID = derivedIntel.target.id;
//            targetPOIType = derivedIntel.target.poiType;
//            if(derivedIntel.target is TileObject) {
//                targetTileObjectType = (derivedIntel.target as TileObject).tileObjectType;
//            }
//        } else {
//            targetID = -1;
//        }
//    }

//    //public override Intel Load() {
//    //    EventIntel intel = base.Load() as EventIntel;
//    //    intel.Load(this);
//    //    return intel;
//    //}
//}
