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
        playerSkillSettings.SetCooldownSpeed(SKILL_COOLDOWN_SPEED.Normal);
        playerSkillSettings.SetManaCostAmount(SKILL_COST_AMOUNT.Normal);
        playerSkillSettings.SetChargeAmount(SKILL_CHARGE_AMOUNT.Normal);
        playerSkillSettings.SetRetaliationState(RETALIATION.Enabled);
        playerSkillSettings.SetOmnipotentMode(OMNIPOTENT_MODE.Disabled);
    }

    public bool AreSettingsValid(out string invalidityReason) {
        if (factionSettings.GetCurrentTotalVillageCountBasedOnFactions() > mapSettings.GetMaxStartingVillages()) {
            invalidityReason = $"{UtilityScripts.Utilities.NotNormalizedConversionEnumToString(mapSettings.mapSize.ToString())} maps can only have up to {mapSettings.GetMaxStartingVillages().ToString()} Village/s!";
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
        SetVictoryCondition(VICTORY_CONDITION.Summon_Ruinarch);
        SetDefaultSpellSettings();
        mapSettings.AllowMonsterMigrations();
        villageSettings.BlockAllFactionMigrations();
        villageSettings.BlockNewVillages();
        factionSettings.BlockNewFactions();
        villageSettings.SetBlessedMigrantsState(false);
        factionSettings.AllowFactionIdeologyChanges();
        playerSkillSettings.SetForcedArchetype(PLAYER_ARCHETYPE.Old_Puppet_Master, PLAYER_ARCHETYPE.Old_Lich, PLAYER_ARCHETYPE.Old_Ravager);
    }
    private void SetOonaWorldSettings() {
        Debug.Log("Set world settings as Second World");
        worldType = World_Type.Oona;
        SetVictoryCondition(VICTORY_CONDITION.Eliminate_All);
        SetDefaultSpellSettings();
        mapSettings.SetMapSize(MAP_SIZE.Small);
        mapSettings.AllowMonsterMigrations();
        villageSettings.AllowAllFactionMigrations();
        villageSettings.AllowNewVillages();
        factionSettings.AllowNewFactions();
        villageSettings.SetBlessedMigrantsState(false);
        factionSettings.AllowFactionIdeologyChanges();
        playerSkillSettings.SetForcedArchetype(PLAYER_ARCHETYPE.Old_Puppet_Master, PLAYER_ARCHETYPE.Old_Lich, PLAYER_ARCHETYPE.Old_Ravager);
    }
    private void SetIcalawaWorldSettings() {
        Debug.Log("Set world settings as Icalawa");
        worldType = World_Type.Icalawa;
        SetVictoryCondition(VICTORY_CONDITION.Eliminate_All);
        SetDefaultSpellSettings();
        mapSettings.SetMapSize(MAP_SIZE.Small);
        mapSettings.BlockMonsterMigrations();
        villageSettings.AllowAllFactionMigrations();
        villageSettings.AllowNewVillages();
        factionSettings.AllowNewFactions();
        villageSettings.SetBlessedMigrantsState(true);
        factionSettings.AllowFactionIdeologyChanges();
        playerSkillSettings.SetForcedArchetype(PLAYER_ARCHETYPE.Icalawa);
    }
    private void SetPangatLooWorldSettings() {
        Debug.Log("Set world settings as Pangat Loo");
        worldType = World_Type.Pangat_Loo;
        SetVictoryCondition(VICTORY_CONDITION.Wipe_Out_Village_On_Day);
        SetDefaultSpellSettings();
        mapSettings.SetMapSize(MAP_SIZE.Medium);
        mapSettings.AllowMonsterMigrations();
        villageSettings.AllowAllFactionMigrations();
        villageSettings.BlockNewVillages();
        factionSettings.BlockNewFactions();
        villageSettings.SetBlessedMigrantsState(false);
        factionSettings.AllowFactionIdeologyChanges();
        playerSkillSettings.SetForcedArchetype(PLAYER_ARCHETYPE.Old_Lich);
    }
    private void SetAffattWorldSettings() {
        Debug.Log("Set world settings as Affatt");
        worldType = World_Type.Affatt;
        SetVictoryCondition(VICTORY_CONDITION.Wipe_Elven_Kingdom_Survive_Humans);
        SetDefaultSpellSettings();
        villageSettings.SetMigrationSpeed(MIGRATION_SPEED.Slow);
        mapSettings.SetMapSize(MAP_SIZE.Large);
        mapSettings.AllowMonsterMigrations();
        villageSettings.AllowAllFactionMigrations();
        villageSettings.BlockVillagerMigrationForFactionType(FACTION_TYPE.Human_Empire);
        villageSettings.AllowNewVillages();
        factionSettings.BlockNewFactions();
        villageSettings.SetBlessedMigrantsState(false);
        factionSettings.BlockFactionIdeologyChanges();
        playerSkillSettings.SetForcedArchetype(PLAYER_ARCHETYPE.Affatt);
    }
    private void SetZenkoWorldSettings() {
        Debug.Log("Set world settings as Zenko");
        worldType = World_Type.Zenko;
        SetVictoryCondition(VICTORY_CONDITION.Eliminate_All);
        SetDefaultSpellSettings();
        mapSettings.SetMapSize(MAP_SIZE.Large);
        mapSettings.AllowMonsterMigrations();
        villageSettings.AllowAllFactionMigrations();
        villageSettings.BlockNewVillages();
        factionSettings.BlockNewFactions();
        villageSettings.SetBlessedMigrantsState(false);
        factionSettings.AllowFactionIdeologyChanges();
        playerSkillSettings.SetForcedArchetype(PLAYER_ARCHETYPE.Old_Puppet_Master, PLAYER_ARCHETYPE.Old_Lich, PLAYER_ARCHETYPE.Old_Ravager);
    }
    private void SetAneemWorldSettings() {
        Debug.Log("Set world settings as Aneem");
        worldType = World_Type.Aneem;
        SetVictoryCondition(VICTORY_CONDITION.Kill_By_Plague);
        SetDefaultSpellSettings();
        mapSettings.SetMapSize(MAP_SIZE.Large);
        mapSettings.AllowMonsterMigrations();
        villageSettings.AllowAllFactionMigrations();
        villageSettings.AllowNewVillages();
        factionSettings.AllowNewFactions();
        villageSettings.SetBlessedMigrantsState(false);
        factionSettings.AllowFactionIdeologyChanges();
        playerSkillSettings.SetForcedArchetype(PLAYER_ARCHETYPE.Old_Lich);
    }
    private void SetPittoWorldSettings() {
        Debug.Log("Set world settings as Pitto");
        worldType = World_Type.Pitto;
        SetVictoryCondition(VICTORY_CONDITION.Create_Demon_Cult);
        SetDefaultSpellSettings();
        mapSettings.AllowMonsterMigrations();
        villageSettings.AllowAllFactionMigrations();
        villageSettings.AllowNewVillages();
        factionSettings.AllowNewFactions();
        villageSettings.SetBlessedMigrantsState(false);
        factionSettings.AllowFactionIdeologyChanges();
        playerSkillSettings.SetForcedArchetype(PLAYER_ARCHETYPE.Old_Puppet_Master, PLAYER_ARCHETYPE.Old_Lich, PLAYER_ARCHETYPE.Old_Ravager);
    }
    public void ApplyCustomWorldSettings() {
        mapSettings.AllowMonsterMigrations();
        villageSettings.AllowAllFactionMigrations();
        villageSettings.AllowNewVillages();
        factionSettings.AllowNewFactions();
        villageSettings.SetBlessedMigrantsState(false);
        factionSettings.AllowFactionIdeologyChanges();
        playerSkillSettings.SetForcedArchetype(null);
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

    #region Retaliation
    public bool IsRetaliationAllowed() {
        return playerSkillSettings.retaliation == RETALIATION.Enabled;
    }
    #endregion
}