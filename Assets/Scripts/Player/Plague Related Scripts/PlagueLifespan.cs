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
    #endregion
    
    public PlagueLifespan() {
        Initialize();
    }
    
    private void Initialize() {
        //Default Data
        _sapientInfectionTimeInHours = new Dictionary<RACE, int>();

        SetTileObjectLifespanInHoursByLevel(1);
        SetMonsterInfectionTimeInHours(1);
        SetUndeadInfectionTimeInHours(1);
        SetSapientInfectionTimeInHours(RACE.HUMANS, 1);
        SetSapientInfectionTimeInHours(RACE.ELVES, 1);
    }

    #region Tile Object Lifespan
    public void SetTileObjectLifespanInHoursByLevel(int p_level) {
        SetTileObjectInfectionTimeInHours(GetTileObjectLifespanInHoursByLevel(p_level));
    }
    public void SetTileObjectInfectionTimeInHours(int p_hours) {
        _tileObjectInfectionTimeInHours = p_hours;
    }
    public void UpgradeTileObjectInfectionTime() {
        switch (_tileObjectInfectionTimeInHours) {
            case 3:
                SetTileObjectInfectionTimeInHours(6);
                break;
            case 6:
                SetTileObjectInfectionTimeInHours(12);
                break;
            case 12:
                SetTileObjectInfectionTimeInHours(24);
                break;
            default:
                Debug.LogError($"Could not upgrade Tile Object Infection Time: {_tileObjectInfectionTimeInHours.ToString()}");
                break;
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
        switch (_monsterInfectionTimeInHours) {
            case -1:
                SetMonsterInfectionTimeInHours(12);
                break;
            case 12:
                SetMonsterInfectionTimeInHours(24);
                break;
            case 24:
                SetMonsterInfectionTimeInHours(72);
                break;
            default:
                Debug.LogError($"Could not upgrade Monster Infection Time: {_monsterInfectionTimeInHours.ToString()}");
                break;
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
        switch (_undeadInfectionTimeInHours) {
            case -1:
                SetUndeadInfectionTimeInHours(12);
                break;
            case 12:
                SetUndeadInfectionTimeInHours(24);
                break;
            case 24:
                SetUndeadInfectionTimeInHours(72);
                break;
            default:
                Debug.LogError($"Could not upgrade Undead Infection Time: {_undeadInfectionTimeInHours.ToString()}");
                break;
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
        int currentDuration = GetSapientLifespanOfPlague(p_race);
        switch (currentDuration) {
            case 24:
                SetSapientInfectionTimeInHours(p_race, 72);
                break;
            case 72:
                SetSapientInfectionTimeInHours(p_race, 144);
                break;
            case 144:
                SetSapientInfectionTimeInHours(p_race, 240);
                break;
            default:
                Debug.LogError($"Could not upgrade {p_race.ToString()} Sapient Infection Time: {currentDuration.ToString()}");
                break;
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