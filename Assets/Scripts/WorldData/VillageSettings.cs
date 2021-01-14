using System.Collections.Generic;
using UnityEngine;

public class VillageSettings {
    public List<FACTION_TYPE> disabledFactionMigrations;
    public MIGRATION_SPEED migrationSpeed;
    public bool disableAllVillagerMigrations;
    public bool disableNewVillages;
    public bool disableAllMonsterMigrations;
    public bool blessedMigrants;
    
    public VillageSettings() {
        migrationSpeed = MIGRATION_SPEED.Normal;
        disabledFactionMigrations = new List<FACTION_TYPE>();
    }
    public void SetMigrationSpeed(MIGRATION_SPEED p_value) {
        migrationSpeed = p_value;
        Debug.Log($"Set Migration Speed {p_value.ToString()}");
    }
    public void EnableAllVillagerMigrations() {
        disableAllVillagerMigrations = false;
    }
    public void DisableAllVillagerMigrations() {
        disableAllVillagerMigrations = true;
    }
    public void EnableVillagerMigrationForFactionType(FACTION_TYPE p_factionType) {
        disabledFactionMigrations.Remove(p_factionType);
    }
    public void DisableVillagerMigrationForFactionType(FACTION_TYPE p_factionType) {
        disabledFactionMigrations.Add(p_factionType);
    }
    public void EnableVillagerMigrationForAllFactionTypes() {
        disabledFactionMigrations.Clear();
    }
    public bool IsMigrationAllowedForFaction(FACTION_TYPE p_factionType) {
        return !disabledFactionMigrations.Contains(p_factionType);
    }
    public void EnableAllFactionMigrations() {
        disabledFactionMigrations.Clear();
    }
    public void AllowNewVillages() {
        disableNewVillages = false;
    }
    public void BlockNewVillages() {
        disableNewVillages = true;
    }
    public void SetBlessedMigrantsState(bool p_state) {
        blessedMigrants = p_state;
    }
}