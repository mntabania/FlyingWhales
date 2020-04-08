using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerSkillTreeNode {
    public SPELL_TYPE skill;
    //public PLAYER_SKILL_STATE state;
    //public int manaCost;
    //public int charges;
    //public int cooldown;
}

[System.Serializable]
public struct PlayerSkillTreeNodeID {
    public int id;
    public PlayerSkillTreeNodeID[] requirements;
}

[System.Serializable]
public class PlayerSkillTreeNodeData {
    public SPELL_TYPE skill;
    public PLAYER_SKILL_STATE state;
    public int manaCost;
    public int charges;
    public int cooldown;
}