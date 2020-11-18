using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlagueLifespan {
    //Life span are always in hours
    //0 hour means indefinite
    //-1 means not applicable
    //IMPORTANT: If the lifespan returns -1, this means that the Plague must not be transmitted to that poi
    private int _tileObjectInfectionTimeInHours;
    private int _monsterInfectionTimeInHours;
    private int _undeadInfectionTimeInHours;
    private Dictionary<RACE, int> _sapientInfectionTimeInHours;

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

        SetTileObjectLifespanInHoursByLevel(1);
        SetMonsterInfectionTimeInHours(-1);
        SetUndeadInfectionTimeInHours(-1);
        SetSapientInfectionTimeInHours(RACE.HUMANS, 24);
        SetSapientInfectionTimeInHours(RACE.ELVES, 24);
    }

    #region Tile Object Lifespan
    public void SetTileObjectLifespanInHoursByLevel(int p_level) {
        SetTileObjectInfectionTimeInHours(GetTileObjectLifespanInHoursByLevel(p_level));
    }
    public void SetTileObjectInfectionTimeInHours(int p_hours) {
        _tileObjectInfectionTimeInHours = p_hours;
    }
    public void UpgradeTileObjectInfectionTime() {
        SetTileObjectInfectionTimeInHours(GetUpgradedTileObjectInfectionTime());
    }
    public int GetUpgradedTileObjectInfectionTime() {
        switch (_tileObjectInfectionTimeInHours) {
            case 3:
                return 6;
            case 6:
                return 12;
            case 12:
                return 24;
            default:
                Debug.LogError($"Could not upgrade Tile Object Infection Time: {_tileObjectInfectionTimeInHours.ToString()}");
                return -1;
        }
    }
    public int GetTileObjectInfectionTimeUpgradeCost() {
        switch (_tileObjectInfectionTimeInHours) {
            case 3:
                return 10;
            case 6:
                return 25;
            case 12:
                return 50;
            default:
                return -1;
        }
    }
    public bool IsTileObjectAtMaxLevel() {
        return _tileObjectInfectionTimeInHours == 24;
    }
    private int GetTileObjectLifespanInHoursByLevel(int p_level) {
        switch (p_level) {
            case 1: return 3;
            case 2: return 6;
            case 3: return 12;
            case 4: return 24;
            default: return 3;
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
        switch (_monsterInfectionTimeInHours) {
            case -1:
                return 12;
            case 12:
                return 24;
            case 24:
                return 72;
            default:
                Debug.LogError($"Could not upgrade Monster Infection Time: {_monsterInfectionTimeInHours.ToString()}");
                return -1;
        }
    }
    public int GetMonsterInfectionTimeUpgradeCost() {
        switch (_monsterInfectionTimeInHours) {
            case -1:
                return 10;
            case 12:
                return 20;
            case 24:
                return 30;
            default:
                return -1;
        }
    }
    public bool IsMonstersAtMaxLevel() {
        return _monsterInfectionTimeInHours == 72;
    }
    private int GetMonsterLifespanInHoursByLevel(int p_level) {
        switch (p_level) {
            case 1: return -1;
            case 2: return 12;
            case 3: return 24;
            case 4: return 72;
            default: return -1;
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
        switch (_undeadInfectionTimeInHours) {
            case -1:
                return 12;
            case 12:
                return 24;
            case 24:
                return 72;
            default:
                Debug.LogError($"Could not upgrade Undead Infection Time: {_undeadInfectionTimeInHours.ToString()}");
                return -1;
        }
    }
    public int GetUndeadInfectionTimeUpgradeCost() {
        switch (_undeadInfectionTimeInHours) {
            case -1:
                return 10;
            case 12:
                return 20;
            case 24:
                return 30;
            default:
                return -1;
        }
    }
    public bool IsUndeadAtMaxLevel() {
        return _undeadInfectionTimeInHours == 72;
    }
    private int GetUndeadLifespanInHoursByLevel(int p_level) {
        switch (p_level) {
            case 1: return -1;
            case 2: return 12;
            case 3: return 24;
            case 4: return 72;
            default: return -1;
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
        int currentDuration = GetSapientLifespanOfPlague(p_race);
        switch (currentDuration) {
            case 24:
                return 72;
            case 72:
                return 144;
            case 144:
                return 240;
            default:
                Debug.LogError($"Could not upgrade {p_race.ToString()} Sapient Infection Time: {currentDuration.ToString()}");
                return -1;
        }
    }
    public int GetSapientInfectionTimeUpgradeCost(RACE p_race) {
        int currentDuration = GetSapientLifespanOfPlague(p_race);
        switch (currentDuration) {
            case 24:
                return 10;
            case 72:
                return 25;
            case 144:
                return 50;
            default:
                return -1;
        }
    }
    public int GetSapientLifespanOfPlague(RACE p_race) {
        if (_sapientInfectionTimeInHours.ContainsKey(p_race)) {
            return _sapientInfectionTimeInHours[p_race];
        }
        return -1;
    }
    public bool IsSapientAtMaxLevel(RACE p_race) {
        return GetSapientLifespanOfPlague(p_race) == 240;
    }
    private int GetSapientLifespanInHoursByLevel(RACE p_race, int p_level) {
        //Note: Since Elves and Humans have the same lifespan right now, do not separate them
        switch (p_level) {
            case 1: return 24;
            case 2: return 72;
            case 3: return 144;
            case 4: return 240;
            default: return 24;
        }
    }
    #endregion

    public int GetLifespanInHoursOfPlagueOn(IPointOfInterest p_poi) {
        if(p_poi is Character character) {
            if(character.faction?.factionType.type == FACTION_TYPE.Wild_Monsters) {
                return monsterInfectionTimeInHours;
            } else if (character.faction?.factionType.type == FACTION_TYPE.Undead) {
                return undeadInfectionTimeInHours;
            } else {
                return GetSapientLifespanOfPlague(character.race);
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
        sapientInfectionTimeInHours = p_data.sapientInfectionTimeInHours;
    }
    public override PlagueLifespan Load() {
        PlagueLifespan component = new PlagueLifespan(this);
        return component;
    }
}