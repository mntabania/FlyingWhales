using System;
using UnityEngine;
using System.Collections.Generic;
using UtilityScripts;

[System.Serializable]
public class SkillUpgradeData
{
    public List<int> upgradeCosts;//using chaos energy
    public List<int> manacostPerLevel;
    public List<int> chargesPerLevel;
    
    [HideInInspector]
    public List<UPGRADE_BONUS> bonuses = new List<UPGRADE_BONUS>();
    [HideInInspector]
    public List<int> additionalDamagePerLevel = new List<int>();
    [HideInInspector]
    public List<float> additionalPiercePerLevel = new List<float>();
    [HideInInspector]
    public List<float> additionalHpPercentagePerLevel = new List<float>();
    [HideInInspector]
    public List<float> additionalAttackPercentagePerLevel = new List<float>();
    [HideInInspector]
    public List<int> additionalHpValuePerLevel = new List<int>();
    [HideInInspector]
    public List<int> additionalAttackValuePerLevel = new List<int>();
    [HideInInspector]
    public List<int> applyOnLevel = new List<int>();
    [HideInInspector]
    public List<float> additionalmanaReceivedPercentagePerLevel = new List<float>();
    [HideInInspector]
    public List<float> statsIncreasedPercentagePerLevel = new List<float>();
    [HideInInspector]
    public List<int> durationBonusPerLevel = new List<int>();
    [HideInInspector]
    public List<float> additionalMaxHPPercentagePerLevel = new List<float>();
    [HideInInspector]
    public List<int> additionalMaxHPActualPerLevel = new List<int>();
    [HideInInspector]
    public List<float> additionalChanceBonusPercentagePerLevel = new List<float>();
    [HideInInspector]
    public List<int> additionalTileRangeBonusPerLevel = new List<int>();
    [HideInInspector]
    public List<int> decreaseMovementSpeedPerLevel = new List<int>();
    [HideInInspector]
    public List<int> cooldownPerLevel = new List<int>();
    [HideInInspector]
    public List<int> skillMovementSpeed = new List<int>();
    [HideInInspector]

    public int GetUpgradeCostBaseOnLevel(int p_currentLevel) {
        if (upgradeCosts == null || upgradeCosts.Count <= 0) {
            return 0;
        }
        if (p_currentLevel >= upgradeCosts.Count) {
            return upgradeCosts[upgradeCosts.Count - 1];
        }
        return SpellUtilities.GetModifiedSpellCost(upgradeCosts[p_currentLevel], WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification());
    }
    public int GetAdditionalDamageBaseOnLevel(int p_currentLevel) {
        if (additionalDamagePerLevel == null || additionalDamagePerLevel.Count <= 0) {
            return 0;
        }
        if (p_currentLevel >= additionalDamagePerLevel.Count) {
            return additionalDamagePerLevel[upgradeCosts.Count - 1];
        }
        return additionalDamagePerLevel[p_currentLevel];
    }
    public float GetAdditionalPiercePerLevelBaseOnLevel(int p_currentLevel) {
        if (additionalPiercePerLevel == null || additionalPiercePerLevel.Count <= 0) {
            return 0;
        }
        if (p_currentLevel >= additionalPiercePerLevel.Count) {
            return additionalPiercePerLevel[upgradeCosts.Count - 1];
        }
        return additionalPiercePerLevel[p_currentLevel];
    }
    public float GetAdditionalHpPercentagePerLevelBaseOnLevel(int p_currentLevel) {
        if (additionalHpPercentagePerLevel == null || additionalHpPercentagePerLevel.Count <= 0) {
            return 0;
        }
        if (p_currentLevel >= additionalHpPercentagePerLevel.Count) {
            return additionalHpPercentagePerLevel[upgradeCosts.Count - 1];
        }
        return additionalHpPercentagePerLevel[p_currentLevel];
    }
    public int GetAdditionalMaxHpActualPerLevelBaseOnLevel(int p_currentLevel) {
        if (additionalMaxHPActualPerLevel == null || additionalMaxHPActualPerLevel.Count <= 0) {
            return 0;
        }
        if (p_currentLevel >= additionalMaxHPActualPerLevel.Count) {
            return additionalMaxHPActualPerLevel[upgradeCosts.Count - 1];
        }
        return additionalMaxHPActualPerLevel[p_currentLevel];
    }
    public float GetAdditionalMaxHpPercentagePerLevelBaseOnLevel(int p_currentLevel) {
        if (additionalMaxHPPercentagePerLevel == null || additionalMaxHPPercentagePerLevel.Count <= 0) {
            return 0;
        }
        if (p_currentLevel >= additionalMaxHPPercentagePerLevel.Count) {
            return additionalMaxHPPercentagePerLevel[upgradeCosts.Count - 1];
        }
        return additionalMaxHPPercentagePerLevel[p_currentLevel];
    }
    public int GetAdditionalHpActualPerLevelBaseOnLevel(int p_currentLevel) {
        if (additionalHpValuePerLevel == null || additionalHpValuePerLevel.Count <= 0) {
            return 0;
        }
        if (p_currentLevel >= additionalHpValuePerLevel.Count) {
            return additionalHpValuePerLevel[upgradeCosts.Count - 1];
        }
        return additionalHpValuePerLevel[p_currentLevel];
    }
    public float GetAdditionalAttackPercentagePerLevelBaseOnLevel(int p_currentLevel) {
        if (additionalAttackPercentagePerLevel == null || additionalAttackPercentagePerLevel.Count <= 0) {
            return 0;
        }
        if (p_currentLevel >= additionalAttackPercentagePerLevel.Count) {
            return additionalAttackPercentagePerLevel[upgradeCosts.Count - 1];
        }
        return additionalAttackPercentagePerLevel[p_currentLevel];
    }
    public int GetAdditionalAttackActualPerLevelBaseOnLevel(int p_currentLevel) {
        if (additionalAttackValuePerLevel == null || additionalAttackValuePerLevel.Count <= 0) {
            return 0;
        }
        if (p_currentLevel >= additionalAttackValuePerLevel.Count) {
            return additionalAttackValuePerLevel[upgradeCosts.Count - 1];
        }
        return additionalAttackValuePerLevel[p_currentLevel];
    }
    public float GetIncreaseStatsPercentagePerLevel(int p_currentLevel) {
        if (statsIncreasedPercentagePerLevel == null || statsIncreasedPercentagePerLevel.Count <= 0) {
            return 0;
        }
        if (p_currentLevel >= statsIncreasedPercentagePerLevel.Count) {
            return statsIncreasedPercentagePerLevel[upgradeCosts.Count - 1];
        }
        return statsIncreasedPercentagePerLevel[p_currentLevel];
    }
    public int GetDurationBonusPerLevel(int p_currentLevel) {
        if (durationBonusPerLevel == null || durationBonusPerLevel.Count <= 0) {
            return 0;
        }
        if (p_currentLevel >= durationBonusPerLevel.Count) {
            return durationBonusPerLevel[upgradeCosts.Count - 1];
        }
        return durationBonusPerLevel[p_currentLevel];
    }
    public int GetTileRangeBonusPerLevel(int p_currentLevel) {
        if (additionalTileRangeBonusPerLevel == null || additionalTileRangeBonusPerLevel.Count <= 0) {
            return 0;
        }
        if (p_currentLevel >= additionalTileRangeBonusPerLevel.Count) {
            return additionalTileRangeBonusPerLevel[upgradeCosts.Count - 1];
        }
        return additionalTileRangeBonusPerLevel[p_currentLevel];
    }
    public float GetChanceBonusPerLevel(int p_currentLevel) {
        if (additionalChanceBonusPercentagePerLevel == null || additionalChanceBonusPercentagePerLevel.Count <= 0) {
            return 0;
        }
        if (p_currentLevel >= additionalChanceBonusPercentagePerLevel.Count) {
            return additionalChanceBonusPercentagePerLevel[upgradeCosts.Count - 1];
        }
        return additionalChanceBonusPercentagePerLevel[p_currentLevel];
    }
    public int GetManaCostPerLevel(int p_currentLevel) {
        if (manacostPerLevel == null || manacostPerLevel.Count <= 0) {
            return -1;
        }
        if (p_currentLevel >= manacostPerLevel.Count) {
            return manacostPerLevel[upgradeCosts.Count - 1];
        }
        return manacostPerLevel[p_currentLevel];
    }
    public int GetCoolDownPerLevel(int p_currentLevel) {
        if (cooldownPerLevel == null || cooldownPerLevel.Count <= 0) {
            return -1;
        }
        if (p_currentLevel >= cooldownPerLevel.Count) {
            return cooldownPerLevel[upgradeCosts.Count - 1];
        }
        return cooldownPerLevel[p_currentLevel];
    }
    public int GetChargesBaseOnLevel(int p_currentLevel) {
        if (chargesPerLevel == null || chargesPerLevel.Count <= 0) {
            return -1;
        }
        if (p_currentLevel >= chargesPerLevel.Count) {
            return chargesPerLevel[upgradeCosts.Count - 1];
        }
        return chargesPerLevel[p_currentLevel];
    }
    public int GetSkillMovementSpeedPerLevel(int p_currentLevel) {
        if (skillMovementSpeed == null || skillMovementSpeed.Count <= 0) {
            return 0;
        }
        if (p_currentLevel >= skillMovementSpeed.Count) {
            return skillMovementSpeed[upgradeCosts.Count - 1];
        }
        return skillMovementSpeed[p_currentLevel];
    }

    public string GetDescriptionBaseOnFirstBonus(int p_currentLevel) {
        if (bonuses == null || bonuses.Count <= 0) {
            return "n/a";
        }
        return GetDescription(bonuses[0], p_currentLevel);
    }

    private string GetDescription(UPGRADE_BONUS p_upgradeBonus, int p_currentLevel) {
		switch (p_upgradeBonus) {
            case UPGRADE_BONUS.Pierce:
            if (additionalPiercePerLevel.Count <= 0) {
                return "Pierce: No Data";
            }
            if (p_currentLevel >= additionalPiercePerLevel.Count) {
                p_currentLevel = additionalPiercePerLevel.Count - 1;
            }
            return additionalPiercePerLevel[p_currentLevel] + "%";

            case UPGRADE_BONUS.Damage:
            if (additionalDamagePerLevel.Count <= 0) {
                return "Damage: No Data";
            }
            if (p_currentLevel >= additionalDamagePerLevel.Count) {
                p_currentLevel = additionalDamagePerLevel.Count - 1;
            }
            return additionalDamagePerLevel[p_currentLevel].ToString();

            case UPGRADE_BONUS.Duration:
            if (durationBonusPerLevel.Count <= 0) {
                return "Duration: No Data";
            }
            if (p_currentLevel >= durationBonusPerLevel.Count) {
                p_currentLevel = durationBonusPerLevel.Count - 1;
            }
            return durationBonusPerLevel[p_currentLevel].ToString();

            case UPGRADE_BONUS.Max_HP_Percentage:
            if (additionalMaxHPPercentagePerLevel.Count <= 0) {
                return "Max HP: No Data";
            }
            if (p_currentLevel >= additionalMaxHPPercentagePerLevel.Count) {
                p_currentLevel = additionalMaxHPPercentagePerLevel.Count - 1;
            }
            return additionalMaxHPPercentagePerLevel[p_currentLevel] + "%";

            case UPGRADE_BONUS.Atk_Percentage:
            if (additionalAttackPercentagePerLevel.Count <= 0) {
                return "Atk: No Data";
            }
            if (p_currentLevel >= additionalAttackPercentagePerLevel.Count) {
                p_currentLevel = additionalAttackPercentagePerLevel.Count - 1;
            }
            return additionalAttackPercentagePerLevel[p_currentLevel] + "%";

            case UPGRADE_BONUS.Amplify_Effect_By_Percentage:
            if (statsIncreasedPercentagePerLevel.Count <= 0) {
                return "Effect: No Data";
            }
            if (p_currentLevel >= statsIncreasedPercentagePerLevel.Count) {
                p_currentLevel = statsIncreasedPercentagePerLevel.Count - 1;
            }
            return statsIncreasedPercentagePerLevel[p_currentLevel] + "%";

            case UPGRADE_BONUS.Tile_Range:
            if (additionalTileRangeBonusPerLevel.Count <= 0) {
                return "Tiles: No Data";
            }
            if (p_currentLevel >= additionalTileRangeBonusPerLevel.Count) {
                p_currentLevel = additionalTileRangeBonusPerLevel.Count - 1;
            }
            return additionalTileRangeBonusPerLevel[p_currentLevel] + "%";
            case UPGRADE_BONUS.Cooldown:
            if (cooldownPerLevel.Count <= 0) {
                return "Tiles: No Data";
            }
            if (p_currentLevel >= cooldownPerLevel.Count) {
                p_currentLevel = cooldownPerLevel.Count - 1;
            }
            return cooldownPerLevel[p_currentLevel] + "%";
        }
        return "n/a";
	}
}