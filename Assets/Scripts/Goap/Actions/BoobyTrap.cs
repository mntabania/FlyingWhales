using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Traits;

public class BoobyTrap : GoapAction {

    public BoobyTrap() : base(INTERACTION_TYPE.BOOBY_TRAP) {
        actionIconString = GoapActionStateDB.Hostile_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Booby Trapped", false, GOAP_EFFECT_TARGET.TARGET));
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Trap Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}:";
        int cost = 10;
        costLog += $" +{cost}(Initial)";
        actor.logComponent.AppendCostLog(costLog);
        return cost;
    }
    #endregion

    #region State Effects
    public void AfterTrapSuccess(ActualGoapNode goapNode) {
        Character actor = goapNode.actor;
        IPointOfInterest target = goapNode.poiTarget;

        Trait trait;
        target.traitContainer.AddTrait(target, "Booby Trapped", out trait, actor);
        if(trait != null) {
            (trait as BoobyTrapped).SetElementType(actor.combatComponent.elementalDamage.type);
        }
    }
    #endregion

    #region Requirement
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            return poiTarget.gridTileLocation != null;
        }
        return false;
    }
    #endregion
}