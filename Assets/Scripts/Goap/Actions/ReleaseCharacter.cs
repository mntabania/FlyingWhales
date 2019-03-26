﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReleaseCharacter : GoapAction {

    public ReleaseCharacter(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.RELEASE_ABDUCTED_ACTION, INTERACTION_ALIGNMENT.GOOD, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Social_Icon;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = HasAbductedOrRestrainedTrait;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_ITEM, conditionKey = SPECIAL_TOKEN.TOOL, targetPOI = actor }, HasItemTool);
        //AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Abducted", targetPOI = poiTarget });
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Restrained", targetPOI = poiTarget });
    }
    public override void PerformActualAction() {
        if (actor.gridTileLocation.IsNeighbour(poiTarget.gridTileLocation)) {
            SetState("Release Success");
        } else {
            SetState("Target Missing");
        }
        base.PerformActualAction();
    }
    protected override int GetCost() {
        return 1;
    }
    public override void FailAction() {
        base.FailAction();
        SetState("Target Missing");
    }
    #endregion

    #region Preconditions
    private bool HasItemTool() {
        return actor.isHoldingItem && actor.tokenInInventory.name == "Tool";
    }
    #endregion

    #region Requirements
    protected bool HasAbductedOrRestrainedTrait() {
        if (poiTarget is Character) {
            Character target = poiTarget as Character;
            //return target.GetTraitOr("Abducted", "Restrained") != null;
            return target.GetTrait("Restrained") != null;
        }
        return false;
    }
    #endregion

    #region State Effects
    public void PreReleaseSuccess() {
        currentState.AddLogFiller(poiTarget as Character, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    public void AfterReleaseSuccess() {
        Character target = poiTarget as Character;
        target.RemoveTrait("Abducted");
        target.RemoveTrait("Restrained");
    }
    public void PreTargetMissing() {
        currentState.AddLogFiller(poiTarget as Character, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    public void AfterTargetMissing() {
        actor.RemoveAwareness(poiTarget);
    }
    #endregion
}
