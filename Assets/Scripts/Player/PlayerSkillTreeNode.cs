using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerSkillTreeNode {
    //public SPELL_TYPE skill;
    public int manaCost;
    public int charges;
    public int cooldown;
    public int threat;
    public int threatPerHour;
    public int expCost;
    public Sprite buttonSprite;
    public SPELL_TYPE[] unlockedSkills;
}

[System.Serializable]
public class PlayerSkillTreeNodeData {
    public SPELL_TYPE skill;
    public int manaCost;
    public int charges;
    public int cooldown;
    public int threat;
    public int threatPerHour;
}