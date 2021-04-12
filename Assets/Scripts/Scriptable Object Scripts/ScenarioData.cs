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
}