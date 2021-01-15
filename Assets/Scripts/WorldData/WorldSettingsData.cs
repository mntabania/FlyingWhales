using System;
using UnityEngine;
using UnityEngine.Assertions;

[System.Serializable]
public class WorldSettingsData {

    public enum World_Type { Tutorial, Oona, Custom, Zenko, Pangat_Loo, Affatt, Icalawa, Aneem, Pitto };

    public World_Type worldType;
    public VICTORY_CONDITION victoryCondition;
    public PlayerSkillSettings playerSkillSettings;
    public MapSettings mapSettings;
    public VillageSettings villageSettings;
    public FactionSettings factionSettings;
    
    public WorldSettingsData() {
        worldType = World_Type.Custom;
        victoryCondition = VICTORY_CONDITION.Eliminate_All;
        playerSkillSettings = new PlayerSkillSettings();
        mapSettings = new MapSettings();
        villageSettings = new VillageSettings();
        factionSettings = new FactionSettings();
    }

    #region World Type
    public void SetWorldType(World_Type type) {
        worldType = type;
    }
    #endregion

    #region Utilities
    private void SetDefaultSpellSettings() {
        villageSettings.SetMigrationSpeed(MIGRATION_SPEED.Normal);
        SetVictoryCondition(VICTORY_CONDITION.Eliminate_All);
        playerSkillSettings.SetCooldownSpeed(SKILL_COOLDOWN_SPEED.Normal);
        playerSkillSettings.SetManaCostAmount(SKILL_COST_AMOUNT.Normal);
        playerSkillSettings.SetChargeAmount(SKILL_CHARGE_AMOUNT.Normal);
        playerSkillSettings.SetThreatAmount(THREAT_AMOUNT.Normal);
    }

    public bool AreSettingsValid(out string invalidityReason) {
        if (factionSettings.GetCurrentTotalVillageCountBasedOnFactions() > mapSettings.GetMaxVillages()) {
            invalidityReason = $"{UtilityScripts.Utilities.NotNormalizedConversionEnumToString(mapSettings.mapSize.ToString())} maps can only have up to {mapSettings.GetMaxVillages().ToString()} Village/s!";
            return false;
        }
        invalidityReason = string.Empty;
        return true;
    }
    #endregion

    #region Maps
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
            case World_Type.Aneem:
                SetAneemWorldSettings();
                break;
            case World_Type.Pitto:
                SetPittoWorldSettings();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(p_worldType), p_worldType, null);
        }
    }
    private void SetTutorialWorldSettings() {
        Debug.Log("Set world settings as Tutorial");
        worldType = World_Type.Tutorial;
        victoryCondition = VICTORY_CONDITION.Eliminate_All;
        SetDefaultSpellSettings();
        villageSettings.EnableAllVillagerMigrations();
        villageSettings.EnableAllFactionMigrations();
        villageSettings.AllowNewVillages();
        factionSettings.AllowNewFactions();
        villageSettings.SetBlessedMigrantsState(false);
        factionSettings.AllowFactionIdeologyChanges();
        playerSkillSettings.SetForcedArchetype(PLAYER_ARCHETYPE.Normal);
    }
    private void SetOonaWorldSettings() {
        Debug.Log("Set world settings as Second World");
        worldType = World_Type.Oona;
        victoryCondition = VICTORY_CONDITION.Eliminate_All;
        SetDefaultSpellSettings();
        villageSettings.EnableAllVillagerMigrations();
        villageSettings.EnableAllFactionMigrations();
        villageSettings.AllowNewVillages();
        factionSettings.AllowNewFactions();
        villageSettings.SetBlessedMigrantsState(false);
        factionSettings.AllowFactionIdeologyChanges();
        playerSkillSettings.SetForcedArchetype(PLAYER_ARCHETYPE.Normal);
    }
    private void SetIcalawaWorldSettings() {
        Debug.Log("Set world settings as Icalawa");
        worldType = World_Type.Icalawa;
        victoryCondition = VICTORY_CONDITION.Eliminate_All;
        SetDefaultSpellSettings();
        villageSettings.EnableAllVillagerMigrations();
        villageSettings.EnableAllFactionMigrations();
        villageSettings.AllowNewVillages();
        factionSettings.AllowNewFactions();
        villageSettings.SetBlessedMigrantsState(false);
        factionSettings.AllowFactionIdeologyChanges();
        playerSkillSettings.SetForcedArchetype(PLAYER_ARCHETYPE.Puppet_Master);
    }
    private void SetPangatLooWorldSettings() {
        Debug.Log("Set world settings as Pangat Loo");
        worldType = World_Type.Pangat_Loo;
        victoryCondition = VICTORY_CONDITION.Eliminate_All;
        SetDefaultSpellSettings();
        villageSettings.EnableAllVillagerMigrations();
        villageSettings.EnableAllFactionMigrations();
        villageSettings.AllowNewVillages();
        factionSettings.AllowNewFactions();
        villageSettings.SetBlessedMigrantsState(false);
        factionSettings.AllowFactionIdeologyChanges();
        playerSkillSettings.SetForcedArchetype(PLAYER_ARCHETYPE.Lich);
    }
    private void SetAffattWorldSettings() {
        Debug.Log("Set world settings as Affatt");
        worldType = World_Type.Affatt;
        victoryCondition = VICTORY_CONDITION.Eliminate_All;
        SetDefaultSpellSettings();
        villageSettings.EnableAllVillagerMigrations();
        villageSettings.EnableAllFactionMigrations();
        villageSettings.AllowNewVillages();
        factionSettings.AllowNewFactions();
        villageSettings.SetBlessedMigrantsState(false);
        factionSettings.AllowFactionIdeologyChanges();
        playerSkillSettings.SetForcedArchetype(PLAYER_ARCHETYPE.Normal);
    }
    private void SetZenkoWorldSettings() {
        Debug.Log("Set world settings as Zenko");
        worldType = World_Type.Zenko;
        victoryCondition = VICTORY_CONDITION.Eliminate_All;
        SetDefaultSpellSettings();
        villageSettings.EnableAllVillagerMigrations();
        villageSettings.EnableAllFactionMigrations();
        villageSettings.AllowNewVillages();
        factionSettings.AllowNewFactions();
        villageSettings.SetBlessedMigrantsState(false);
        factionSettings.AllowFactionIdeologyChanges();
        playerSkillSettings.SetForcedArchetype(PLAYER_ARCHETYPE.Normal);
    }
    private void SetAneemWorldSettings() {
        Debug.Log("Set world settings as Aneem");
        worldType = World_Type.Aneem;
        victoryCondition = VICTORY_CONDITION.Eliminate_All;
        SetDefaultSpellSettings();
        villageSettings.EnableAllVillagerMigrations();
        villageSettings.EnableAllFactionMigrations();
        villageSettings.AllowNewVillages();
        factionSettings.AllowNewFactions();
        villageSettings.SetBlessedMigrantsState(false);
        factionSettings.AllowFactionIdeologyChanges();
        playerSkillSettings.SetForcedArchetype(PLAYER_ARCHETYPE.Lich);
    }
    private void SetPittoWorldSettings() {
        Debug.Log("Set world settings as Pitto");
        worldType = World_Type.Pitto;
        victoryCondition = VICTORY_CONDITION.Eliminate_All;
        SetDefaultSpellSettings();
        villageSettings.EnableAllVillagerMigrations();
        villageSettings.EnableAllFactionMigrations();
        villageSettings.AllowNewVillages();
        factionSettings.AllowNewFactions();
        villageSettings.SetBlessedMigrantsState(false);
        factionSettings.AllowFactionIdeologyChanges();
        playerSkillSettings.SetForcedArchetype(PLAYER_ARCHETYPE.Normal);
    }
    public void ApplyCustomWorldSettings() {
        villageSettings.EnableAllVillagerMigrations();
        villageSettings.EnableAllFactionMigrations();
        villageSettings.AllowNewVillages();
        factionSettings.AllowNewFactions();
        villageSettings.SetBlessedMigrantsState(false);
        factionSettings.AllowFactionIdeologyChanges();
        playerSkillSettings.SetForcedArchetype(PLAYER_ARCHETYPE.Normal);
    }
    #endregion

    #region Factions
    public bool HasReachedMaxFactionCount() {
        return factionSettings.factionTemplates.Count >= mapSettings.GetMaxFactions();
    }
    #endregion

    #region Win Condition
    public void SetVictoryCondition(VICTORY_CONDITION p_value) {
        victoryCondition = p_value;
        Debug.Log($"Set Victory Condition {p_value.ToString()}");
    }
    #endregion
    
}