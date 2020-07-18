using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class ReleaseCharacter : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.DIRECT; } }

    public ReleaseCharacter() : base(INTERACTION_TYPE.RELEASE_CHARACTER) {
        actionIconString = GoapActionStateDB.Work_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] {
            RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON,
            RACE.GOLEM, RACE.KOBOLD, RACE.LESSER_DEMON, RACE.MIMIC, RACE.PIG, RACE.SHEEP, RACE.ENT, RACE.WISP,
            RACE.GHOST, RACE.NYMPH, RACE.SLIME, RACE.SLUDGE, RACE.CHICKEN, RACE.ELEMENTAL, RACE.ABOMINATION, RACE.ANGEL, RACE.DEMON
        };
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
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        return 1;
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
        IPointOfInterest poiTarget = node.poiTarget;
        if (goapActionInvalidity.isInvalid == false) {
            if ((poiTarget as Character).carryComponent.IsNotBeingCarried() == false) {
                goapActionInvalidity.isInvalid = true;
            }
        }
        return goapActionInvalidity;
    }
    #endregion

    //#region Preconditions
    //private bool HasItemTool(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType) {
    //    return actor.HasItem(TILE_OBJECT_TYPE.TOOL);
    //}
    //#endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            Character target = poiTarget as Character;
            return target.traitContainer.HasTrait("Restrained", "Unconscious", "Frozen", "Ensnared");
        }
        return false;
    }
    #endregion

    #region State Effects
    public void AfterReleaseSuccess(ActualGoapNode goapNode) {
        Character target = goapNode.poiTarget as Character;
        target.traitContainer.RemoveStatusAndStacks(target, "Restrained");
        target.traitContainer.RemoveStatusAndStacks(target, "Unconscious");
        target.traitContainer.RemoveStatusAndStacks(target, "Frozen");
        target.traitContainer.RemoveStatusAndStacks(target, "Ensnared");

        if (goapNode.actor.partyComponent.hasParty && goapNode.actor.partyComponent.currentParty is RescueParty rescueParty) {
            if(rescueParty.targetCharacter == goapNode.poiTarget) {
                rescueParty.DisbandParty();
            }
        }
    }
    #endregion
}

public class ReleaseCharacterData : GoapActionData {
    public ReleaseCharacterData() : base(INTERACTION_TYPE.RELEASE_CHARACTER) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (poiTarget is Character) {
            Character target = poiTarget as Character;
            return target.traitContainer.HasTrait("Restrained");
        }
        return false;
    }
}
