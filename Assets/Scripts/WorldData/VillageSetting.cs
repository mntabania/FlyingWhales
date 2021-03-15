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
    
    public int GetTileCountReservedForVillage() {
        switch (villageSize) {
            case VILLAGE_SIZE.Small:
                return 2;
            case VILLAGE_SIZE.Medium:
                return 2;
            case VILLAGE_SIZE.Large:
                return 4;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    public int GetRandomDwellingCount() {
        switch (villageSize) {
            case VILLAGE_SIZE.Small:
                return UtilityScripts.Utilities.Rng.Next(4, 6);
            case VILLAGE_SIZE.Medium:
                return UtilityScripts.Utilities.Rng.Next(6, 8);
            case VILLAGE_SIZE.Large:
                return UtilityScripts.Utilities.Rng.Next(7, 9);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    public int GetRandomFacilityCount() {
        switch (villageSize) {
            case VILLAGE_SIZE.Small:
                return UtilityScripts.Utilities.Rng.Next(2, 4);
            case VILLAGE_SIZE.Medium:
                return UtilityScripts.Utilities.Rng.Next(3, 5);
            case VILLAGE_SIZE.Large:
                return UtilityScripts.Utilities.Rng.Next(3, 6);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}