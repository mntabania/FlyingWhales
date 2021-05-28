using System;
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
    public ELEMENTAL_TYPE elementAttackBonus;
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
}