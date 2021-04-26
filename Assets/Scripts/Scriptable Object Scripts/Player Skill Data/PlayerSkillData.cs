using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UtilityScripts;

[CreateAssetMenu(fileName = "New Player Skill Data", menuName = "Scriptable Objects/Player Skills/Player Skill Data")]
public class PlayerSkillData : ScriptableObject {
    public PLAYER_SKILL_TYPE skill;
    public int manaCost;
    public int charges;
    public int cooldown;
    public int threat;
    public int threatPerHour;
    public int expCost;
    public int cheatedLevel;
    public float pierce;
    public int bonusChargeWhenUnlocked;
    public int unlockChargeOnPortalUpgrade = 1;
    public Sprite buttonSprite;
    public VideoClip tooltipVideoClip;
    public Texture tooltipImage;

    public int unlockCost = 0;
    public int tier;
    public int baseLoadoutWeight;
    public RESISTANCE resistanceType;
    public PLAYER_ARCHETYPE archetypeWeightedBonus;
    [Tooltip("If true, this power will show up in the release ability menu even if it is already learned by the player")]
    public bool canBeReleasedEvenIfLearned;
    
    [Header("Context Menu")]
    public Sprite contextMenuIcon;
    public int contextMenuColumn;

    [Header("Player Action Icon")]
    public Sprite playerActionIcon;

    public bool isNonUpgradeable;

    public bool isAffliction;

    public bool isLockedBaseOnRequirements; //used by plague and plague rats

    [Space]
    [Header("--------------Upgrade Related---------------")]
    public RequirementData requirementData;
    public SkillUpgradeData skillUpgradeData;
    public AfflictionUpgradeData afflictionUpgradeData;

    public int GetManaCostBaseOnLevel(int level) {
        return SpellUtilities.GetModifiedSpellCost(skillUpgradeData.GetManaCostPerLevel(level), WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification());
    }

    public int GetCoolDownBaseOnLevel(int level) {
        if (!isAffliction) {
            return SpellUtilities.GetModifiedSpellCost(skillUpgradeData.GetCoolDownPerLevel(level), WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCooldownSpeedModification());
        } else {
            return SpellUtilities.GetModifiedSpellCost(afflictionUpgradeData.GetCoolDownPerLevel(level), WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCooldownSpeedModification());
        }
    }

    public int GetMaxChargesBaseOnLevel(int level) {
        return SpellUtilities.GetModifiedSpellCost(skillUpgradeData.GetChargesBaseOnLevel(level), WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetChargeCostsModification());
    }
}

//[System.Serializable]
//public class PlayerSkillDataCopy {
//    public SPELL_TYPE skill;
//    public int manaCost;
//    public int charges;
//    public int cooldown;
//    public int threat;
//    public int threatPerHour;
//}