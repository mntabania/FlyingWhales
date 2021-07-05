using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Scenario Data", menuName = "Scriptable Objects/Scenario")]
public class ScenarioData : ScriptableObject {
    public TextAsset scenarioSettings;
    public MonsterMigrationBiomeAtomizedData[] faunaList;
    
    [Header("Power Levels")]
    public PowerAndLevelDictionary spells;
    public PowerAndLevelDictionary afflictions;
    public PowerAndLevelDictionary minions;
    public PowerAndLevelDictionary structures;
    public PowerAndLevelDictionary skills;

    [Header("Loadouts")] 
    public List<ScenarioLoadoutData> loadoutData;
    public PlayerSkillLoadout puppetmasterOverride;
    public PlayerSkillLoadout lichOverride;
    public PlayerSkillLoadout ravagerOverride;
    
    

    /// <summary>
    /// Get 0 based starting level of a given power for this scenario
    /// </summary>
    /// <param name="p_Power">The power to check.</param>
    /// <returns>The starting level of the given power in this scenario</returns>
    public int GetLevelForPower(PLAYER_SKILL_TYPE p_Power) {
        if (spells.ContainsKey(p_Power)) {
            return Mathf.Max(spells[p_Power] - 1, 0);
        } else if (afflictions.ContainsKey(p_Power)) {
            return Mathf.Max(afflictions[p_Power] - 1, 0);
        } else if (minions.ContainsKey(p_Power)) {
            return Mathf.Max(minions[p_Power] - 1, 0);
        } else if (structures.ContainsKey(p_Power)) {
            return Mathf.Max(structures[p_Power] - 1, 0);
        } else if (skills.ContainsKey(p_Power)) {
            return Mathf.Max(skills[p_Power] - 1, 0);
        }
        return 0;
    }

    // public ScenarioLoadoutData GetScenarioLoadoutDataForArchetype(PLAYER_ARCHETYPE p_archetype) {
    //     for (int i = 0; i < loadoutData.Count; i++) {
    //         ScenarioLoadoutData data = loadoutData[i];
    //         if (data.archetype == p_archetype) {
    //             return data;
    //         }
    //     }
    //     return null;
    // }
}

[System.Serializable]
public class ScenarioLoadoutData {
    public PLAYER_ARCHETYPE archetype;
    [Header("Spells")]
    public int spellExtraSlots;
    public List<PLAYER_SKILL_TYPE> spellAvailableSkills;
    [Space(10)]
    [Header("Afflictions")]
    public int afflictionExtraSlots;
    public List<PLAYER_SKILL_TYPE> afflictionAvailableSkills;
    [Space(10)]
    [Header("Minions")]
    public int minionExtraSlots;
    public List<PLAYER_SKILL_TYPE> minionAvailableSkills;
    [Space(10)]
    [Header("Structures")]
    public int structureExtraSlots;
    public List<PLAYER_SKILL_TYPE> structureAvailableSkills;
    [Space(10)]
    [Header("Miscs")]
    public int miscExtraSlots;
    public List<PLAYER_SKILL_TYPE> miscAvailableSkills;
}