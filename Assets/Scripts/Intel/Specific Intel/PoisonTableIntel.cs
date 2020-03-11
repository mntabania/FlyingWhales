using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

//public class PoisonTableIntel : EventIntel {

//    public GoapAction eatAtTableAction { get; private set; }
//    public Table poisonedTable { get; private set; }
//    public Dwelling targetDwelling { get; private set; }

//    public PoisonTableIntel(Character actor, GoapAction action) : base(actor, action) {
//        //if (action.result == InteractionManager.Goap_State_Success) {
//        //    poisonedTable = this.action.poiTarget as Table;
//        //    targetDwelling = poisonedTable.gridTileLocation.structure as Dwelling;
//        //    Messenger.AddListener<Character, GoapAction>(Signals.CHARACTER_DID_ACTION, OnCharacterDidAction);
//        //}
//    }

//    public PoisonTableIntel(SaveDataPoisonTableIntel data) : base(data) {
//        if (data.poisonedTableID != -1) {
//            //NPCSettlement npcSettlement = LandmarkManager.Instance.GetAreaByID(data.poisonedTableAreaID);
//            poisonedTable = InnerMapManager.Instance.GetTileObject(TILE_OBJECT_TYPE.TABLE, data.poisonedTableID) as Table;
//        }

//        if (data.targetDwellingID != -1) {
//            NPCSettlement npcSettlement = LandmarkManager.Instance.GetAreaByID(data.targetDwellingAreaID);
//            targetDwelling = npcSettlement.GetStructureByID(STRUCTURE_TYPE.DWELLING, data.targetDwellingID) as Dwelling;
//        }
//    }

//    private void OnCharacterDidAction(Character character, GoapAction action) {
//        //once this intel is made, it should listen for when a character eats at the table that was poisoned, and store that action
//        //if (action.goapType == INTERACTION_TYPE.EAT && action.poiTarget == poisonedTable) {
//        //    eatAtTableAction = action;
//        //}
//    }

//    public override void OnIntelExpire() {
//        base.OnIntelExpire();
//        if (Messenger.eventTable.ContainsKey(Signals.CHARACTER_DID_ACTION)) {
//            Messenger.RemoveListener<Character, GoapAction>(Signals.CHARACTER_DID_ACTION, OnCharacterDidAction);
//        }
//    }

//}

//public class SaveDataPoisonTableIntel : SaveDataEventIntel {
//    public int poisonedTableID;
//    public int poisonedTableAreaID;
//    public int targetDwellingID;
//    public int targetDwellingAreaID;

//    public override void Save(Intel intel) {
//        base.Save(intel);
//        PoisonTableIntel derivedIntel = intel as PoisonTableIntel;
//        if (derivedIntel.poisonedTable != null) {
//            poisonedTableID = derivedIntel.poisonedTable.id;
//            poisonedTableAreaID = derivedIntel.poisonedTable.gridTileLocation.structure.location.id;
//        } else {
//            poisonedTableID = -1;
//        }
//        if (derivedIntel.targetDwelling != null) {
//            targetDwellingID = derivedIntel.targetDwelling.id;
//            targetDwellingAreaID = derivedIntel.targetDwelling.location.id;
//        } else {
//            targetDwellingID = -1;
//        }
//    }

//    //public override Intel Load() {
//    //    PoisonTableIntel intel = base.Load() as PoisonTableIntel;
//    //    intel.Load(this);
//    //    return intel;
//    //}
//}