using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;
using Inner_Maps.Location_Structures;
public class ReleaseCharacter : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.DIRECT; } }

    public ReleaseCharacter() : base(INTERACTION_TYPE.RELEASE_CHARACTER) {
        actionIconString = GoapActionStateDB.Work_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] {
            RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON,
            RACE.GOLEM, RACE.KOBOLD, RACE.LESSER_DEMON, RACE.MIMIC, RACE.PIG, RACE.SHEEP, RACE.ENT, RACE.WISP,
            RACE.GHOST, RACE.NYMPH, RACE.SLIME, RACE.SLUDGE, RACE.CHICKEN, RACE.ELEMENTAL, RACE.ABOMINATION, RACE.ANGEL, RACE.DEMON, RACE.RATMAN
        };
        logTags = new[] {LOG_TAG.Work};
    }

    #region Overrides
    //protected override void ConstructBasePreconditionsAndEffects() {
    //    AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Restrained", target = GOAP_EFFECT_TARGET.TARGET });
    //    AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Unconscious", target = GOAP_EFFECT_TARGET.TARGET });
    //}
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Release Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        return 1;
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
        IPointOfInterest poiTarget = node.poiTarget;
        if (goapActionInvalidity.isInvalid == false) {
            if ((poiTarget as Character).carryComponent.IsNotBeingCarried() == false) {
                goapActionInvalidity.isInvalid = true;
                goapActionInvalidity.reason = "target_carried";
            }
        }
        return goapActionInvalidity;
    }
    public override void OnStoppedInterrupt(ActualGoapNode node) {
        base.OnStoppedInterrupt(node);
        if (node.actor.partyComponent.hasParty && node.actor.partyComponent.currentParty.isActive && node.actor.partyComponent.currentParty.currentQuest is IRescuePartyQuest quest) {
            if (quest.targetCharacter == node.poiTarget) {
                quest.SetIsReleasing(false);
            }
        }
    }
    public override void OnStopWhilePerforming(ActualGoapNode node) {
        base.OnStopWhilePerforming(node);
        if (node.actor.partyComponent.hasParty && node.actor.partyComponent.currentParty.isActive && node.actor.partyComponent.currentParty.currentQuest is IRescuePartyQuest quest) {
            if (quest.targetCharacter == node.poiTarget) {
                quest.SetIsReleasing(false);
            }
        }
    }
    public override void OnStopWhileStarted(ActualGoapNode node) {
        base.OnStopWhileStarted(node);
        if (node.actor.partyComponent.hasParty && node.actor.partyComponent.currentParty.isActive && node.actor.partyComponent.currentParty.currentQuest is IRescuePartyQuest quest) {
            if (quest.targetCharacter == node.poiTarget) {
                quest.SetIsReleasing(false);
            }
        }
    }
    public override void OnInvalidAction(ActualGoapNode node) {
        base.OnInvalidAction(node);
        if (node.actor.partyComponent.hasParty && node.actor.partyComponent.currentParty.isActive && node.actor.partyComponent.currentParty.currentQuest is IRescuePartyQuest quest) {
            if (quest.targetCharacter == node.poiTarget) {
                quest.SetIsReleasing(false);
            }
        }
    }
    #endregion

    //#region Preconditions
    //private bool HasItemTool(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType) {
    //    return actor.HasItem(TILE_OBJECT_TYPE.TOOL);
    //}
    //#endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            Character target = poiTarget as Character;
            return target.traitContainer.HasTrait("Restrained", "Unconscious", "Frozen", "Ensnared", "Enslaved");
        }
        return false;
    }
    #endregion

    #region State Effects
    public void AfterReleaseSuccess(ActualGoapNode goapNode) {
        Character target = goapNode.poiTarget as Character;
        bool isEnslaved = target.traitContainer.HasTrait("Enslaved");
        target.traitContainer.RemoveRestrainAndImprison(target, goapNode.actor);
        target.traitContainer.RemoveStatusAndStacks(target, "Unconscious", goapNode.actor);
        target.traitContainer.RemoveStatusAndStacks(target, "Frozen", goapNode.actor);
        target.traitContainer.RemoveStatusAndStacks(target, "Ensnared", goapNode.actor);
        target.traitContainer.RemoveTrait(target, "Enslaved", goapNode.actor);

        if (goapNode.actor.partyComponent.hasParty && goapNode.actor.partyComponent.currentParty.isActive && goapNode.actor.partyComponent.currentParty.currentQuest is IRescuePartyQuest quest) {
            if(quest.targetCharacter == goapNode.poiTarget) {
                quest.SetIsSuccessful(true);
                quest.SetIsReleasing(false);
                goapNode.actor.partyComponent.currentParty.currentQuest.EndQuest("Finished quest");

                //if target is paralyzed carry back home
                if (target.traitContainer.HasTrait("Paralyzed")) {
                    //if target is paralyzed carry back home
                    if (!target.IsPOICurrentlyTargetedByAPerformingAction(JOB_TYPE.MOVE_CHARACTER)) {
                        goapNode.actor.jobComponent.TryTriggerMoveCharacter(target);
                    }
                }
            }
        }
        if (isEnslaved) {
            //If target is enslaved and is released, must try to join the faction of the releaser
            if (goapNode.actor.faction != null && goapNode.actor.faction.isMajorNonPlayer) {
                target.interruptComponent.TriggerInterrupt(INTERRUPT.Join_Faction, goapNode.actor, "join_faction_normal");
            }
        }
        target.combatComponent.RemoveHostileInRange(goapNode.actor);
        target.combatComponent.RemoveAvoidInRange(goapNode.actor);
    }
    #endregion
}