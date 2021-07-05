using System;

[System.Serializable]
public struct VillageSetting {
    public string villageName;
    public VILLAGE_SIZE villageSize;

    public static VillageSetting Default {
        get {
            return new VillageSetting() {
                villageName = RandomNameGenerator.GenerateSettlementName(RACE.HUMANS),
                villageSize = VILLAGE_SIZE.Small
            };
        }
    }
    
    public int GetTileCountReservedForVillage(MAP_SIZE p_mapSize) {
        switch (villageSize) {
            case VILLAGE_SIZE.Small:
                return 2;
            case VILLAGE_SIZE.Medium:
                if (p_mapSize == MAP_SIZE.Small) {
                    return 2;
                }
                return 3;
            case VILLAGE_SIZE.Large:
                if (p_mapSize == MAP_SIZE.Small) {
                    return 2;
                }
                return 3;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    public int GetRandomDwellingCount() {
        switch (villageSize) {
            case VILLAGE_SIZE.Small:
                return UtilityScripts.Utilities.Rng.Next(5, 7);
            case VILLAGE_SIZE.Medium:
                return UtilityScripts.Utilities.Rng.Next(9, 11);
            case VILLAGE_SIZE.Large:
                return UtilityScripts.Utilities.Rng.Next(13, 15);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    public int GetFacilityCount() {
        switch (villageSize) {
            case VILLAGE_SIZE.Small:
                return 2;
            case VILLAGE_SIZE.Medium:
                return 4;
            case VILLAGE_SIZE.Large:
                return 6;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    public int GetFoodProducingStructureCount() {
        switch (villageSize) {
            case VILLAGE_SIZE.Small:
                return 1;
            case VILLAGE_SIZE.Medium:
                return 2;
            case VILLAGE_SIZE.Large:
                return 2;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    public int GetBasicResourceProducingStructureCount() {
        switch (villageSize) {
            case VILLAGE_SIZE.Small:
                return 1;
            case VILLAGE_SIZE.Medium:
                return 1;
            case VILLAGE_SIZE.Large:
                return 1;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    public int GetSpecialStructureCount() {
        switch (villageSize) {
            case VILLAGE_SIZE.Small:
                return 0;
            case VILLAGE_SIZE.Medium:
                return 1;
            case VILLAGE_SIZE.Large:
                return 2;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}