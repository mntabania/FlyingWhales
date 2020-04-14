using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class HealSelf : GoapAction {

    public HealSelf() : base(INTERACTION_TYPE.HEAL_SELF) {
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        actionIconString = GoapActionStateDB.Cure_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPrecondition(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, "Healing Potion", false, GOAP_EFFECT_TARGET.ACTOR), HasItemInInventory);
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Heal Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    #endregion

    #region State Effects
    public void PerTickHealSuccess(ActualGoapNode goapNode) {
        goapNode.actor.AdjustHP(Mathf.FloorToInt(goapNode.actor.maxHP * 0.25f), ELEMENTAL_TYPE.Normal);
    }
    public void AfterHealSuccess(ActualGoapNode goapNode) {
        //Remove Healing Potion from Actor's Inventory
        goapNode.actor.UnobtainItem(TILE_OBJECT_TYPE.HEALING_POTION);
    }
    #endregion

    #region Preconditions
    private bool HasItemInInventory(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor.HasItem(TILE_OBJECT_TYPE.HEALING_POTION);
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            return poiTarget == actor;
        }
        return false;
    }
    #endregion
    
}
