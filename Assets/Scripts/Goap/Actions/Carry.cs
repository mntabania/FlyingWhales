﻿
using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class Carry : GoapAction {

    public Carry() : base(INTERACTION_TYPE.CARRY) {
        actionIconString = GoapActionStateDB.Work_Icon;
        isNotificationAnIntel = false;
        canBeAdvertisedEvenIfActorIsUnavailable = true;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPrecondition(new GoapEffect(GOAP_EFFECT_CONDITION.CANNOT_MOVE, string.Empty, false, GOAP_EFFECT_TARGET.TARGET), TargetCannotMove);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.IN_PARTY, conditionKey = string.Empty, isKeyANumber = false, target = GOAP_EFFECT_TARGET.TARGET });
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
        GoapActionInvalidity goapActionInvalidity = new GoapActionInvalidity(defaultTargetMissing, stateName);
        if (defaultTargetMissing == false) {
            //check the target's traits, if any of them can make this action invalid
            for (int i = 0; i < poiTarget.traitContainer.allTraits.Count; i++) {
                Trait trait = poiTarget.traitContainer.allTraits[i];
                if (trait.TryStopAction(goapType, actor, poiTarget, ref goapActionInvalidity)) {
                    break; //a trait made this action invalid, stop loop
                }
            }
        }
        if (goapActionInvalidity.isInvalid == false) {
            if(poiTarget is Character) {
                if ((poiTarget as Character).IsInOwnParty() == false) {
                    goapActionInvalidity.isInvalid = true;
                }
            }
        }
        return goapActionInvalidity;
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        return 1;
    }
    #endregion

    #region Requirements
   protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            return actor != poiTarget;
        }
        return false;
    }
    #endregion

    #region State Effects
    public void AfterCarrySuccess(ActualGoapNode goapNode) {
        //Character target = goapNode.poiTarget as Character;
        goapNode.actor.ownParty.AddPOI(goapNode.poiTarget);
    }
    #endregion

    #region Precondition
    private bool TargetCannotMove(Character actor, IPointOfInterest target, object[] otherData) {
        if(target is Character) {
            return (target as Character).canMove == false;
        }
        return true;
    }
    #endregion

    private bool TargetMissingForCarry(ActualGoapNode node) {
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        return poiTarget.gridTileLocation == null || actor.specificLocation != poiTarget.specificLocation
                    || !(actor.gridTileLocation == poiTarget.gridTileLocation || actor.gridTileLocation.IsNeighbour(poiTarget.gridTileLocation));
    }
}

public class CarryData : GoapActionData {
    public CarryData() : base(INTERACTION_TYPE.CARRY) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor != poiTarget && poiTarget is Character && (poiTarget as Character).traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE);
    }
}
