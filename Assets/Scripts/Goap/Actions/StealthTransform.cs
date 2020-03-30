using System.Collections;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UnityEngine;
using Traits;

public class StealthTransform : GoapAction {
    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.INDIRECT; } }

    public StealthTransform() : base(INTERACTION_TYPE.STEALTH_TRANSFORM) {
        actionLocationType = ACTION_LOCATION_TYPE.RANDOM_LOCATION;
        actionIconString = GoapActionStateDB.No_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
    }

    #region Overrides
    public override LocationStructure GetTargetStructure(ActualGoapNode node) {
        object[] otherData = node.otherData;
        if (otherData != null && otherData.Length == 1) {
            //if (otherData[0] is Dwelling) {
            //    return otherData[0] as Dwelling;
            //} else 
            if (otherData[0] is LocationStructure) {
                return otherData[0] as LocationStructure;
            }
        }
        return null;
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Transform Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        return 1;
    }
    #endregion

    #region Requirement
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            return actor == poiTarget;
        }
        return false;
    }
    #endregion

    #region State Effects
    //public void PreVisitSuccess(ActualGoapNode goapNode) {
    //goapNode.descriptionLog.AddToFillers(null, goapNode.targetStructure.GetNameRelativeTo(goapNode.actor), LOG_IDENTIFIER.LANDMARK_1);
    //}
    public void AfterTransformSuccess(ActualGoapNode goapNode) {
        //goapNode.actor.trapStructure.SetStructureAndDuration(goapNode.targetStructure, GameManager.Instance.GetTicksBasedOnHour(2) + GameManager.Instance.GetTicksBasedOnMinutes(30));
        Lycanthrope lycanthrope = goapNode.actor.traitContainer.GetNormalTrait<Lycanthrope>("Lycanthrope");
        if(lycanthrope != null) {
            lycanthrope.CheckIfAlone();
        }
    }
    #endregion
}
