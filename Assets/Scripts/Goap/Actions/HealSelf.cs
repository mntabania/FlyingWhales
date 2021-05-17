using System;
using System.Collections;
using System.Collections.Generic;
using Goap.Unique_Action_Data;
using UnityEngine;  
using Traits;

public class HealSelf : GoapAction {
    public override Type uniqueActionDataType => typeof(HealSelfUAD);
    public HealSelf() : base(INTERACTION_TYPE.HEAL_SELF) {
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        actionIconString = GoapActionStateDB.Cure_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Needs};
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        SetPrecondition(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, "Healing Potion", false, GOAP_EFFECT_TARGET.ACTOR), HasItemInInventory);
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Heal Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 10;
    }
#endregion

#region State Effects
    public void PreHealSuccess(ActualGoapNode goapNode) {
        TileObject chosenHealingPotion = goapNode.actor.GetItem(TILE_OBJECT_TYPE.HEALING_POTION);
        if (chosenHealingPotion != null && chosenHealingPotion.traitContainer.HasTrait("Poisoned")) {
            HealSelfUAD data = goapNode.GetConvertedUniqueActionData<HealSelfUAD>();
            data.SetUsedPoisonedHealingPotion(true);
            Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "GoapAction", "Heal Self", "used_poison", goapNode, logTags);
            log.AddToFillers(goapNode.actor, goapNode.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(goapNode.poiTarget, goapNode.poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            goapNode.OverrideDescriptionLog(log);
        }
    }
    public void PerTickHealSuccess(ActualGoapNode goapNode) {
        HealSelfUAD data = goapNode.GetConvertedUniqueActionData<HealSelfUAD>();
        if (data.usedPoisonedHealingPotion) {
            goapNode.actor.AdjustHP(-100, ELEMENTAL_TYPE.Normal, triggerDeath: true);  
        } else {
            goapNode.actor.AdjustHP(Mathf.FloorToInt(goapNode.actor.maxHP * 0.25f), ELEMENTAL_TYPE.Normal);    
        }
        
    }
    public void AfterHealSuccess(ActualGoapNode goapNode) {
        HealSelfUAD data = goapNode.GetConvertedUniqueActionData<HealSelfUAD>();
        if (data.usedPoisonedHealingPotion) {
            goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Poisoned", bypassElementalChance: true);
            //specifically remove poisoned healing potion from inventory, if none exist just remove a random one.
            bool foundPoisonedPotion = false;
            for (int i = 0; i < goapNode.actor.items.Count; i++) {
                TileObject item = goapNode.actor.items[i];
                if (item.tileObjectType == TILE_OBJECT_TYPE.HEALING_POTION && item.traitContainer.HasTrait("Poisoned")) {
                    goapNode.actor.UnobtainItem(item);
                    foundPoisonedPotion = true;
                    break;
                }
            }
            if (!foundPoisonedPotion) {
                goapNode.actor.UnobtainItem(TILE_OBJECT_TYPE.HEALING_POTION);
            }
        } else {
            //Remove Healing Potion from Actor's Inventory
            goapNode.actor.UnobtainItem(TILE_OBJECT_TYPE.HEALING_POTION);    
        }
        
    }
#endregion

#region Preconditions
    private bool HasItemInInventory(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType) {
        return actor.HasItem(TILE_OBJECT_TYPE.HEALING_POTION);
    }
#endregion

#region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            return poiTarget == actor;
        }
        return false;
    }
#endregion
    
}
