using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

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
    public Sprite buttonSprite;
    public VideoClip tooltipVideoClip;
    public Texture tooltipImage;

    public int unlockCost = 0;
    public int tier;
    public int baseLoadoutWeight;
    public RESISTANCE resistanceType;
    public PLAYER_ARCHETYPE archetypeWeightedBonus;

    [Header("Context Menu")]
    public Sprite contextMenuIcon;
    public int contextMenuColumn;

    [Header("Player Action Icon")]
    public Sprite playerActionIcon;

    [Space]
    [Header("--------------Upgrade Related---------------")]
    public RequirementData requirementData;
    public SkillUpgradeData skillUpgradeData;

    public int GetManaCostBaseOnLevel(int level) {
        return skillUpgradeData.GetManaCostPerLevel(level);
    }

    public int GetCoolDownBaseOnLevel(int level) {
        return skillUpgradeData.GetCoolDownPerLevel(level);
    }

    public int GetMaxChargesBaseOnLevel(int level) {
        return skillUpgradeData.GetChargesBaseOnLevel(level);
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