
using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class CarryCorpse : GoapAction {

    public CarryCorpse() : base(INTERACTION_TYPE.CARRY_CORPSE) {
        actionIconString = GoapActionStateDB.Work_Icon;
        canBeAdvertisedEvenIfTargetIsUnavailable = true;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER, POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        //racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF,
        //    RACE.SPIDER, RACE.DRAGON, RACE.GOLEM, RACE.DEMON, RACE.ELEMENTAL, RACE.KOBOLD, RACE.MIMIC, RACE.ABOMINATION,
        //    RACE.CHICKEN, RACE.SHEEP, RACE.PIG, RACE.NYMPH, RACE.WISP, RACE.SLUDGE, RACE.GHOST, RACE.LESSER_DEMON, RACE.ANGEL };
        logTags = new[] {LOG_TAG.Work};
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        SetPrecondition(new GoapEffect(GOAP_EFFECT_CONDITION.DEATH, string.Empty, false, GOAP_EFFECT_TARGET.TARGET), TargetIsDead);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_POI, conditionKey = "Carry Corpse", isKeyANumber = false, target = GOAP_EFFECT_TARGET.TARGET });
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
        //if (defaultTargetMissing == false) {
        //    //check the target's traits, if any of them can make this action invalid
        //    for (int i = 0; i < poiTarget.traitContainer.allTraits.Count; i++) {
        //        Trait trait = poiTarget.traitContainer.allTraits[i];
        //        if (trait.TryStopAction(goapType, actor, poiTarget, ref goapActionInvalidity)) {
        //            break; //a trait made this action invalid, stop loop
        //        }
        //    }
        //}
        if (goapActionInvalidity.isInvalid == false) {
            if (poiTarget.isBeingCarriedBy != null && poiTarget.isBeingCarriedBy != actor) {
                //If the target is already being carried by another character, fail this carry
                goapActionInvalidity.isInvalid = true;
                goapActionInvalidity.reason = "target_carried";
            } else if (poiTarget.numOfActionsBeingPerformedOnThis > 0) {
                goapActionInvalidity.isInvalid = true;
                goapActionInvalidity.reason = "target_unavailable";
            } else if (poiTarget is Tombstone tombstone && tombstone.character != null && tombstone.character.numOfActionsBeingPerformedOnThis > 0) {
                goapActionInvalidity.isInvalid = true;
                goapActionInvalidity.reason = "target_unavailable";
            }
        }
        return goapActionInvalidity;
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}:";
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
                Character deadTarget = null;
                if (poiTarget is Character targetCharacter) {
                    deadTarget = targetCharacter;
                } else if (poiTarget is Tombstone tombstone) {
                    deadTarget = tombstone.character;
                }
                if(deadTarget != null && deadTarget.isDead) {
                    return actor != poiTarget && poiTarget.mapObjectVisual && poiTarget.numOfActionsBeingPerformedOnThis <= 0 && poiTarget.isBeingCarriedBy == null;
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
    private bool TargetIsDead(Character actor, IPointOfInterest target, object[] otherData, JOB_TYPE jobType) {
        if(target is Character targetCharacter) {
            return targetCharacter.isDead;
        } else if (target is Tombstone tombstone) {
            return tombstone.character.isDead;
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