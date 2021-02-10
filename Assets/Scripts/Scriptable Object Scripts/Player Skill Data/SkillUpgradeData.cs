using System;
using UnityEngine;
using System.Collections.Generic;
[System.Serializable]
public class SkillUpgradeData
{
    public List<int> upgradeCosts;//using chaos energy
    public List<int> manacostPerLevel;
    public List<int> chargesPerLevel;
    
    [HideInInspector]
    public List<UPGRADE_BONUS> bonuses = new List<UPGRADE_BONUS>();
    [HideInInspector]
    public List<int> additionalDamagePerLevel;
    [HideInInspector]
    public List<float> additionalPiercePerLevel;
    [HideInInspector]
    public List<float> additionalHpPercentagePerLevel;
    [HideInInspector]
    public List<float> additionalAttackPercentagePerLevel;
    [HideInInspector]
    public List<float> additionalHpValuePerLevel;
    [HideInInspector]
    public List<float> additionalAttackValuePerLevel;
    [HideInInspector]
    public List<float> additionalmanaReceivedPercentagePerLevel;
    [HideInInspector]
    public List<float> statsIncreasedPercentagePerLevel;
    [HideInInspector]
    public List<float> durationBonusPerLevel;
    [HideInInspector]
    public List<float> additionalMaxHPPercentagePerLevel;
    [HideInInspector]
    public List<float> additionalMaxHPActualPerLevel;
    [HideInInspector]
    public List<float> additionalChanceBonusPercentagePerLevel;
    [HideInInspector]
    public List<int> additionalTileRangeBonusPerLevel;
    [HideInInspector]
    public List<int> drainEnergyBonus;
    [HideInInspector]
    public List<int> drainHappinessBonus;

    public int GetUpgradeCostBaseOnLevel(int p_currentLevel) {
        return upgradeCosts[p_currentLevel];
    }

    public int GetChargesBaseOnLevel(int p_currentLevel) {
        return chargesPerLevel[p_currentLevel];
    }

    public float GetAdditionalDamageBaseOnLevel(int p_currentLevel) {
        return additionalDamagePerLevel[p_currentLevel];
    }

    public float GetAdditionalPiercePerLevelBaseOnLevel(int p_currentLevel) {
        return additionalPiercePerLevel[p_currentLevel];
    }

    public float GetAdditionalHpPercentagePerLevelBaseOnLevel(int p_currentLevel) {
        return additionalHpPercentagePerLevel[p_currentLevel];
    }

    public float GetAdditionalMaxHpActualPerLevelBaseOnLevel(int p_currentLevel) {
        return additionalMaxHPActualPerLevel[p_currentLevel];
    }

    public float GetAdditionalMaxHpPercentagePerLevelBaseOnLevel(int p_currentLevel) {
        return additionalMaxHPPercentagePerLevel[p_currentLevel];
    }

    public float GetAdditionalHpActualPerLevelBaseOnLevel(int p_currentLevel) {
        return additionalHpValuePerLevel[p_currentLevel];
    }

    public float GetAdditionalAttackPercentagePerLevelBaseOnLevel(int p_currentLevel) {
        return additionalAttackPercentagePerLevel[p_currentLevel];
    }

    public float GetAdditionalAttackActualPerLevelBaseOnLevel(int p_currentLevel) {
        return additionalAttackValuePerLevel[p_currentLevel];
    }

    public int GetManaCostPerLevel(int p_currentLevel) {
        return manacostPerLevel[p_currentLevel];
    }

    public float GetIncreaseStatsPercentagePerLevel(int p_currentLevel) {
        return statsIncreasedPercentagePerLevel[p_currentLevel];
    }

    public float GetDurationBonusPerLevel(int p_currentLevel) {
        return durationBonusPerLevel[p_currentLevel];
    }

    public int GetTileRangeBonusPerLevel(int p_currentLevel) {
        return additionalTileRangeBonusPerLevel[p_currentLevel];
    }

    public float GetChanceBonusPerLevel(int p_currentLevel) {
        return additionalChanceBonusPercentagePerLevel[p_currentLevel];
    }
    public float GetDrainEnergyBonus(int p_currentLevel) {
        return drainEnergyBonus[p_currentLevel];
    }

    public float GetDrainHappinessBonus(int p_currentLevel) {
        return drainHappinessBonus[p_currentLevel];
    }
}
