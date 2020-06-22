using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Player Skill Loadout", menuName = "Scriptable Objects/Player Skill Loadout")]
public class PlayerSkillLoadout : ScriptableObject {
    public PLAYER_ARCHETYPE archetype;
    public PlayerSkillLoadoutData spells;
    public PlayerSkillLoadoutData afflictions;
    public PlayerSkillLoadoutData minions;
    public PlayerSkillLoadoutData structures;
    public PlayerSkillLoadoutData miscs;
}

[System.Serializable]
public class PlayerSkillLoadoutData {
    public int extraSlots;
    public List<SPELL_TYPE> fixedSkills;
}