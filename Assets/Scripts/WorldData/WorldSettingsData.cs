using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WorldSettingsData {

    public enum World_Type { Tutorial, Oona, Custom, Zenko, Pangat_Loo, Affatt, Icalawa };

    public World_Type worldType;
    public MAP_SIZE mapSize;
    public MIGRATION_SPEED migrationSpeed;
    public VICTORY_CONDITION victoryCondition;
    public SKILL_COOLDOWN_SPEED cooldownSpeed;
    public SKILL_COST_AMOUNT costAmount;
    public SKILL_CHARGE_AMOUNT chargeAmount;
    public THREAT_AMOUNT threatAmount;
    
    public bool omnipotentMode;
    public bool noThreatMode;
    public List<RACE> races;
    public List<BIOMES> biomes;
    public List<FactionSetting> factionSettings;
    
    public WorldSettingsData() {
        races = new List<RACE>();
        biomes = new List<BIOMES>();
        worldType = World_Type.Custom;
        mapSize = MAP_SIZE.Small;
        migrationSpeed = MIGRATION_SPEED.Normal;
        victoryCondition = VICTORY_CONDITION.Eliminate_All;
        cooldownSpeed = SKILL_COOLDOWN_SPEED.Normal;
        costAmount = SKILL_COST_AMOUNT.Normal;
        chargeAmount = SKILL_CHARGE_AMOUNT.Normal;
        threatAmount = THREAT_AMOUNT.Normal;
        factionSettings = new List<FactionSetting>();
    }
    
    public void SetOmnipotentMode(bool state) {
        omnipotentMode = state;
    }
    public void SetNoThreatMode(bool state) {
        noThreatMode = state;
    }
    public void SetWorldType(World_Type type) {
        worldType = type;
    }
    public void SetMapSize(MAP_SIZE p_mapSize) {
        mapSize = p_mapSize;
    }

    #region Race
    public void AddRace(RACE race) {
        if (!races.Contains(race)) {
            races.Add(race);
        }
    }
    public bool RemoveRace(RACE race) {
        return races.Remove(race);
    }
    public void ClearRaces() {
        races.Clear();
    }
    #endregion

    #region Biomes
    public void AddBiome(BIOMES biome) {
        Debug.Log($"Added {biome.ToString()} to biomes");
        biomes.Add(biome);
    }
    public bool RemoveBiome(BIOMES biome) {
        Debug.Log($"Removed {biome.ToString()} from biomes");
        return biomes.Remove(biome);
    }
    public void ClearBiomes() {
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
    #endregion

    public bool AreSettingsValid() {
        if (races.Count == 1) {
            //if only 1 race was toggled.
            //check that that races needed biome is also available
            RACE race = races[0];
            if (race == RACE.HUMANS) {
                return biomes.Contains(BIOMES.DESERT) || biomes.Contains(BIOMES.GRASSLAND);
            } else if (race == RACE.ELVES) {
                return biomes.Contains(BIOMES.FOREST) || biomes.Contains(BIOMES.SNOW);
            }
        }
        return races.Count >= 1 && biomes.Count >= 1;
    }

    #region Utilities
    public void SetTutorialWorldSettings() {
        Debug.Log("Set world settings as Tutorial");
        worldType = World_Type.Tutorial;
        omnipotentMode = false;
        noThreatMode = false;
        ClearBiomes();
        ClearRaces();
        AddRace(RACE.HUMANS);
        AddBiome(BIOMES.GRASSLAND);
    }
    public void SetSecondWorldSettings() {
        Debug.Log("Set world settings as Second World");
        worldType = World_Type.Oona;
        omnipotentMode = false;
        noThreatMode = false;
        ClearBiomes();
        ClearRaces();
        AddRace(RACE.HUMANS);
        AddBiome(BIOMES.DESERT);
        //DisableSpellForWorld(SPELL_TYPE.EARTHQUAKE);
    }
    public void SetDefaultCustomWorldSettings() {
        Debug.Log("Set world settings as Default Custom");
        worldType = World_Type.Custom;
        omnipotentMode = false;
        noThreatMode = false;
        ClearBiomes();
        ClearRaces();
        AddRace(RACE.HUMANS);
        AddRace(RACE.ELVES);
        AddBiome(BIOMES.DESERT);
        AddBiome(BIOMES.GRASSLAND);
        AddBiome(BIOMES.SNOW);
        AddBiome(BIOMES.FOREST);
    }
    public void SetZenkoWorldSettings() {
        Debug.Log("Set world settings as Zenko");
        worldType = World_Type.Zenko;
        omnipotentMode = false;
        noThreatMode = false;
        ClearBiomes();
        ClearRaces();
        AddRace(RACE.HUMANS);
        AddRace(RACE.ELVES);
        AddBiome(BIOMES.SNOW);
        AddBiome(BIOMES.GRASSLAND);
        AddBiome(BIOMES.FOREST);
        AddBiome(BIOMES.DESERT);
    }
    public void SetPangatLooWorldSettings() {
        Debug.Log("Set world settings as Pangat Loo");
        worldType = World_Type.Pangat_Loo;
        omnipotentMode = false;
        noThreatMode = false;
        ClearBiomes();
        ClearRaces();
        AddRace(RACE.HUMANS); ;
        AddBiome(BIOMES.GRASSLAND);
        AddBiome(BIOMES.DESERT);
    }
    public void SetAffattWorldSettings() {
        Debug.Log("Set world settings as Affatt");
        worldType = World_Type.Affatt;
        omnipotentMode = false;
        noThreatMode = false;
        ClearBiomes();
        ClearRaces();
        AddRace(RACE.HUMANS);
        AddRace(RACE.ELVES);
        AddBiome(BIOMES.SNOW);
        AddBiome(BIOMES.FOREST);
    }
    public void SetIcalawaWorldSettings() {
        Debug.Log("Set world settings as Icalawa");
        worldType = World_Type.Icalawa;
        omnipotentMode = false;
        noThreatMode = false;
        ClearBiomes();
        ClearRaces();
        AddRace(RACE.ELVES);
        AddBiome(BIOMES.SNOW);
    }
    #endregion

    #region Scenario Maps
    public bool IsScenarioMap() {
        switch (worldType) {
            case World_Type.Custom:
                return false;
            default:
                return true;
        }
    }
    #endregion

    #region Map Size
    public Vector2 GetMapSize() {
        switch (mapSize) {
            case MAP_SIZE.Small:
                return new Vector2(8, 8);
            case MAP_SIZE.Medium:
                return new Vector2(12, 8);
            case MAP_SIZE.Large:
                return new Vector2(16, 10);
            case MAP_SIZE.Extra_Large:
                return new Vector2(16, 16);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    public int GetVillagesToCreate() {
        switch (mapSize) {
            case MAP_SIZE.Small:
                return 1;
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
    public int GetTilesInBetweenVillages() {
        switch (mapSize) {
            case MAP_SIZE.Small:
                return 3;
            case MAP_SIZE.Medium:
                return 3;
            case MAP_SIZE.Large:
                return 4;
            case MAP_SIZE.Extra_Large:
                return 4;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    #endregion

    #region Faction Settings
    public int GetMaxFactions() {
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
    public int GetMaxVillages() {
        switch (mapSize) {
            case MAP_SIZE.Small:
                return 1;
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
    public int GetCurrentVillageCount() {
        int villageCount = 0;
        for (int i = 0; i < factionSettings.Count; i++) {
            villageCount += factionSettings[i].villageSettings.Count;
        }
        return villageCount;
    }
    public FactionSetting AddFactionSetting(int p_villageCount) {
        FactionSetting factionSetting = new FactionSetting(p_villageCount);
        AddFactionSetting(factionSetting);
        return factionSetting;
    }
    public void AddFactionSetting(FactionSetting p_factionSetting) {
        factionSettings.Add(p_factionSetting);
    }
    public void RemoveFactionSetting(FactionSetting p_factionSetting) {
        factionSettings.Remove(p_factionSetting);
    }
    public bool HasReachedMaxFactionCount() {
        return factionSettings.Count >= GetMaxFactions();
    }
    public void ClearFactionSettings() {
        factionSettings.Clear();
    }
    #endregion

    public void SetMigrationSpeed(MIGRATION_SPEED p_value) {
        migrationSpeed = p_value;
        Debug.Log($"Set Migration Speed {p_value.ToString()}");
    }
    public void SetVictoryCondition(VICTORY_CONDITION p_value) {
        victoryCondition = p_value;
        Debug.Log($"Set Victory Condition {p_value.ToString()}");
    }
    public void SetCooldownSpeed(SKILL_COOLDOWN_SPEED p_value) {
        cooldownSpeed = p_value;
        Debug.Log($"Set Cooldown Speed {p_value.ToString()}");
    }
    public void SetSkillCostAmount(SKILL_COST_AMOUNT p_value) {
        costAmount = p_value;
        Debug.Log($"Set Skill Cost {p_value.ToString()}");
    }
    public void SetChargeAmount(SKILL_CHARGE_AMOUNT p_value) {
        chargeAmount = p_value;
        Debug.Log($"Set Charge Amount {p_value.ToString()}");
    }
    public void SetThreatAmount(THREAT_AMOUNT p_value) {
        threatAmount = p_value;
        Debug.Log($"Set Threat Amount {p_value.ToString()}");
    }
}