using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class Sit : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.DIRECT; } }

    public Sit() : base(INTERACTION_TYPE.SIT) {
        actionIconString = GoapActionStateDB.No_Icon;
        showNotification = false;
        shouldAddLogs = false;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Sit Success", goapNode);
       
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
        IPointOfInterest poiTarget = node.poiTarget;
        if (goapActionInvalidity.isInvalid == false) {
            if (poiTarget.IsAvailable() == false) {
                goapActionInvalidity.isInvalid = true;
                goapActionInvalidity.stateName = "Sit Fail";
            }
        }
        return goapActionInvalidity;
    }
    #endregion

    #region Effects
    //public void PerTickSitSuccess(ActualGoapNode goapNode) {
    //    goapNode.actor.needsComponent.AdjustStamina(0.3f);
    //}
    //public void PreSitFail(ActualGoapNode goapNode) {
    //    goapNode.descriptionLog.AddToFillers(null, goapNode.poiTarget.name, LOG_IDENTIFIER.STRING_1);
    //}
    //public void PreTargetMissing(ActualGoapNode goapNode) {
    //    goapNode.descriptionLog.AddToFillers(null, goapNode.poiTarget.name, LOG_IDENTIFIER.STRING_1);
    //}
    #endregion

    #region Requirement
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (poiTarget.gridTileLocation != null) { //&& poiTarget.gridTileLocation.structure.structureType == STRUCTURE_TYPE.DWELLING
                return poiTarget.IsAvailable();
            }
        }
        return false;
    }
    #endregion
}

public class SitData : GoapActionData {
    public SitData() : base(INTERACTION_TYPE.SIT) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (poiTarget.gridTileLocation != null) { //&& poiTarget.gridTileLocation.structure.structureType == STRUCTURE_TYPE.DWELLING
            return poiTarget.IsAvailable();
        }
        return false;
    }
}