
using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class Carry : GoapAction {

    private Precondition _carryPrecondition;

    public Carry() : base(INTERACTION_TYPE.CARRY) {
        actionIconString = GoapActionStateDB.Work_Icon;
        canBeAdvertisedEvenIfTargetIsUnavailable = true;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        //racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF,
        //    RACE.SPIDER, RACE.DRAGON, RACE.GOLEM, RACE.DEMON, RACE.ELEMENTAL, RACE.KOBOLD, RACE.MIMIC, RACE.ABOMINATION,
        //    RACE.CHICKEN, RACE.SHEEP, RACE.PIG, RACE.NYMPH, RACE.WISP, RACE.SLUDGE, RACE.GHOST, RACE.LESSER_DEMON, RACE.ANGEL };
        logTags = new[] {LOG_TAG.Work};
        _carryPrecondition = new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.CANNOT_MOVE, string.Empty, false, GOAP_EFFECT_TARGET.TARGET), TargetCannotMove);
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_POI, conditionKey = string.Empty, isKeyANumber = false, target = GOAP_EFFECT_TARGET.TARGET });
    }
    public override Precondition GetPrecondition(Character actor, IPointOfInterest target, OtherData[] otherData, JOB_TYPE jobType, out bool isOverridden) {
        if (jobType != JOB_TYPE.MOVE_CHARACTER) {
            //Only use precondition of carry which is ensuring the target cannot move before carrying if the job is not move character
            //If the job is move character, this should not have any preconditions so that the plan will end here, since we do not want the actor to attack the target just for him to carry the target
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
            if(poiTarget is Character) {
                if ((poiTarget as Character).carryComponent.IsNotBeingCarried() == false) {
                    goapActionInvalidity.isInvalid = true;
                    goapActionInvalidity.reason = "target_carried";
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
#endif
        if (job.jobType == JOB_TYPE.MOVE_CHARACTER) {
            //If the job is move character and the target can move again, should not, do move character anymore
            //because when you try to carry a character that can move, it will knock it out first so that it cannot move, the character will end up attacking the other character which we do not want because we use this on paralyzed characters only
            //We do not unnecessary fighting because it will lead to criminality which we do not intended to do in this case
            if (target is Character targetCharacter) {
                if (targetCharacter.limiterComponent.canMove) {
#if DEBUG_LOG
                    costLog += $" +2000(Move Character, target can move again)";
                    actor.logComponent.AppendCostLog(costLog);
#endif
                    return 2000;
                }
            }
        }
#if DEBUG_LOG
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
            // if(poiTarget is TileObject) {
            //     TileObject tileObj = poiTarget as TileObject;
            //     return tileObj.isBeingCarriedBy == null && tileObj.gridTileLocation != null;
            // }
            if (job.jobType == JOB_TYPE.MOVE_CHARACTER) {
                //If job is move character and target already is moving, do not do carry anymore
                if (poiTarget is Character character) {
                    if (character.limiterComponent.canMove) {
                        return false;
                    }
                }
            }
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
        //Character target = goapNode.poiTarget as Character;
        // goapNode.actor.ownParty.AddPOI(goapNode.poiTarget);
        bool setOwnership = true;
        if (goapNode.associatedJobType == JOB_TYPE.HAUL
            || goapNode.associatedJobType == JOB_TYPE.FULLNESS_RECOVERY_NORMAL
            || goapNode.associatedJobType == JOB_TYPE.FULLNESS_RECOVERY_URGENT
            || goapNode.associatedJobType == JOB_TYPE.OBTAIN_PERSONAL_FOOD) {
            setOwnership = false;
        }

        if (goapNode.associatedJobType == JOB_TYPE.SNATCH) {
            //Special case for snatch, so that we can be sure that snatched characters are always restrained
            goapNode.poiTarget.traitContainer.RestrainAndImprison(goapNode.poiTarget, goapNode.actor, PlayerManager.Instance.player.playerFaction);
            //goapNode.poiTarget.traitContainer.AddTrait(goapNode.poiTarget, "Restrained", goapNode.actor);
            //if (goapNode.poiTarget.traitContainer.HasTrait("Prisoner")) {
            //    Prisoner prisoner = goapNode.poiTarget.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner");
            //    prisoner.SetPrisonerOfFaction(PlayerManager.Instance.player.playerFaction);
            //}
        }
        
        goapNode.actor.CarryPOI(goapNode.poiTarget, setOwnership: setOwnership);
    }
#endregion

#region Precondition
    private bool TargetCannotMove(Character actor, IPointOfInterest target, object[] otherData, JOB_TYPE jobType) {
        if(target is Character) {
            return (target as Character).limiterComponent.canMove == false;
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