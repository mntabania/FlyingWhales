
using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class CarryRestrained : GoapAction {
    
    private readonly Precondition _carryPrecondition;
    
    public CarryRestrained() : base(INTERACTION_TYPE.CARRY_RESTRAINED) {
        actionIconString = GoapActionStateDB.Work_Icon;
        canBeAdvertisedEvenIfTargetIsUnavailable = true;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        logTags = new[] {LOG_TAG.Work};
        _carryPrecondition = new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Restrained", false, GOAP_EFFECT_TARGET.TARGET), TargetIsRestrainedOrDead);
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        // SetPrecondition(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Restrained", false, GOAP_EFFECT_TARGET.TARGET), TargetIsRestrainedOrDead);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_POI, conditionKey = "Carry Restrained", isKeyANumber = false, target = GOAP_EFFECT_TARGET.TARGET });
    }
    public override Precondition GetPrecondition(Character actor, IPointOfInterest target, OtherData[] otherData, JOB_TYPE jobType, out bool isOverridden) {
        if (!target.traitContainer.HasTrait("Hibernating")) {
            Precondition p = _carryPrecondition;
            isOverridden = true;
            return p;
        }
        return base.GetPrecondition(actor, target, otherData, jobType, out isOverridden);
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Carry Success", goapNode);
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;

        string stateName = "Target Missing";
        bool defaultTargetMissing = TargetMissingForCarry(node);
        GoapActionInvalidity goapActionInvalidity = new GoapActionInvalidity(defaultTargetMissing, stateName, "target_unavailable");
        if (goapActionInvalidity.isInvalid == false) {
            if(poiTarget is Character) {
                if ((poiTarget as Character).carryComponent.IsNotBeingCarried() == false) {
                    goapActionInvalidity.isInvalid = true;
                }
            }
        }
        if (goapActionInvalidity.isInvalid == false) {
            if (node.associatedJobType == JOB_TYPE.MONSTER_ABDUCT) {
                if (node.poiTarget is Character targetCharacter && targetCharacter.isDead) {
                    //Cannot abduct dead characters
                    goapActionInvalidity.isInvalid = true;
                    goapActionInvalidity.reason = "target_dead";
                }
            }
        }
        return goapActionInvalidity;
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}:";
        //if (job.jobType == JOB_TYPE.MOVE_CHARACTER) {
        //    //If the job is move character and the target can move again, should not, do move character anymore
        //    //because when you try to carry a character that can move, it will knock it out first so that it cannot move, the character will end up attacking the other character which we do not want because we use this on paralyzed characters only
        //    //We do not unnecessary fighting because it will lead to criminality which we do not intended to do in this case
        //    if (target is Character targetCharacter) {
        //        if (targetCharacter.canMove) {
        //            costLog += $" +2000(Move Character, target can move again)";
        //            actor.logComponent.AppendCostLog(costLog);
        //            return 2000;
        //        }
        //    }
        //}
        costLog += $" +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 10;
    }
#endregion

#region Requirements
   protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            if(actor.gridTileLocation != null && poiTarget.gridTileLocation != null) {
                if (poiTarget is Character character) {
                    return actor != poiTarget && poiTarget.mapObjectVisual &&
                           poiTarget.numOfActionsBeingPerformedOnThis <= 0 && character.carryComponent.IsNotBeingCarried();
                } else {
                    return actor != poiTarget && poiTarget.mapObjectVisual &&
                           poiTarget.numOfActionsBeingPerformedOnThis <= 0;
                }
            }
        }
        return false;
    }
#endregion

#region State Effects
    public void AfterCarrySuccess(ActualGoapNode goapNode) {
        goapNode.actor.CarryPOI(goapNode.poiTarget, setOwnership: false);
    }
#endregion

#region Precondition
    private bool TargetIsRestrainedOrDead(Character actor, IPointOfInterest target, object[] otherData, JOB_TYPE jobType) {
        if(target is Character) {
            return target.traitContainer.HasTrait("Restrained") || target.isDead;
        }
        return true;
    }
#endregion

    private bool TargetMissingForCarry(ActualGoapNode node) {
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        if (poiTarget.gridTileLocation == null || actor.currentRegion != poiTarget.currentRegion || !poiTarget.mapObjectVisual) {
            return true;
        } else if (actor.gridTileLocation != poiTarget.gridTileLocation && !actor.gridTileLocation.IsNeighbour(poiTarget.gridTileLocation, true)) {
            if (actor.hasMarker && actor.marker.IsCharacterInLineOfSightWith(poiTarget)) {
                return false;
            }
            return true;
        }
        return false;
    }
}