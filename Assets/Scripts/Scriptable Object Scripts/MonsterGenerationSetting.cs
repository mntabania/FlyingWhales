using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
[CreateAssetMenu(fileName = "New Monster Generation Setting", menuName = "Scriptable Objects/Monster Generation")]
 public class MonsterGenerationSetting : ScriptableObject {
     [SerializeField] private IntRange _iterations;
     [SerializeField] private BiomeMonsterDictionary _monsterChoices;
     
     #region getters
     public IntRange iterations => _iterations;
     #endregion

     public List<MonsterSetting> GetMonsterChoicesForBiome(BIOMES biome) {
         List<MonsterSetting> settings = new List<MonsterSetting>();
         if (_monsterChoices.ContainsKey(biome)) {
             settings.AddRange(_monsterChoices[biome]);
         }
         if (_monsterChoices.ContainsKey(BIOMES.NONE)) {
             settings.AddRange(_monsterChoices[BIOMES.NONE]);
         }
         return settings;
     }
 }

[System.Serializable]
public struct MonsterSetting {
    public SUMMON_TYPE monsterType;
    public IntRange minMaxRange;
}