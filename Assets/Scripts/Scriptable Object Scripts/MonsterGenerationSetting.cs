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

     public WeightedDictionary<MonsterSetting> GetMonsterChoicesForBiome(BIOMES biome) {
         WeightedDictionary<MonsterSetting> settings = new WeightedDictionary<MonsterSetting>();
         if (_monsterChoices.ContainsKey(biome)) {
             List<MonsterSetting> biomeChoices = _monsterChoices[biome];
             for (int i = 0; i < biomeChoices.Count; i++) {
                 MonsterSetting monsterSetting = biomeChoices[i];
                 settings.AddElement(monsterSetting, monsterSetting.weight);
             }
         }
         if (_monsterChoices.ContainsKey(BIOMES.NONE)) {
             List<MonsterSetting> neutralChoices = _monsterChoices[BIOMES.NONE];
             for (int i = 0; i < neutralChoices.Count; i++) {
                 MonsterSetting monsterSetting = neutralChoices[i];
                 settings.AddElement(monsterSetting, monsterSetting.weight);
             }
         }
         return settings;
     }
     
 }

[System.Serializable]
public struct MonsterSetting {
    public int weight;
    public SUMMON_TYPE monsterType;
    public IntRange minMaxRange;
}