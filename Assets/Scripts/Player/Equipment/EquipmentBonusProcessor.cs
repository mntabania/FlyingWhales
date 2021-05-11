using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EquipmentBonusProcessor
{
    public static void ApplyEquipBonusToTarget(EquipmentData p_equipData, Character p_targetCharacter) {
        p_equipData.equipmentUpgradeData.bonuses.ForEach((eachBonus) => {
            ApplyEachBonusToTarget(p_equipData, eachBonus, p_targetCharacter);
        });  
    }

    public static void RemoveEquipBonusToTarget(EquipmentData p_equipData, Character p_targetCharacter) {
        p_equipData.equipmentUpgradeData.bonuses.ForEach((eachBonus) => {
            RemoveEachBonusToTarget(p_equipData, eachBonus, p_targetCharacter);
        });
    }

    static void ApplyEachBonusToTarget(EquipmentData p_equipData, EQUIPMENT_BONUS p_equipBonus, Character p_targetCharacter) {
        switch (p_equipBonus) {
            case EQUIPMENT_BONUS.Atk_Actual:
            p_targetCharacter.combatComponent.AdjustAttackModifier((int)p_equipData.equipmentUpgradeData.AdditionalAttackActual);
            break;
            case EQUIPMENT_BONUS.Atk_Percentage:
            p_targetCharacter.combatComponent.AddAttackBaseOnPercentage(p_equipData.equipmentUpgradeData.AdditionalAttackPercentage / 100f);
            break;
            case EQUIPMENT_BONUS.Max_HP_Actual:
            p_targetCharacter.combatComponent.AdjustMaxHPModifier((int)p_equipData.equipmentUpgradeData.AdditionalmaxHPActual);
            break;
            case EQUIPMENT_BONUS.Max_HP_Percentage:
            float addedHP = p_targetCharacter.maxHP * (p_equipData.equipmentUpgradeData.AdditionalmaxHPPercentage / 100f);
            p_targetCharacter.combatComponent.AdjustMaxHPModifier((int)addedHP);
            break;
            case EQUIPMENT_BONUS.Increased_Piercing:
            p_targetCharacter.piercingAndResistancesComponent.AdjustPiercing(p_equipData.equipmentUpgradeData.AdditionalPiercing);
            break;
        }
    }

    static void RemoveEachBonusToTarget(EquipmentData p_equipData, EQUIPMENT_BONUS p_equipBonus, Character p_targetCharacter) {
        switch (p_equipBonus) {
            case EQUIPMENT_BONUS.Atk_Actual:
            p_targetCharacter.combatComponent.AdjustAttackModifier(-1 * (int)p_equipData.equipmentUpgradeData.AdditionalAttackActual);
            break;
            case EQUIPMENT_BONUS.Atk_Percentage:
            p_targetCharacter.combatComponent.AddAttackBaseOnPercentage(-1 * (p_equipData.equipmentUpgradeData.AdditionalAttackPercentage / 100f));
            break;
            case EQUIPMENT_BONUS.Max_HP_Actual:
            p_targetCharacter.combatComponent.AdjustMaxHPModifier(-1 * (int)p_equipData.equipmentUpgradeData.AdditionalmaxHPActual);
            break;
            case EQUIPMENT_BONUS.Max_HP_Percentage:
            float addedHP = p_targetCharacter.maxHP * (p_equipData.equipmentUpgradeData.AdditionalmaxHPPercentage / 100f);
            p_targetCharacter.combatComponent.AdjustMaxHPModifier(-1 * (int)addedHP);
            break;
            case EQUIPMENT_BONUS.Increased_Piercing:
            p_targetCharacter.piercingAndResistancesComponent.AdjustPiercing(-1 * p_equipData.equipmentUpgradeData.AdditionalPiercing);
            break;
        }
    }
}
