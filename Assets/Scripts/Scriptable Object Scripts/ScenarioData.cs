using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Scenario Data", menuName = "Scriptable Objects/Scenario")]
public class ScenarioData : ScriptableObject {
    public TextAsset scenarioSettings;
    public MonsterMigrationBiomeAtomizedData[] faunaList;
    public PowerAndLevelDictionary spells;
    public PowerAndLevelDictionary afflictions;
    public PowerAndLevelDictionary minions;
    public PowerAndLevelDictionary structures;
    public PowerAndLevelDictionary skills;

    public int GetLevelForPower(PLAYER_SKILL_TYPE p_Power) {
        if (spells.ContainsKey(p_Power)) {
            return spells[p_Power];
        } else if (afflictions.ContainsKey(p_Power)) {
            return afflictions[p_Power];
        } else if (minions.ContainsKey(p_Power)) {
            return minions[p_Power];
        } else if (structures.ContainsKey(p_Power)) {
            return structures[p_Power];
        } else if (skills.ContainsKey(p_Power)) {
            return skills[p_Power];
        }
        return 0;
    }
}