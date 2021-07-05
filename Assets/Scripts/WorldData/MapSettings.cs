using System;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;
using Random = System.Random;

[System.Serializable]
public class MapSettings {
    public MAP_SIZE mapSize;
    public List<BIOMES> biomes;
    public bool disableAllMonsterMigrations;
    
    public MapSettings() {
        biomes = new List<BIOMES>();
        mapSize = MAP_SIZE.Small;
        disableAllMonsterMigrations = false;
    }
    public void SetMapSize(MAP_SIZE p_mapSize) {
        mapSize = p_mapSize;
    }
    public void AddBiome(BIOMES biome) {
        Debug.Log($"Added {biome.ToString()} to biomes");
        biomes.Add(biome);
    }
    public bool RemoveBiome(BIOMES biome) {
        Debug.Log($"Removed {biome.ToString()} from biomes");
        return biomes.Remove(biome);
    }
    public void ClearBiomes() {
        Debug.Log($"Cleared Biomes");
        biomes.Clear();
    }
    public int GetMaxBiomeCount() {
        switch (mapSize) {
            case MAP_SIZE.Small:
                return 1;
            case MAP_SIZE.Medium:
                return 2;
            case MAP_SIZE.Large:
                return 3;
            case MAP_SIZE.Extra_Large:
                return 4;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    public bool HasReachedMaxBiomeCount() {
        return biomes.Count >= GetMaxBiomeCount();
    }
    public void ApplyBiomeSettings(List<string> p_biomes) {
        ClearBiomes();
        for (int i = 0; i < p_biomes.Count; i++) {
            string value = p_biomes[i];
            if (value == "Random") {
                BIOMES chosenBiome = CollectionUtilities.GetRandomElement(GameUtilities.customWorldBiomeChoices);
                AddBiome(chosenBiome);
            } else {
                string biomeStr = UtilityScripts.Utilities.NotNormalizedConversionStringToEnum(value).ToUpper();
                BIOMES chosenBiome = (BIOMES) System.Enum.Parse(typeof(BIOMES), biomeStr);
                AddBiome(chosenBiome);
            }
        }
    }
    public Vector2 GetMapSize() {
        switch (mapSize) {
            case MAP_SIZE.Small:
                return new Vector2(10, 8);
            case MAP_SIZE.Medium:
                return new Vector2(13, 10);
            case MAP_SIZE.Large:
                return new Vector2(16, 12);
            case MAP_SIZE.Extra_Large:
                return new Vector2(20, 14);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    public int GetMaxFactions() {
        switch (mapSize) {
            case MAP_SIZE.Small:
                return 1;
            case MAP_SIZE.Medium:
                return 1;
            case MAP_SIZE.Large:
                return 2;
            case MAP_SIZE.Extra_Large:
                return 3;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    public int GetMaxStartingVillages() {
        switch (mapSize) {
            case MAP_SIZE.Small:
                return 1;
            case MAP_SIZE.Medium:
                return 2;
            case MAP_SIZE.Large:
                return 4;
            case MAP_SIZE.Extra_Large:
                return 6;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    public int GetMaxVillagesForMapSize() {
        switch (mapSize) {
            case MAP_SIZE.Small:
                return 2;
            case MAP_SIZE.Medium:
                return 4;
            case MAP_SIZE.Large:
                return 6;
            case MAP_SIZE.Extra_Large:
                return 8;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    public int GetSpecialStructuresToCreate() {
        switch (mapSize) {
            case MAP_SIZE.Small:
                return UnityEngine.Random.Range(0, 5);
            case MAP_SIZE.Medium:
                return UnityEngine.Random.Range(1, 7);
            case MAP_SIZE.Large:
                return UnityEngine.Random.Range(2, 9);
            case MAP_SIZE.Extra_Large:
                return UnityEngine.Random.Range(3, 11);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    #region Monster Migrations
    public void AllowMonsterMigrations() {
        disableAllMonsterMigrations = false;
    }
    public void BlockMonsterMigrations() {
        disableAllMonsterMigrations = true;
    }
    #endregion
}