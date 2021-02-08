﻿using System.Collections;
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
    public Sprite buttonSprite;
    public VideoClip tooltipVideoClip;
    public Texture tooltipImage;

    public List<int> upgradeCosts;
    public int unlockCost = 0;
    public RequirementData requirementData;
    public int tier;
    
    [Header("Context Menu")]
    public Sprite contextMenuIcon;
    public int contextMenuColumn;

    [Header("Player Action Icon")]
    public Sprite playerActionIcon;
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