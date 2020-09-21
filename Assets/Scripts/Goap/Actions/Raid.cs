//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;  
//using Traits;
//using Inner_Maps;
//using Inner_Maps.Location_Structures;
//using Locations.Settlements;

//public class Raid : GoapAction {

//    public Raid() : base(INTERACTION_TYPE.RAID) {
//        actionIconString = GoapActionStateDB.No_Icon;
//        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
//        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
//    }

//    #region Overrides
//    public override void Perform(ActualGoapNode goapNode) {
//        base.Perform(goapNode);
//        SetState("Raid Success", goapNode);
//    }
//    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
//        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
//        actor.logComponent.AppendCostLog(costLog);
//        return 10;
//    }
//    #endregion

//    #region Requirements
//    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData) {
//        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
//        if (satisfied) {
//            return !actor.partyComponent.hasParty;
//        }
//        return false;
//    }
//    #endregion

//    #region State Effects
//    public void AfterRaidSuccess(ActualGoapNode goapNode) {
//        OtherData[] otherData = goapNode.otherData;
//        if (otherData != null && otherData.Length == 2 && otherData[0].obj is BaseSettlement targetSettlement) {
//            Party party = CharacterManager.Instance.CreateNewParty(PARTY_QUEST_TYPE.Raid, goapNode.actor);
//            RaidParty raidParty = party as RaidParty;
//            raidParty.SetTargetSettlement(targetSettlement);
//        }
//    }
//    #endregion

//}