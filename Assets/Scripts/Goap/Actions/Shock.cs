using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

//public class Shock : GoapAction {

//    public Shock() : base(INTERACTION_TYPE.SHOCK) {
//        actionIconString = GoapActionStateDB.Entertain_Icon;
//        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
//        
//        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
//        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
//    }

//    #region Overrides
//    public override void Perform(ActualGoapNode goapNode) {
//        base.Perform(goapNode);
//        SetState("Shock Success", goapNode);
//    }
//    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
//        return Utilities.rng.Next(25, 51);
//    }
//    #endregion

//    #region Requirements
//    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) {
//        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
//        if (satisfied) {
//            return actor == poiTarget;
//        }
//        return false;
//    }
//    #endregion
//}

//public class ShockData : GoapActionData {
//    public ShockData() : base(INTERACTION_TYPE.SHOCK) {
//        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
//        requirementAction = Requirement;
//    }

//    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
//        return actor == poiTarget;
//    }
//}