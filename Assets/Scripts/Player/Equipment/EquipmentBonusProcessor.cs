using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Traits;
using UtilityScripts;
public static class EquipmentBonusProcessor
{
    private static Dictionary<EQUIPMENT_SLAYER_BONUS, string> traitDictionaryForSlayer = new Dictionary<EQUIPMENT_SLAYER_BONUS, string>
    {
            { EQUIPMENT_SLAYER_BONUS.Monster_Slayer, "Monster Slayer" },
            { EQUIPMENT_SLAYER_BONUS.Elf_Slayer, "Elf Slayer" },
            { EQUIPMENT_SLAYER_BONUS.Human_Slayer, "Human Slayer" },
            { EQUIPMENT_SLAYER_BONUS.Demon_Slayer, "Demon Slayer" },
            { EQUIPMENT_SLAYER_BONUS.Undead_SLayer, "Undead Slayer" },
    };

    private static Dictionary<EQUIPMENT_WARD_BONUS, string> traitDictionaryForWard = new Dictionary<EQUIPMENT_WARD_BONUS, string>
    {
            { EQUIPMENT_WARD_BONUS.Monster_Ward, "Monster Ward" },
            { EQUIPMENT_WARD_BONUS.Elf_Wawrd, "Elf Ward" },
            { EQUIPMENT_WARD_BONUS.Human_Ward, "Human Ward" },
            { EQUIPMENT_WARD_BONUS.Demon_Ward, "Demon Ward" },
            { EQUIPMENT_WARD_BONUS.Undead_Ward, "Undead Ward" },
    };

    public static void ApplyEquipBonusToTarget(EquipmentItem p_equipItem, Character p_targetCharacter, bool p_initializedStackCountOnly = false) {
        if (p_equipItem.equipmentData == null) {
            p_equipItem.AssignData();
        }
        p_equipItem.equipmentData.equipmentUpgradeData.bonuses.ForEach((eachBonus) => {
            ApplyEachBonusToTarget(p_equipItem, eachBonus, p_targetCharacter, p_initializedStackCountOnly);
        });
        p_equipItem.addedBonus.ForEach((eachBonus) => {
            if (eachBonus == EQUIPMENT_BONUS.Slayer_Bonus) {
                ApplyEachBonusToTarget(p_equipItem, eachBonus, p_targetCharacter, p_initializedStackCountOnly, p_slayerBonus: p_equipItem.randomSlayerBonus);
            }
            if (eachBonus == EQUIPMENT_BONUS.Ward_Bonus) {
                ApplyEachBonusToTarget(p_equipItem, eachBonus, p_targetCharacter, p_initializedStackCountOnly, p_wardBonus: p_equipItem.randomWardBonus);
            }
        });
    }

    public static void RemoveEquipBonusToTarget(EquipmentItem p_equipItem, Character p_targetCharacter) {
        if (p_equipItem.equipmentData == null) {
            p_equipItem.AssignData();
        }
        p_equipItem.equipmentData.equipmentUpgradeData.bonuses.ForEach((eachBonus) => {
            RemoveEachBonusToTarget(p_equipItem, eachBonus, p_targetCharacter);
        });
        p_equipItem.addedBonus.ForEach((eachBonus) => {
            if (eachBonus == EQUIPMENT_BONUS.Slayer_Bonus) {
                RemoveEachBonusToTarget(p_equipItem, eachBonus, p_targetCharacter, p_slayerBonus: p_equipItem.randomSlayerBonus);
            }
            if (eachBonus == EQUIPMENT_BONUS.Ward_Bonus) {
                RemoveEachBonusToTarget(p_equipItem, eachBonus, p_targetCharacter, p_wardBonus: p_equipItem.randomWardBonus);
            }
        });
    }

    static void ApplyEachBonusToTarget(EquipmentItem p_equipItem, EQUIPMENT_BONUS p_equipBonus, Character p_targetCharacter, bool p_initializedStackCountOnly = false, EQUIPMENT_WARD_BONUS p_wardBonus = EQUIPMENT_WARD_BONUS.None, EQUIPMENT_SLAYER_BONUS p_slayerBonus = EQUIPMENT_SLAYER_BONUS.None) {
        switch (p_equipBonus) {
            case EQUIPMENT_BONUS.Str_Actual:
            if (p_initializedStackCountOnly) {
                return;
            }
            p_targetCharacter.combatComponent.AdjustStrengthModifier(p_equipItem.equipmentData.equipmentUpgradeData.GetProcessedAdditionalAttack(p_equipItem.quality));
            break;
            case EQUIPMENT_BONUS.Str_Percentage:
            if (p_initializedStackCountOnly) {
                return;
            }
            //float computedAttack = p_targetCharacter.combatComponent.unModifiedAttack * (p_equipItem.equipmentData.equipmentUpgradeData.AdditionalAttackPercentage / 100f);
            p_targetCharacter.combatComponent.AdjustStrengthPercentModifier(p_equipItem.equipmentData.equipmentUpgradeData.AdditionalAttackPercentage);
            break;
            case EQUIPMENT_BONUS.Int_Actual:
            if (p_initializedStackCountOnly) {
                return;
            }
            p_targetCharacter.combatComponent.AdjustIntelligenceModifier(p_equipItem.equipmentData.equipmentUpgradeData.GetProcessedAdditionalInt(p_equipItem.quality));
            break;
            case EQUIPMENT_BONUS.Int_Percentage:
            if (p_initializedStackCountOnly) {
                return;
            }
            //float computedAttack = p_targetCharacter.combatComponent.unModifiedAttack * (p_equipItem.equipmentData.equipmentUpgradeData.AdditionalAttackPercentage / 100f);
            p_targetCharacter.combatComponent.AdjustIntelligencePercentModifier(p_equipItem.equipmentData.equipmentUpgradeData.GetProcessedAdditionalIntPercentage(p_equipItem.quality));
            break;
            case EQUIPMENT_BONUS.Crit_Rate_Actual:
            if (p_initializedStackCountOnly) {
                return;
            }
            p_targetCharacter.combatComponent.AdjustCritRate(p_equipItem.equipmentData.equipmentUpgradeData.GetProcessedAdditionalCritRate(p_equipItem.quality));
            break;
            case EQUIPMENT_BONUS.Max_HP_Actual:
            if (p_initializedStackCountOnly) {
                return;
            }
            p_targetCharacter.combatComponent.AdjustMaxHPModifier(p_equipItem.equipmentData.equipmentUpgradeData.GetProcessedAdditionalmaxHP(p_equipItem.quality));
            break;
            case EQUIPMENT_BONUS.Max_HP_Percentage:
            if (p_initializedStackCountOnly) {
                return;
            }
            //float addedHP = p_targetCharacter.combatComponent.unModifiedMaxHP * (p_equipItem.equipmentData.equipmentUpgradeData.AdditionalmaxHPPercentage / 100f);
            p_targetCharacter.combatComponent.AdjustMaxHPPercentModifier(p_equipItem.equipmentData.equipmentUpgradeData.AdditionalMaxHPPercentage);
            break;
            case EQUIPMENT_BONUS.Increased_Piercing:
            if (p_initializedStackCountOnly) {
                return;
            }
            p_targetCharacter.piercingAndResistancesComponent.AdjustBasePiercing(p_equipItem.equipmentData.equipmentUpgradeData.GetProcessedAdditionalPiercing(p_equipItem.quality));
            break;
            case EQUIPMENT_BONUS.Attack_Element:
            if (p_initializedStackCountOnly) {
                return;
            }
            p_targetCharacter.combatComponent.SetElementalType(p_equipItem.equipmentData.equipmentUpgradeData.elementAttackBonus);
            break;
            case EQUIPMENT_BONUS.Increased_3_Random_Resistance:
            case EQUIPMENT_BONUS.Increased_4_Random_Resistance:
            case EQUIPMENT_BONUS.Increased_5_Random_Resistance:
            if (p_initializedStackCountOnly) {
                return;
            }
            ApplyResistanceBonusOnCharacter(p_equipItem, p_targetCharacter);
            break;
            case EQUIPMENT_BONUS.Slayer_Bonus:
            EQUIPMENT_SLAYER_BONUS esb = p_equipItem.equipmentData.equipmentUpgradeData.slayerBonus;
            if (p_slayerBonus != EQUIPMENT_SLAYER_BONUS.None) {
                esb = p_slayerBonus;
            }
            if (p_targetCharacter.traitContainer.HasTrait(traitDictionaryForSlayer[esb])) {
                Trait trait = p_targetCharacter.traitContainer.GetTraitOrStatus<Trait>(traitDictionaryForSlayer[esb]);
                Slayer monsterSlayerTrait = trait as Slayer;
                monsterSlayerTrait.stackCount++;
            } else {
                p_targetCharacter.traitContainer.AddTrait(p_targetCharacter, traitDictionaryForSlayer[esb]);
                Trait trait = p_targetCharacter.traitContainer.GetTraitOrStatus<Trait>(traitDictionaryForSlayer[esb]);
                Slayer monsterSlayerTrait = trait as Slayer;
                monsterSlayerTrait.stackCount++;
            }
            break;
            case EQUIPMENT_BONUS.Ward_Bonus:
            EQUIPMENT_WARD_BONUS ewb = p_equipItem.equipmentData.equipmentUpgradeData.wardBonus;
            if (p_wardBonus != EQUIPMENT_WARD_BONUS.None) {
                ewb = p_wardBonus;
            }
            if (p_targetCharacter.traitContainer.HasTrait(traitDictionaryForWard[ewb])) {
                Trait trait = p_targetCharacter.traitContainer.GetTraitOrStatus<Trait>(traitDictionaryForWard[ewb]);
                Ward monsterSLayerWard = trait as Ward;
                monsterSLayerWard.stackCount++;
            } else {
                p_targetCharacter.traitContainer.AddTrait(p_targetCharacter, traitDictionaryForWard[ewb]);
                Trait trait = p_targetCharacter.traitContainer.GetTraitOrStatus<Trait>(traitDictionaryForWard[ewb]);
                Ward monsterWard = trait as Ward;
                monsterWard.stackCount++;
            }
            break;
            case EQUIPMENT_BONUS.Flight:
            if (p_targetCharacter.traitContainer.HasTrait("Flying")) {
                Trait trait = p_targetCharacter.traitContainer.GetTraitOrStatus<Trait>("Flying");
                Flying flyingTrait = trait as Flying;
                flyingTrait.stackCount++;
            } else {
                p_targetCharacter.movementComponent.SetToFlying();
                Trait trait = p_targetCharacter.traitContainer.GetTraitOrStatus<Trait>("Flying");
                Flying flyingTrait = trait as Flying;
                flyingTrait.stackCount++;
            }
            break;
        }
    }
    static void RemoveEachBonusToTarget(EquipmentItem p_equipItem, EQUIPMENT_BONUS p_equipBonus, Character p_targetCharacter, EQUIPMENT_WARD_BONUS p_wardBonus = EQUIPMENT_WARD_BONUS.None, EQUIPMENT_SLAYER_BONUS p_slayerBonus = EQUIPMENT_SLAYER_BONUS.None) {
        switch (p_equipBonus) {
            case EQUIPMENT_BONUS.Str_Actual:
            p_targetCharacter.combatComponent.AdjustStrengthModifier(-p_equipItem.equipmentData.equipmentUpgradeData.GetProcessedAdditionalAttack(p_equipItem.quality));
            break;
            case EQUIPMENT_BONUS.Str_Percentage:
            //float computedAttack = p_targetCharacter.combatComponent.unModifiedAttack * (p_equipItem.equipmentData.equipmentUpgradeData.AdditionalAttackPercentage / 100f);
            p_targetCharacter.combatComponent.AdjustStrengthPercentModifier(-p_equipItem.equipmentData.equipmentUpgradeData.AdditionalAttackPercentage);
            break;
            case EQUIPMENT_BONUS.Int_Actual:
            p_targetCharacter.combatComponent.AdjustIntelligenceModifier(-p_equipItem.equipmentData.equipmentUpgradeData.GetProcessedAdditionalInt(p_equipItem.quality));
            break;
            case EQUIPMENT_BONUS.Int_Percentage:
            //float computedAttack = p_targetCharacter.combatComponent.unModifiedAttack * (p_equipItem.equipmentData.equipmentUpgradeData.AdditionalAttackPercentage / 100f);
            p_targetCharacter.combatComponent.AdjustIntelligencePercentModifier(-p_equipItem.equipmentData.equipmentUpgradeData.GetProcessedAdditionalIntPercentage(p_equipItem.quality));
            break;
            case EQUIPMENT_BONUS.Crit_Rate_Actual:
            //float computedAttack = p_targetCharacter.combatComponent.unModifiedAttack * (p_equipItem.equipmentData.equipmentUpgradeData.AdditionalAttackPercentage / 100f);
            p_targetCharacter.combatComponent.AdjustCritRate(-p_equipItem.equipmentData.equipmentUpgradeData.GetProcessedAdditionalCritRate(p_equipItem.quality));
            break;
            case EQUIPMENT_BONUS.Max_HP_Actual:
            p_targetCharacter.combatComponent.AdjustMaxHPModifier(-p_equipItem.equipmentData.equipmentUpgradeData.GetProcessedAdditionalmaxHP(p_equipItem.quality));
            break;
            case EQUIPMENT_BONUS.Max_HP_Percentage:
            //float addedHP = p_targetCharacter.combatComponent.unModifiedMaxHP * (p_equipItem.equipmentData.equipmentUpgradeData.AdditionalmaxHPPercentage / 100f);
            p_targetCharacter.combatComponent.AdjustMaxHPPercentModifier(-p_equipItem.equipmentData.equipmentUpgradeData.AdditionalMaxHPPercentage);
            break;
            case EQUIPMENT_BONUS.Increased_Piercing:
            p_targetCharacter.piercingAndResistancesComponent.AdjustBasePiercing(-p_equipItem.equipmentData.equipmentUpgradeData.GetProcessedAdditionalPiercing(p_equipItem.quality));
            break;
            case EQUIPMENT_BONUS.Attack_Element:
            EquipmentComponent ec = p_targetCharacter.equipmentComponent;
            if (ec.allEquipments.Count > 0 && ec.allEquipments[ec.allEquipments.Count - 1] == p_equipItem){
                EquipmentItem ei = ec.GetRandomRemainingEquipment(p_equipItem);
                ProcessElementAfterRemovingSomeItem(ec, ei, p_targetCharacter);
            }
            break;
            case EQUIPMENT_BONUS.Increased_3_Random_Resistance:
            case EQUIPMENT_BONUS.Increased_4_Random_Resistance:
            case EQUIPMENT_BONUS.Increased_5_Random_Resistance:
            RemoveResistanceBonusOnCharacter(p_equipItem, p_targetCharacter);
            break;
            case EQUIPMENT_BONUS.Slayer_Bonus:
            EQUIPMENT_SLAYER_BONUS esb = p_equipItem.equipmentData.equipmentUpgradeData.slayerBonus;
            if (p_slayerBonus != EQUIPMENT_SLAYER_BONUS.None) {
                esb = p_slayerBonus;
            }
            if (p_targetCharacter.traitContainer.HasTrait(traitDictionaryForSlayer[esb])) {
                Trait trait = p_targetCharacter.traitContainer.GetTraitOrStatus<Trait>(traitDictionaryForSlayer[esb]);
                Slayer monsterSlayerTrait = trait as Slayer;
                monsterSlayerTrait.stackCount--;
                if (monsterSlayerTrait.stackCount <= 0) {
                    p_targetCharacter.traitContainer.RemoveTrait(p_targetCharacter, traitDictionaryForSlayer[esb]);
                }
            }
            break;
            case EQUIPMENT_BONUS.Ward_Bonus:
            EQUIPMENT_WARD_BONUS ewb = p_equipItem.equipmentData.equipmentUpgradeData.wardBonus;
            if (p_wardBonus != EQUIPMENT_WARD_BONUS.None) {
                ewb = p_wardBonus;
            }
            if (p_targetCharacter.traitContainer.HasTrait(traitDictionaryForWard[ewb])) {
                Trait trait = p_targetCharacter.traitContainer.GetTraitOrStatus<Trait>(traitDictionaryForWard[ewb]);
                Ward monsterWard = trait as Ward;
                monsterWard.stackCount--;
                if (monsterWard.stackCount <= 0) {
                    p_targetCharacter.traitContainer.RemoveTrait(p_targetCharacter, traitDictionaryForWard[ewb]);
                }
            }
            break;
            case EQUIPMENT_BONUS.Flight:
            if (p_targetCharacter.traitContainer.HasTrait("Flying")) {
                Trait trait = p_targetCharacter.traitContainer.GetTraitOrStatus<Trait>("Flying");
                Flying flyingTrait = trait as Flying;
                flyingTrait.stackCount--;
                if (flyingTrait.stackCount <= 0) {
                    p_targetCharacter.movementComponent.SetToNonFlying();
                }
            }
            break;
        }
    }

    static void ProcessElementAfterRemovingSomeItem(EquipmentComponent ec, EquipmentItem ei, Character p_targetCharacter) {
        if (ei != null) {
            if (ei.equipmentData.equipmentUpgradeData.bonuses.Contains(EQUIPMENT_BONUS.Attack_Element)) {
                p_targetCharacter.combatComponent.SetElementalType(ei.equipmentData.equipmentUpgradeData.elementAttackBonus);
            }
        } else if (p_targetCharacter.combatComponent.elementalStatusWaitingList.Count > 0) {
            p_targetCharacter.combatComponent.UpdateElementalType();
        } else {
            p_targetCharacter.combatComponent.SetElementalType(p_targetCharacter.combatComponent.initialElementalType);
        }
    }

    static void ApplyResistanceBonusOnCharacter(EquipmentItem p_equipItem, Character p_targetCharacter) {
        for (int x = 0; x < p_equipItem.resistanceBonuses.Count; ++x) {
            p_targetCharacter.piercingAndResistancesComponent.AdjustResistance(p_equipItem.resistanceBonuses[x], p_equipItem.equipmentData.equipmentUpgradeData.GetProcessedAdditionalResistanceBonus(p_equipItem.quality));
        }
    }

    static void RemoveResistanceBonusOnCharacter(EquipmentItem p_equipItem, Character p_targetCharacter) {
        for (int x = 0; x < p_equipItem.resistanceBonuses.Count; ++x) {
            p_targetCharacter.piercingAndResistancesComponent.AdjustResistance(p_equipItem.resistanceBonuses[x], -p_equipItem.equipmentData.equipmentUpgradeData.GetProcessedAdditionalResistanceBonus(p_equipItem.quality));
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

        List<int> result = GameUtilities.GetUniqueRandomNumbersInBetween(1, (int)RESISTANCE.Physical, resistanceCount);
        foreach (var item in result) {
            RESISTANCE addElem = (RESISTANCE)item;
            p_equipItem.resistanceBonuses.Add(addElem);
        }
    }

    public static void SetBonusResistanceOnPowerCrystal(PowerCrystal p_crystal, int p_numberOfresistance) {
        /*
        List<int> result = GameUtilities.GetUniqueRandomNumbersInBetween(1, (int)RESISTANCE.Physical, p_numberOfresistance);

        foreach (var item in result) {
            RESISTANCE addElem = (RESISTANCE)item;
            p_crystal.resistanceBonuses.Add(addElem);
        }*/
        int res = GameUtilities.RandomBetweenTwoNumbers(1, ((int)RESISTANCE.Physical - 1));
        p_crystal.resistanceBonuses.Add((RESISTANCE)res);
    }

    public static float GetSlayerBonusDamage(Character p_damager, Character p_damageReceiver, float p_currentAmountDagame) {
        float adjustedAttack = 0;
        if (p_damageReceiver != null && p_damager != null && p_damager.traitContainer != null && p_damageReceiver.faction != null && p_damageReceiver.faction.factionType != null && p_damager.traitContainer.HasTrait(traitDictionaryForSlayer[EQUIPMENT_SLAYER_BONUS.Monster_Slayer]) && p_damageReceiver.isWildMonster) {
            adjustedAttack = p_currentAmountDagame * 0.5f;
        }
        if (p_damageReceiver != null && p_damager != null && p_damager.traitContainer != null && p_damager.traitContainer.HasTrait(traitDictionaryForSlayer[EQUIPMENT_SLAYER_BONUS.Human_Slayer]) && p_damageReceiver.race == RACE.HUMANS) {
            adjustedAttack = p_currentAmountDagame * 0.5f;
        }
        if (p_damageReceiver != null && p_damager != null && p_damager.traitContainer != null && p_damager.traitContainer.HasTrait(traitDictionaryForSlayer[EQUIPMENT_SLAYER_BONUS.Elf_Slayer]) && p_damageReceiver.race == RACE.ELVES) {
            adjustedAttack = p_currentAmountDagame * 0.5f;
        }
        if (p_damageReceiver != null && p_damager != null && p_damager.traitContainer != null && p_damager.traitContainer.HasTrait(traitDictionaryForSlayer[EQUIPMENT_SLAYER_BONUS.Demon_Slayer]) && p_damageReceiver.faction.factionType.type == FACTION_TYPE.Demons) {
            adjustedAttack = p_currentAmountDagame * 0.5f;
        }
        if (p_damageReceiver != null && p_damager != null && p_damager.traitContainer != null && p_damager.traitContainer.HasTrait(traitDictionaryForSlayer[EQUIPMENT_SLAYER_BONUS.Undead_SLayer]) && p_damageReceiver.IsUndead()) {
            adjustedAttack = p_currentAmountDagame * 0.5f;
        }
        return adjustedAttack;
    }

    public static float GetWardBonusDamage(Character p_damager, Character p_damageReceiver, float p_currentAmountDagame) {
        float adjustedAttack = 0;
        if (p_damageReceiver != null && p_damager != null && p_damageReceiver.traitContainer != null && p_damager.faction != null && p_damageReceiver.faction.factionType != null && p_damageReceiver.traitContainer.HasTrait(traitDictionaryForWard[EQUIPMENT_WARD_BONUS.Monster_Ward]) && p_damager.isWildMonster) {
            adjustedAttack = p_currentAmountDagame * 0.5f;
        }
        if (p_damageReceiver != null && p_damager != null && p_damageReceiver.traitContainer != null && p_damageReceiver.traitContainer.HasTrait(traitDictionaryForWard[EQUIPMENT_WARD_BONUS.Human_Ward]) && p_damager.race == RACE.HUMANS) {
            adjustedAttack = p_currentAmountDagame * 0.5f;
        }
        if (p_damageReceiver != null && p_damager != null && p_damageReceiver.traitContainer != null && p_damageReceiver.traitContainer.HasTrait(traitDictionaryForWard[EQUIPMENT_WARD_BONUS.Elf_Wawrd]) && p_damager.race == RACE.ELVES) {
            adjustedAttack = p_currentAmountDagame * 0.5f;
        }
        if (p_damageReceiver != null && p_damager != null && p_damageReceiver.traitContainer != null && p_damageReceiver.traitContainer.HasTrait(traitDictionaryForWard[EQUIPMENT_WARD_BONUS.Demon_Ward]) && p_damager.faction.factionType.type == FACTION_TYPE.Demons) {
            adjustedAttack = p_currentAmountDagame * 0.5f;
        }
        if (p_damageReceiver != null && p_damager != null && p_damageReceiver.traitContainer != null && p_damageReceiver.traitContainer.HasTrait(traitDictionaryForWard[EQUIPMENT_WARD_BONUS.Undead_Ward]) && p_damager.IsUndead()) {
            adjustedAttack = p_currentAmountDagame * 0.5f;
        }
        return adjustedAttack;
    }
}