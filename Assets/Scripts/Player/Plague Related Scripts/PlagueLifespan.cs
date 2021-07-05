using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

public class PlagueLifespan {
    //Life span are always in hours
    //0 hour means indefinite
    //-1 means not applicable
    //IMPORTANT: If the lifespan returns -1, this means that the Plague must not be transmitted to that poi
    private int _tileObjectInfectionTimeInHours;
    private int _monsterInfectionTimeInHours;
    private int _undeadInfectionTimeInHours;
    private Dictionary<RACE, int> _sapientInfectionTimeInHours;

    private const int MAX_LEVEL = 4;

    #region getters
    public int tileObjectInfectionTimeInHours => _tileObjectInfectionTimeInHours;
    public int monsterInfectionTimeInHours => _monsterInfectionTimeInHours;
    public int undeadInfectionTimeInHours => _undeadInfectionTimeInHours;
    public Dictionary<RACE, int> sapientInfectionTimeInHours => _sapientInfectionTimeInHours;
    #endregion

    public PlagueLifespan() {
        Initialize();
    }
    public PlagueLifespan(SaveDataPlagueLifespan p_data) {
        _tileObjectInfectionTimeInHours = p_data.tileObjectInfectionTimeInHours;
        _monsterInfectionTimeInHours = p_data.monsterInfectionTimeInHours;
        _undeadInfectionTimeInHours = p_data.undeadInfectionTimeInHours;
        _sapientInfectionTimeInHours = p_data.sapientInfectionTimeInHours;
    }

    private void Initialize() {
        //Default Data
        _sapientInfectionTimeInHours = new Dictionary<RACE, int>();

        SetTileObjectInfectionTimeInHours(24);
        SetMonsterInfectionTimeInHours(-1);
        SetUndeadInfectionTimeInHours(-1);
        SetSapientInfectionTimeInHours(RACE.HUMANS, 48);
        SetSapientInfectionTimeInHours(RACE.ELVES, 48);
    }

    #region Tile Object Lifespan
    public void SetTileObjectInfectionTimeInHours(int p_hours) {
        _tileObjectInfectionTimeInHours = p_hours;
    }
    public void UpgradeTileObjectInfectionTime() {
        SetTileObjectInfectionTimeInHours(GetUpgradedTileObjectInfectionTime());
    }
    public int GetUpgradedTileObjectInfectionTime() {
        int level = GetTileObjectLifespanLevelByHours(_tileObjectInfectionTimeInHours);
        switch (level) {
            case 1:
                return GetTileObjectLifespanInHoursByLevel(2);
            case 2:
                return GetTileObjectLifespanInHoursByLevel(3);
            case 3:
                return GetTileObjectLifespanInHoursByLevel(4);
            default:
                Debug.LogError($"Could not upgrade Tile Object Infection Time: {_tileObjectInfectionTimeInHours.ToString()}");
                return -1;
        }
    }
    public int GetTileObjectInfectionTimeUpgradeCost() {
        int level = GetTileObjectLifespanLevelByHours(_tileObjectInfectionTimeInHours);
        switch (level) {
            case 1:
                return SpellUtilities.GetModifiedSpellCost(10, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification());
            case 2:
                return SpellUtilities.GetModifiedSpellCost(25, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification());
            case 3:
                return SpellUtilities.GetModifiedSpellCost(50, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification());
            default:
                return -1;
        }
    }
    public bool IsTileObjectAtMaxLevel() {
        return _tileObjectInfectionTimeInHours == GetTileObjectLifespanInHoursByLevel(MAX_LEVEL);
    }
    private int GetTileObjectLifespanInHoursByLevel(int p_level) {
        switch (p_level) {
            case 1: return 24;
            case 2: return 48;
            case 3: return 72;
            case 4: return 96;
            default: return 24;
        }
    }
    private int GetTileObjectLifespanLevelByHours(int p_hours) {
        switch (p_hours) {
            case 24: return 1;
            case 48: return 2;
            case 72: return 3;
            case 96: return 4;
            default: return 1;
        }
    }
    #endregion

    #region Monsters
    public void SetMonsterInfectionTimeInHours(int p_hours) {
        _monsterInfectionTimeInHours = p_hours;
    }
    public void UpgradeMonsterInfectionTime() {
        SetMonsterInfectionTimeInHours(GetUpgradedMonsterInfectionTime());
    }
    public int GetUpgradedMonsterInfectionTime() {
        int level = GetMonsterLifespanLevelByHours(_monsterInfectionTimeInHours);
        switch (level) {
            case 1:
                return GetMonsterLifespanInHoursByLevel(2);
            case 2:
                return GetMonsterLifespanInHoursByLevel(3);
            case 3:
                return GetMonsterLifespanInHoursByLevel(4);
            default:
                Debug.LogError($"Could not upgrade Monster Infection Time: {_monsterInfectionTimeInHours.ToString()}");
                return -1;
        }
    }
    public int GetMonsterInfectionTimeUpgradeCost() {
        int level = GetMonsterLifespanLevelByHours(_monsterInfectionTimeInHours);
        switch (level) {
            case 1:
                return SpellUtilities.GetModifiedSpellCost(10, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification());
            case 2:
                return SpellUtilities.GetModifiedSpellCost(20, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification());
            case 3:
                return SpellUtilities.GetModifiedSpellCost(30, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification());
            default:
                return -1;
        }
    }
    public bool IsMonstersAtMaxLevel() {
        return _monsterInfectionTimeInHours == GetMonsterLifespanInHoursByLevel(MAX_LEVEL);
    }
    private int GetMonsterLifespanInHoursByLevel(int p_level) {
        switch (p_level) {
            case 1: return -1;
            case 2: return 24;
            case 3: return 72;
            case 4: return 120;
            default: return -1;
        }
    }
    private int GetMonsterLifespanLevelByHours(int p_hours) {
        switch (p_hours) {
            case -1: return 1;
            case 24: return 2;
            case 72: return 3;
            case 120: return 4;
            default: return 1;
        }
    }
    #endregion

    #region Undead
    public void SetUndeadInfectionTimeInHours(int p_hours) {
        _undeadInfectionTimeInHours = p_hours;
    }
    public void UpgradeUndeadInfectionTime() {
        SetUndeadInfectionTimeInHours(GetUpgradedUndeadInfectionTime());
    }
    public int GetUpgradedUndeadInfectionTime() {
        int level = GetUndeadLifespanLevelByHours(_undeadInfectionTimeInHours);
        switch (level) {
            case 1:
                return GetUndeadLifespanInHoursByLevel(2);
            case 2:
                return GetUndeadLifespanInHoursByLevel(3);
            case 3:
                return GetUndeadLifespanInHoursByLevel(4);
            default:
                Debug.LogError($"Could not upgrade Undead Infection Time: {_undeadInfectionTimeInHours.ToString()}");
                return -1;
        }
    }
    public int GetUndeadInfectionTimeUpgradeCost() {
        int level = GetUndeadLifespanLevelByHours(_undeadInfectionTimeInHours);
        switch (level) {
            case 1:
                return SpellUtilities.GetModifiedSpellCost(10, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification());
            case 2:
                return SpellUtilities.GetModifiedSpellCost(20, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification());
            case 3:
                return SpellUtilities.GetModifiedSpellCost(30, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification());
            default:
                return -1;
        }
    }
    public bool IsUndeadAtMaxLevel() {
        return _undeadInfectionTimeInHours == GetUndeadLifespanInHoursByLevel(MAX_LEVEL);
    }
    private int GetUndeadLifespanInHoursByLevel(int p_level) {
        switch (p_level) {
            case 1: return -1;
            case 2: return 24;
            case 3: return 72;
            case 4: return 120;
            default: return -1;
        }
    }
    private int GetUndeadLifespanLevelByHours(int p_hours) {
        switch (p_hours) {
            case -1: return 1;
            case 24: return 2;
            case 72: return 3;
            case 120: return 4;
            default: return 1;
        }
    }
    #endregion

    #region Sapients
    public void SetSapientInfectionTimeInHours(RACE p_race, int p_hours) {
        if (_sapientInfectionTimeInHours.ContainsKey(p_race)) {
            _sapientInfectionTimeInHours[p_race] = p_hours;
        } else {
            _sapientInfectionTimeInHours.Add(p_race, p_hours);
        }
    }
    public void UpgradeSapientInfectionTime(RACE p_race) {
        SetSapientInfectionTimeInHours(p_race, GetUpgradedSapientInfectionTime(p_race));
    }
    public int GetUpgradedSapientInfectionTime(RACE p_race) {
        int currentDuration = GetSapientLifespanOfPlagueInHours(p_race);
        int level = GetSapientLifespanLevelByHours(p_race, currentDuration);
        switch (level) {
            case 1:
                return GetSapientLifespanInHoursByLevel(p_race, 2);
            case 2:
                return GetSapientLifespanInHoursByLevel(p_race, 3);
            case 3:
                return GetSapientLifespanInHoursByLevel(p_race, 4);
            default:
                Debug.LogError($"Could not upgrade {p_race.ToString()} Sapient Infection Time: {currentDuration.ToString()}");
                return -1;
        }
    }
    public int GetSapientInfectionTimeUpgradeCost(RACE p_race) {
        int currentDuration = GetSapientLifespanOfPlagueInHours(p_race);
        int level = GetSapientLifespanLevelByHours(p_race, currentDuration);
        switch (level) {
            case 1:
                return SpellUtilities.GetModifiedSpellCost(20, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification());
            case 2:
                return SpellUtilities.GetModifiedSpellCost(40, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification());
            case 3:
                return SpellUtilities.GetModifiedSpellCost(60, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification());
            default:
                return -1;
        }
    }
    public int GetSapientLifespanOfPlagueInHours(RACE p_race) {
        if (_sapientInfectionTimeInHours.ContainsKey(p_race)) {
            return _sapientInfectionTimeInHours[p_race];
        }
        return -1;
    }
    public bool IsSapientAtMaxLevel(RACE p_race) {
        return GetSapientLifespanOfPlagueInHours(p_race) == GetSapientLifespanInHoursByLevel(p_race, MAX_LEVEL);
    }
    private int GetSapientLifespanInHoursByLevel(RACE p_race, int p_level) {
        //Note: Since Elves and Humans have the same lifespan right now, do not separate them
        switch (p_level) {
            case 1: return 48;
            case 2: return 96;
            case 3: return 144;
            case 4: return 192;
            default: return 48;
        }
    }
    private int GetSapientLifespanLevelByHours(RACE p_race, int p_hours) {
        //Note: Since Elves and Humans have the same lifespan right now, do not separate them
        switch (p_hours) {
            case 48: return 1;
            case 96: return 2;
            case 144: return 3;
            case 192: return 4;
            default: return 1;
        }
    }
    #endregion

    #region Utilities
    private int GetLifespanInHoursOfPlagueOn(IPointOfInterest p_poi) {
        if(p_poi is Character character) {
            if(character.faction?.factionType.type == FACTION_TYPE.Wild_Monsters) {
                return monsterInfectionTimeInHours;
            } else if (character.faction?.factionType.type == FACTION_TYPE.Undead) {
                return undeadInfectionTimeInHours;
            } else {
                if (character is Summon summon) {
                    if (summon.IsUndead()) {
                        return undeadInfectionTimeInHours;                        
                    } else {
                        return monsterInfectionTimeInHours;        
                    }
                } else {
                    return GetSapientLifespanOfPlagueInHours(character.race);    
                }
            }
        } else if (p_poi is TileObject tileObject) {
            return tileObjectInfectionTimeInHours;
        }
        return -1;
    }
    public int GetLifespanInTicksOfPlagueOn(IPointOfInterest p_poi) {
        int lifespanInHours = GetLifespanInHoursOfPlagueOn(p_poi);
        if(lifespanInHours != -1) {
            return GameManager.Instance.GetTicksBasedOnHour(lifespanInHours);
        }
        return -1;
    }
    public string GetInfectionTimeString(int timeInHours) {
        if (timeInHours == 0) {
            return "Indefinite";
        } else if (timeInHours == -1) {
            return "Immune";
        }
        else {
            return $"{timeInHours.ToString()} Hours";
        }
    }
    #endregion
}

[System.Serializable]
public class SaveDataPlagueLifespan : SaveData<PlagueLifespan> {
    public int tileObjectInfectionTimeInHours;
    public int monsterInfectionTimeInHours;
    public int undeadInfectionTimeInHours;
    public Dictionary<RACE, int> sapientInfectionTimeInHours;

    public override void Save(PlagueLifespan p_data) {
        tileObjectInfectionTimeInHours = p_data.tileObjectInfectionTimeInHours;
        monsterInfectionTimeInHours = p_data.monsterInfectionTimeInHours;
        undeadInfectionTimeInHours = p_data.undeadInfectionTimeInHours;
        sapientInfectionTimeInHours = new Dictionary<RACE, int>();
        foreach (var kvp in p_data.sapientInfectionTimeInHours) {
            sapientInfectionTimeInHours.Add(kvp.Key, kvp.Value);
        }
    }
    public override PlagueLifespan Load() {
        PlagueLifespan component = new PlagueLifespan(this);
        return component;
    }
}