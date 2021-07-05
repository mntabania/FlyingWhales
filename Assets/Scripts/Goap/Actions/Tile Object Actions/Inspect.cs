using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class Inspect : GoapAction {

    public Inspect() : base(INTERACTION_TYPE.INSPECT) {
        actionIconString = GoapActionStateDB.Work_Icon;
        
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Work};
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Inspect Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = string.Empty;
#endif
        if (target.gridTileLocation != null && actor.movementComponent.structuresToAvoid.Contains(target.gridTileLocation.structure)) {
            if (!actor.partyComponent.hasParty) {
                //target is at structure that character is avoiding
#if DEBUG_LOG
                costLog += $" +2000(Location of target is in avoid structure)";
                actor.logComponent.AppendCostLog(costLog);
#endif
                return 2000;
            }
        }
#if DEBUG_LOG
        costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 10;
    }
#endregion

#region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
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