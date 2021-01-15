using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Scenario Data", menuName = "Scriptable Objects/Scenario")]
public class ScenarioData : ScriptableObject {
    public TextAsset scenarioSettings;
    public MonsterMigrationBiomeAtomizedData[] faunaList;
}