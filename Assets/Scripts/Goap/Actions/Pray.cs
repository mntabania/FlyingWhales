﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pray : GoapAction {

    private LocationStructure _targetStructure;
    public override LocationStructure targetStructure { get { return _targetStructure; } }

    protected override string failActionState { get { return "Pray Failed"; } }

    public Pray(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.PRAY, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        this.goapName = "Pray";
        actionLocationType = ACTION_LOCATION_TYPE.NEARBY;
        actionIconString = GoapActionStateDB.Entertain_Icon;
        shouldIntelNotificationOnlyIfActorIsActive = true;
        isNotificationAnIntel = false;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, targetPOI = actor });
    }
    public override void PerformActualAction() {
        //if (targetTile.occupant != null && targetTile.occupant != actor) {
        //    SetState("Pray Failed");
        //} else {
        //}
        base.PerformActualAction();
        SetState("Pray Success");
    }
    public override LocationGridTile GetTargetLocationTile() {
        return InteractionManager.Instance.GetTargetLocationTile(actionLocationType, actor, null, targetStructure);
    }
    protected override int GetCost() {
        return Utilities.rng.Next(20, 65);
    }
    //public override void FailAction() {
    //    base.FailAction();
    //    SetState("Pray Failed");
    //}
    public override void SetTargetStructure() {
        _targetStructure = actor.currentStructure;
        base.SetTargetStructure();
    }
    public override void OnStopActionDuringCurrentState() {
        if (currentState.name == "Pray Success") {
            actor.AdjustDoNotGetLonely(-1);
        }
    }
    #endregion

    #region State Effects
    public void PrePraySuccess() {
        actor.AdjustDoNotGetLonely(1);
    }
    public void PerTickPraySuccess() {
        actor.AdjustHappiness(8);
    }
    public void AfterPraySuccess() {
        actor.AdjustDoNotGetLonely(-1);
    }
    #endregion

    #region Requirement
    protected bool Requirement() {
        if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            return false;
        }
        return actor == poiTarget;
    }
    #endregion
}
