
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class CraftWeapon : GoapAction {

    public CraftWeapon() : base(INTERACTION_TYPE.CRAFT_WEAPON) {
        actionIconString = GoapActionStateDB.Chop_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.ELVES, RACE.HUMANS, RACE.RATMAN, };
        logTags = new[] { LOG_TAG.Work };
        canBeAdvertisedEvenIfTargetIsUnavailable = true;
    }

    #region Overrides
    //protected override void ConstructBasePreconditionsAndEffects() {
    //    AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.DEATH, conditionKey = string.Empty, isKeyANumber = false, target = GOAP_EFFECT_TARGET.TARGET }, IsTargetDead);
    //    AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.ABSORB_LIFE, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR));
    //}
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Craft Weapon Success", goapNode);
    }

    protected override void ConstructBasePreconditionsAndEffects() {
        //SetPrecondition(new GoapEffect(GOAP_EFFECT_CONDITION.BUY_OBJECT, "Wood Pile", false, GOAP_EFFECT_TARGET.ACTOR), CheckIfCanBeCrafted);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 10;
    }
    #endregion


    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);

        return satisfied;
    }

    bool CheckIfCanBeCrafted(ActualGoapNode p_node) {
        EquipmentItem equipment = p_node.target as EquipmentItem;
        Workshop workShop = p_node.actor.structureComponent.workPlaceStructure as Workshop;
        if (workShop.CanBeCrafted(equipment.equipmentData.specificResource, equipment.equipmentData.resourceAmount)) {
            return true;
        }
        if (workShop.CanBeCrafted(equipment.equipmentData.resourceType, equipment.equipmentData.resourceAmount)) {
            return true;
        }
        return false;
    }
    #endregion

    #region State Effects
    public void AfterCraftWeaponSuccess(ActualGoapNode p_node) {
        (p_node.target as TileObject).SetMapObjectState(MAP_OBJECT_STATE.BUILT);
        EquipmentItem targetItem = p_node.target as EquipmentItem;
        if (p_node.actor.talentComponent.GetTalent(CHARACTER_TALENT.Crafting).level >= 5) {
            if (GameUtilities.RollChance(20)) {
                Debug.LogError(targetItem.name + " Crafted as Premium Quality Item");
                targetItem.MakeQualityPremium();
            } else if (GameUtilities.RollChance(20)) {
                targetItem.MakeQualityHigh();
            }

        } else if (p_node.actor.talentComponent.GetTalent(CHARACTER_TALENT.Crafting).level >= 3) {
            if (GameUtilities.RollChance(20)) {
                targetItem.MakeQualityHigh();
            }
        } else {
            //Debug.LogError(targetItem.name + " Crafted as normal Item");
        }
    }
    #endregion

}