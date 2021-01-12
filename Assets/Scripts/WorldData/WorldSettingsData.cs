using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;

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
    
    // public bool omnipotentMode;
    // public bool noThreatMode;
    public List<BIOMES> biomes;
    public List<FactionSetting> factionSettings;
    
    public WorldSettingsData() {
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
    
    // public void SetOmnipotentMode(bool state) {
    //     omnipotentMode = state;
    // }
    // public void SetNoThreatMode(bool state) {
    //     noThreatMode = state;
    // }
    public void SetWorldType(World_Type type) {
        worldType = type;
    }
    public void SetMapSize(MAP_SIZE p_mapSize) {
        mapSize = p_mapSize;
    }
    private void SetDefaultSpellSettings() {
        SetMigrationSpeed(MIGRATION_SPEED.Normal);
        SetVictoryCondition(VICTORY_CONDITION.Eliminate_All);
        SetCooldownSpeed(SKILL_COOLDOWN_SPEED.Normal);
        SetManaCostAmount(SKILL_COST_AMOUNT.Normal);
        SetChargeAmount(SKILL_CHARGE_AMOUNT.Normal);
        SetThreatAmount(THREAT_AMOUNT.Normal);
    }
    
    #region Biomes
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

    public bool AreSettingsValid(out string invalidityReason) {
        if (GetCurrentTotalVillageCount() > GetMaxVillages()) {
            invalidityReason = $"{UtilityScripts.Utilities.NotNormalizedConversionEnumToString(mapSize.ToString())} maps can only have up to {GetMaxVillages().ToString()} Village/s!";
            return false;
        }
        invalidityReason = string.Empty;
        return true;
    }

    #region Scenario Maps
    public bool IsScenarioMap() {
        switch (worldType) {
            case World_Type.Custom:
                return false;
            default:
                return true;
        }
    }
    public void ApplySettingsBasedOnScenarioType(World_Type p_worldType) {
        Assert.IsFalse(p_worldType == World_Type.Custom);
        switch (p_worldType) {
            case World_Type.Tutorial:
                SetTutorialWorldSettings();
                break;
            case World_Type.Oona:
                SetOonaWorldSettings();
                break;
            case World_Type.Icalawa:
                SetIcalawaWorldSettings();
                break;
            case World_Type.Pangat_Loo:
                SetPangatLooWorldSettings();
                break;
            case World_Type.Affatt:
                SetAffattWorldSettings();
                break;
            case World_Type.Zenko:
                SetZenkoWorldSettings();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(p_worldType), p_worldType, null);
        }
    }
    private void SetTutorialWorldSettings() {
        Debug.Log("Set world settings as Tutorial");
        worldType = World_Type.Tutorial;
        // omnipotentMode = false;
        // noThreatMode = false;
        victoryCondition = VICTORY_CONDITION.Eliminate_All;
        SetDefaultSpellSettings();
    }
    private void SetOonaWorldSettings() {
        Debug.Log("Set world settings as Second World");
        worldType = World_Type.Oona;
        // omnipotentMode = false;
        // noThreatMode = false;
        victoryCondition = VICTORY_CONDITION.Eliminate_All;
        SetDefaultSpellSettings();
    }
    private void SetIcalawaWorldSettings() {
        Debug.Log("Set world settings as Icalawa");
        worldType = World_Type.Icalawa;
        // omnipotentMode = false;
        // noThreatMode = false;
        victoryCondition = VICTORY_CONDITION.Eliminate_All;
        SetDefaultSpellSettings();
    }
    private void SetPangatLooWorldSettings() {
        Debug.Log("Set world settings as Pangat Loo");
        worldType = World_Type.Pangat_Loo;
        // omnipotentMode = false;
        // noThreatMode = false;
        victoryCondition = VICTORY_CONDITION.Eliminate_All;
        SetDefaultSpellSettings();
    }
    private void SetAffattWorldSettings() {
        Debug.Log("Set world settings as Affatt");
        worldType = World_Type.Affatt;
        // omnipotentMode = false;
        // noThreatMode = false;
        victoryCondition = VICTORY_CONDITION.Eliminate_All;
        SetDefaultSpellSettings();
    }
    private void SetZenkoWorldSettings() {
        Debug.Log("Set world settings as Zenko");
        worldType = World_Type.Zenko;
        // omnipotentMode = false;
        // noThreatMode = false;
        victoryCondition = VICTORY_CONDITION.Eliminate_All;
        SetDefaultSpellSettings();
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
    public int GetCurrentTotalVillageCount() {
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

    #region Cooldown
    public void SetCooldownSpeed(SKILL_COOLDOWN_SPEED p_value) {
        cooldownSpeed = p_value;
        Debug.Log($"Set Cooldown Speed {p_value.ToString()}");
    }
    public float GetCooldownSpeedModification() {
        switch (cooldownSpeed) {
            case SKILL_COOLDOWN_SPEED.None:
                return 0f;
            case SKILL_COOLDOWN_SPEED.Half:
                return 0.5f;
            case SKILL_COOLDOWN_SPEED.Normal:
                return 1f;
            case SKILL_COOLDOWN_SPEED.Double:
                return 2f;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    #endregion

    #region Mana Costs
    public void SetManaCostAmount(SKILL_COST_AMOUNT p_value) {
        costAmount = p_value;
        Debug.Log($"Set Skill Cost {p_value.ToString()}");
    }
    public float GetCostsModification() {
        switch (costAmount) {
            case SKILL_COST_AMOUNT.None:
                return 0f;
            case SKILL_COST_AMOUNT.Half:
                return 0.5f;
            case SKILL_COST_AMOUNT.Normal:
                return 1f;
            case SKILL_COST_AMOUNT.Double:
                return 2f;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    #endregion

    #region Charges
    public void SetChargeAmount(SKILL_CHARGE_AMOUNT p_value) {
        chargeAmount = p_value;
        Debug.Log($"Set Charge Amount {p_value.ToString()}");
    }
    public float GetChargeCostsModification() {
        switch (chargeAmount) {
            case SKILL_CHARGE_AMOUNT.Unlimited:
            case SKILL_CHARGE_AMOUNT.Normal:
                return 1f;
            case SKILL_CHARGE_AMOUNT.Half:
                return 0.5f;
            case SKILL_CHARGE_AMOUNT.Double:
                return 2f;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    #endregion

    #region Threat
    public void SetThreatAmount(THREAT_AMOUNT p_value) {
        threatAmount = p_value;
        Debug.Log($"Set Threat Amount {p_value.ToString()}");
    }
    public float GetThreatModification() {
        switch (threatAmount) {
            case THREAT_AMOUNT.None:
                return 0f;
            case THREAT_AMOUNT.Half:
                return 0.5f;
            case THREAT_AMOUNT.Normal:
                return 1f;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    #endregion
}