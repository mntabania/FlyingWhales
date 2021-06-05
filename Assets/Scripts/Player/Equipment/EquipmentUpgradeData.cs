﻿using System;
using UnityEngine;
using System.Collections.Generic;
using UtilityScripts;

[System.Serializable]
public class EquipmentUpgradeData {
    [HideInInspector]
    public List<EQUIPMENT_BONUS> bonuses = new List<EQUIPMENT_BONUS>();
    [HideInInspector]
    public float AdditionalPiercing;
    [HideInInspector]
    public float AdditionalMaxHPPercentage;
    [HideInInspector]
    public int AdditionalMaxHPActual;
    [HideInInspector]
    public float AdditionalAttackPercentage;
    [HideInInspector]
    public int AdditionalAttackActual;
    [HideInInspector]
    public float AdditionalIntPercentage;
    [HideInInspector]
    public int AdditionalIntActual;
    [HideInInspector]
    public int AdditionalCritRate;
    [HideInInspector]
    public float additionalResistanceBonus;
    [HideInInspector]
    public ELEMENTAL_TYPE elementAttackBonus = ELEMENTAL_TYPE.Normal;
    [HideInInspector]
    public EQUIPMENT_SLAYER_BONUS slayerBonus;
    [HideInInspector]
    public EQUIPMENT_WARD_BONUS wardBonus;

    public float GetProcessedAdditionalPiercing(EQUIPMENT_QUALITY p_quality) {
        float processedPiercing = AdditionalPiercing;
        switch (p_quality) {
            case EQUIPMENT_QUALITY.High:
            processedPiercing += (processedPiercing * .25f);
            break;
            case EQUIPMENT_QUALITY.Premium:
            processedPiercing += (processedPiercing * .5f);
            break;
        }
        return processedPiercing;
    }

    public float GetProcessedAdditionalResistanceBonus(EQUIPMENT_QUALITY p_quality) {
        float resistanceBonus = additionalResistanceBonus;
        switch (p_quality) {
            case EQUIPMENT_QUALITY.High:
            resistanceBonus += (resistanceBonus * .25f);
            break;
            case EQUIPMENT_QUALITY.Premium:
            resistanceBonus += (resistanceBonus * .5f);
            break;
        }
        return resistanceBonus;
    }

    public int GetProcessedAdditionalAttack(EQUIPMENT_QUALITY p_quality) {
        int processedAttack = AdditionalAttackActual;
        switch (p_quality) {
            case EQUIPMENT_QUALITY.High:
            processedAttack += (int)(processedAttack * .25f);
            break;
            case EQUIPMENT_QUALITY.Premium:
            processedAttack += (int)(processedAttack * .5f);
            break;
        }
        return processedAttack;
    }

    public float GetProcessedAdditionalAttackPercentage(EQUIPMENT_QUALITY p_quality) {
        float processedAttack = AdditionalAttackPercentage;
        switch (p_quality) {
            case EQUIPMENT_QUALITY.High:
            processedAttack += (int)(processedAttack * .25f);
            break;
            case EQUIPMENT_QUALITY.Premium:
            processedAttack += (int)(processedAttack * .5f);
            break;
        }
        return processedAttack;
    }

    public int GetProcessedAdditionalInt(EQUIPMENT_QUALITY p_quality) {
        int processedInt = AdditionalIntActual;
        switch (p_quality) {
            case EQUIPMENT_QUALITY.High:
            processedInt += (int)(processedInt * .25f);
            break;
            case EQUIPMENT_QUALITY.Premium:
            processedInt += (int)(processedInt * .5f);
            break;
        }
        return processedInt;
    }

    public int GetProcessedAdditionalCritRate(EQUIPMENT_QUALITY p_quality) {
        int processedCritRate = AdditionalCritRate;
        switch (p_quality) {
            case EQUIPMENT_QUALITY.High:
            processedCritRate += (int)(processedCritRate * .25f);
            break;
            case EQUIPMENT_QUALITY.Premium:
            processedCritRate += (int)(processedCritRate * .5f);
            break;
        }
        return processedCritRate;
    }

    public int GetProcessedAdditionalmaxHP(EQUIPMENT_QUALITY p_quality) {
        int processedMaxHP = AdditionalMaxHPActual;
        switch (p_quality) {
            case EQUIPMENT_QUALITY.High:
            processedMaxHP += (int)(processedMaxHP * .25f);
            break;
            case EQUIPMENT_QUALITY.Premium:
            processedMaxHP += (int)(processedMaxHP * .5f);
            break;
        }
        return processedMaxHP;
    }

    public float GetProcessedAdditionalmaxHPPercentage(EQUIPMENT_QUALITY p_quality) {
        float processedMaxHP = AdditionalMaxHPPercentage;
        switch (p_quality) {
            case EQUIPMENT_QUALITY.High:
            processedMaxHP += (int)(processedMaxHP * .25f);
            break;
            case EQUIPMENT_QUALITY.Premium:
            processedMaxHP += (int)(processedMaxHP * .5f);
            break;
        }
        return processedMaxHP;
    }
    public string GetBonusDescription(EQUIPMENT_QUALITY p_quality) {
        string descripton = String.Empty;
        if (bonuses.Contains(EQUIPMENT_BONUS.Increased_Piercing)) {
            if (AdditionalPiercing > 0) {
                descripton += ("Additional Piercing: " + GetProcessedAdditionalPiercing(p_quality) + "\n");
            }
        }
        if (bonuses.Contains(EQUIPMENT_BONUS.Str_Actual)) {
            if (AdditionalAttackActual > 0) {
                descripton += ("Additional Attack(Actual): " + GetProcessedAdditionalAttack(p_quality) + "\n");
            }
        }
        if (bonuses.Contains(EQUIPMENT_BONUS.Str_Percentage)) {
            if (AdditionalAttackPercentage > 0) {
                descripton += ("Additional Attack(%): " + GetProcessedAdditionalAttackPercentage(p_quality) + "\n");
            }
        }
        
        if (bonuses.Contains(EQUIPMENT_BONUS.Max_HP_Actual)) {
            if (AdditionalMaxHPActual > 0) {
                descripton += ("Additional Max HP(Actual): " + GetProcessedAdditionalmaxHP(p_quality) + "\n");
            }
        }
        
        if (bonuses.Contains(EQUIPMENT_BONUS.Max_HP_Percentage)) {
            if (AdditionalMaxHPPercentage > 0) {
                descripton += ("Additional Max HP(%): " + GetProcessedAdditionalmaxHPPercentage(p_quality) + "\n");
            }
        }
        if (bonuses.Contains(EQUIPMENT_BONUS.Int_Actual)) {
            if (AdditionalIntActual > 0) {
                descripton += ("Additional int(Actual): " + GetProcessedAdditionalResistanceBonus(p_quality) + "\n");
            }
        }
        if (bonuses.Contains(EQUIPMENT_BONUS.Int_Percentage)) {
            if (AdditionalIntPercentage > 0) {
                descripton += ("Additional Int(%): " + GetProcessedAdditionalInt(p_quality) + "\n");
            }
        }
        if (bonuses.Contains(EQUIPMENT_BONUS.Attack_Element)) {
            descripton += ("Weapon Element: " + elementAttackBonus + "\n");
        }

        if (bonuses.Contains(EQUIPMENT_BONUS.Slayer_Bonus)) {
            descripton += ("Slayer Bonus: " + slayerBonus + "\n");
        }
        
        if (bonuses.Contains(EQUIPMENT_BONUS.Ward_Bonus)) {
            descripton += ("ward Bonus: " + wardBonus + "\n");
        }
        
        if (bonuses.Contains(EQUIPMENT_BONUS.Flight)) {
            descripton += ("Flight: YES" + "\n");
        }
        if (bonuses.Contains(EQUIPMENT_BONUS.Increased_3_Random_Resistance) || bonuses.Contains(EQUIPMENT_BONUS.Increased_4_Random_Resistance) || bonuses.Contains(EQUIPMENT_BONUS.Increased_5_Random_Resistance)) {
            if (additionalResistanceBonus > 0) {
                descripton += ("Additional Resistance Bonus: " + GetProcessedAdditionalResistanceBonus(p_quality) + "\n");
            }
        }
        
        return descripton;
    }
}