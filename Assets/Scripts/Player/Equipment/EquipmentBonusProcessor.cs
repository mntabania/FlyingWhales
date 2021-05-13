using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public static class EquipmentBonusProcessor
{
    public static void ApplyEquipBonusToTarget(EquipmentItem p_equipItem, Character p_targetCharacter) {
        p_equipItem.equipmentData.equipmentUpgradeData.bonuses.ForEach((eachBonus) => {
            ApplyEachBonusToTarget(p_equipItem, eachBonus, p_targetCharacter);
        });  
    }

    public static void RemoveEquipBonusToTarget(EquipmentItem p_equipItem, Character p_targetCharacter) {
        p_equipItem.equipmentData.equipmentUpgradeData.bonuses.ForEach((eachBonus) => {
            RemoveEachBonusToTarget(p_equipItem, eachBonus, p_targetCharacter);
        });
    }

    static void ApplyEachBonusToTarget(EquipmentItem p_equipItem, EQUIPMENT_BONUS p_equipBonus, Character p_targetCharacter) {
        switch (p_equipBonus) {
            case EQUIPMENT_BONUS.Atk_Actual:
            p_targetCharacter.combatComponent.AdjustAttackModifierFromEquips((int)p_equipItem.equipmentData.equipmentUpgradeData.AdditionalAttackActual);
            break;
            case EQUIPMENT_BONUS.Atk_Percentage:
            float computedAttack = p_targetCharacter.combatComponent.unModifiedAttack * (p_equipItem.equipmentData.equipmentUpgradeData.AdditionalAttackPercentage / 100f);
            p_targetCharacter.combatComponent.AdjustAttackModifierFromEquips((int)computedAttack);
            break;
            case EQUIPMENT_BONUS.Max_HP_Actual:
            p_targetCharacter.combatComponent.AdjustMaxHPModifierFromEquips((int)p_equipItem.equipmentData.equipmentUpgradeData.AdditionalmaxHPActual);
            break;
            case EQUIPMENT_BONUS.Max_HP_Percentage:
            float addedHP = p_targetCharacter.combatComponent.unModifiedMaxHP * (p_equipItem.equipmentData.equipmentUpgradeData.AdditionalmaxHPPercentage / 100f);
            p_targetCharacter.combatComponent.AdjustMaxHPModifierFromEquips((int)addedHP);
            break;
            case EQUIPMENT_BONUS.Increased_Piercing:
            p_targetCharacter.piercingAndResistancesComponent.AdjustPiercing(p_equipItem.equipmentData.equipmentUpgradeData.AdditionalPiercing);
            break;
            case EQUIPMENT_BONUS.Attack_Element:
            p_targetCharacter.combatComponent.SetElementalType(p_equipItem.equipmentData.equipmentUpgradeData.elementAttackBonus);
            break;
            case EQUIPMENT_BONUS.Increased_3_Random_Resistance:
            case EQUIPMENT_BONUS.Increased_4_Random_Resistance:
            case EQUIPMENT_BONUS.Increased_5_Random_Resistance:
            ApplyResistanceBonusOnCharacter(p_equipItem, p_targetCharacter);
            break;
        }
    }

    static void RemoveEachBonusToTarget(EquipmentItem p_equipItem, EQUIPMENT_BONUS p_equipBonus, Character p_targetCharacter) {
        switch (p_equipBonus) {
            case EQUIPMENT_BONUS.Atk_Actual:
            p_targetCharacter.combatComponent.AdjustAttackModifierFromEquips(-1 * (int)p_equipItem.equipmentData.equipmentUpgradeData.AdditionalAttackActual);
            break;
            case EQUIPMENT_BONUS.Atk_Percentage:
            float computedAttack = p_targetCharacter.combatComponent.unModifiedAttack * (p_equipItem.equipmentData.equipmentUpgradeData.AdditionalAttackPercentage / 100f);
            p_targetCharacter.combatComponent.AdjustAttackModifierFromEquips((int)-computedAttack);
            break;
            case EQUIPMENT_BONUS.Max_HP_Actual:
            p_targetCharacter.combatComponent.AdjustMaxHPModifierFromEquips(-1 * (int)p_equipItem.equipmentData.equipmentUpgradeData.AdditionalmaxHPActual);
            break;
            case EQUIPMENT_BONUS.Max_HP_Percentage:
            float addedHP = p_targetCharacter.combatComponent.unModifiedMaxHP * (p_equipItem.equipmentData.equipmentUpgradeData.AdditionalmaxHPPercentage / 100f);
            p_targetCharacter.combatComponent.AdjustMaxHPModifierFromEquips((int)-addedHP);
            break;
            case EQUIPMENT_BONUS.Increased_Piercing:
            p_targetCharacter.piercingAndResistancesComponent.AdjustPiercing(-1 * p_equipItem.equipmentData.equipmentUpgradeData.AdditionalPiercing);
            break;
            case EQUIPMENT_BONUS.Attack_Element:
            EquipmentComponent ec = p_targetCharacter.equipmentComponent;
            EquipmentItem ei = ec.GetRandomRemainingEquipment(p_equipItem);
            ProcessElementAfterRemovingSomeItem(ec, ei, p_targetCharacter);
            break;
            case EQUIPMENT_BONUS.Increased_3_Random_Resistance:
            case EQUIPMENT_BONUS.Increased_4_Random_Resistance:
            case EQUIPMENT_BONUS.Increased_5_Random_Resistance:
            RemoveResistanceBonusOnCharacter(p_equipItem, p_targetCharacter);
            break;
        }
    }

    static void ProcessElementAfterRemovingSomeItem(EquipmentComponent ec, EquipmentItem ei, Character p_targetCharacter) {
        if (ei != null) {
            p_targetCharacter.combatComponent.SetElementalType(ei.equipmentData.equipmentUpgradeData.elementAttackBonus);
        } else if (p_targetCharacter.combatComponent.elementalStatusWaitingList.Count > 0) {
            int index = UnityEngine.Random.Range(0, p_targetCharacter.combatComponent.elementalStatusWaitingList.Count);
            p_targetCharacter.combatComponent.SetElementalType(p_targetCharacter.combatComponent.elementalStatusWaitingList[index]);
            p_targetCharacter.combatComponent.elementalStatusWaitingList.RemoveAt(index);
        } else {
            p_targetCharacter.combatComponent.SetElementalType(p_targetCharacter.combatComponent.initialElementalType);
        }
    }

    static void ApplyResistanceBonusOnCharacter(EquipmentItem p_equipItem, Character p_targetCharacter) {
        for (int x = 0; x < p_equipItem.resistanceBonuses.Count; ++x) {
            p_targetCharacter.piercingAndResistancesComponent.AdjustResistance(p_equipItem.resistanceBonuses[x], p_equipItem.equipmentData.equipmentUpgradeData.additionalResistanceBonus);
        }
    }

    static void RemoveResistanceBonusOnCharacter(EquipmentItem p_equipItem, Character p_targetCharacter) {
        for (int x = 0; x < p_equipItem.resistanceBonuses.Count; ++x) {
            p_targetCharacter.piercingAndResistancesComponent.AdjustResistance(p_equipItem.resistanceBonuses[x], -p_equipItem.equipmentData.equipmentUpgradeData.additionalResistanceBonus);
        }
    }

    //this one is applied to waepon not to villager
    public static void SetBonusResistanceOnWeapon(EquipmentItem p_equipItem) {
        int resistanceCount = 0;
        if (p_equipItem.equipmentData.equipmentUpgradeData.bonuses.Contains(EQUIPMENT_BONUS.Increased_3_Random_Resistance)) {
            resistanceCount = 3;
        } else if (p_equipItem.equipmentData.equipmentUpgradeData.bonuses.Contains(EQUIPMENT_BONUS.Increased_4_Random_Resistance)) {
            resistanceCount = 4;
        } else if (p_equipItem.equipmentData.equipmentUpgradeData.bonuses.Contains(EQUIPMENT_BONUS.Increased_5_Random_Resistance)) {
            resistanceCount = 5;
        }
        var sequence = Enumerable.Range(1, (int)RESISTANCE.Physical + 1).OrderBy(n => n * n + UnityEngine.Random.Range(1, (int)RESISTANCE.Physical + 1) * (new System.Random()).Next());

        var result = sequence.Distinct().Take(resistanceCount);

        foreach (var item in result) {
            RESISTANCE addElem = (RESISTANCE)item;
            p_equipItem.resistanceBonuses.Add(addElem);
        }
    }
}
