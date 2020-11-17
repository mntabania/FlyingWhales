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

    #region Monster Lifespan
    public void SetMonsterLifespanInHoursByLevel(int p_level) {
        SetMonsterInfectionTimeInHours(GetMonsterLifespanInHoursByLevel(p_level));
    }
    public void SetMonsterInfectionTimeInHours(int p_hours) {
        _monsterInfectionTimeInHours = p_hours;
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

    #region Undead Lifespan
    public void SetUndeadLifespanInHoursByLevel(int p_level) {
        SetUndeadInfectionTimeInHours(GetUndeadLifespanInHoursByLevel(p_level));
    }
    public void SetUndeadInfectionTimeInHours(int p_hours) {
        _undeadInfectionTimeInHours = p_hours;
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

    #region Sapient Lifespan
    public void SetSapientLifespanInHoursByLevel(RACE p_race, int p_level) {
        SetSapientInfectionTimeInHours(p_race, GetSapientLifespanInHoursByLevel(p_race, p_level));
    }
    public void SetSapientInfectionTimeInHours(RACE p_race, int p_hours) {
        if (_sapientInfectionTimeInHours.ContainsKey(p_race)) {
            _sapientInfectionTimeInHours[p_race] = p_hours;
        } else {
            _sapientInfectionTimeInHours.Add(p_race, p_hours);
        }
    }
    private int GetSapientLifespanOfPlague(RACE p_race) {
        if (_sapientInfectionTimeInHours.ContainsKey(p_race)) {
            return _sapientInfectionTimeInHours[p_race];
        }
        return -1;
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
                return _monsterInfectionTimeInHours;
            } else if (character.faction?.factionType.type == FACTION_TYPE.Undead) {
                return _undeadInfectionTimeInHours;
            } else {
                return GetSapientLifespanOfPlague(character.race);
            }
        } else if (p_poi is TileObject tileObject) {
            return _tileObjectInfectionTimeInHours;
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