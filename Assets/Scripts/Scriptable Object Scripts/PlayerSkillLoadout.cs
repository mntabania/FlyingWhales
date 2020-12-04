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
    public PASSIVE_SKILL[] passiveSkills;

    public PLAYER_SKILL_TYPE[] availableSpells;
    public PLAYER_SKILL_TYPE[] availableAfflictions;
    public PLAYER_SKILL_TYPE[] availableMinions;
    public PLAYER_SKILL_TYPE[] availableStructures;
    public PLAYER_SKILL_TYPE[] availableMiscs;

}

[System.Serializable]
public class PlayerSkillLoadoutData {
    public int extraSlots;
    public List<PLAYER_SKILL_TYPE> fixedSkills;
}