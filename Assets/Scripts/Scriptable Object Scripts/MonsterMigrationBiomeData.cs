using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Monster Migration Data", menuName = "Scriptable Objects/Monster Migration")]
public class MonsterMigrationBiomeData : ScriptableObject {
    public MonsterMigrationBiomeAtomizedData[] dataList;
}

[System.Serializable]
public class MonsterMigrationBiomeAtomizedData {
    public SUMMON_TYPE monsterType;
    public int minRange;
    public int maxRange;
    public int weight;
    public override string ToString() {
        return $"{monsterType.ToString()} - Min: {minRange.ToString()} - Max: {maxRange.ToString()} - Weight: {weight.ToString()}";
    }
}