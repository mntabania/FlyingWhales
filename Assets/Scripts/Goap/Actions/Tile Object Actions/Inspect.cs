using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class Inspect : GoapAction {

    public Inspect() : base(INTERACTION_TYPE.INSPECT) {
        actionIconString = GoapActionStateDB.Work_Icon;
        
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON };
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Inspect Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            return poiTarget.IsAvailable() && poiTarget.gridTileLocation != null;
        }
        return false;
    }
    #endregion

    #region State Effects
    public void PreInspectSuccess(ActualGoapNode goapNode) {
        //goapNode.descriptionLog.AddToFillers(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
        goapNode.descriptionLog.AddToFillers(goapNode.poiTarget, goapNode.poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    public void AfterInspectSuccess(ActualGoapNode goapNode) {
        if (goapNode.poiTarget is TileObject to) {
            goapNode.actor.defaultCharacterTrait.AddAlreadyInspectedObject(to);
            to.OnInspect(goapNode.actor);
        }
    }
    //public void PreTargetMissing(ActualGoapNode goapNode) {
    //    goapNode.descriptionLog.AddToFillers(poiTarget, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    //    goapNode.descriptionLog.AddToFillers(actor.currentStructure.location, actor.currentStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    //}
    //public void AfterTargetMissing(ActualGoapNode goapNode) {
    //    actor.RemoveAwareness(poiTarget);
    //}
    #endregion
}

public class InspectData : GoapActionData {
    public InspectData() : base(INTERACTION_TYPE.INSPECT) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return poiTarget.IsAvailable() && poiTarget.gridTileLocation != null;
    }
}