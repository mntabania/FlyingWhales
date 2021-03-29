using System;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

public static class ChanceData {
    public static Dictionary<CHANCE_TYPE, int> integerChances = new Dictionary<CHANCE_TYPE, int>(EnumComparer<CHANCE_TYPE>.Instance) {
        {CHANCE_TYPE.Kleptomania_Pickpocket_Level_1, 5},
        {CHANCE_TYPE.Kleptomania_Pickpocket_Level_2, 10},
        {CHANCE_TYPE.Kleptomania_Rob_Other_House, 5},
        {CHANCE_TYPE.Kleptomania_Rob_Any_Place, 5},
    };

    public static bool RollChance(CHANCE_TYPE p_chanceType) {
        if (integerChances.ContainsKey(p_chanceType)) {
            return GameUtilities.RollChance(integerChances[p_chanceType]);
        }
        throw new Exception($"No chance set for {p_chanceType}");
    }
    public static bool RollChance(CHANCE_TYPE p_chanceType, ref string log) {
        if (integerChances.ContainsKey(p_chanceType)) {
            return GameUtilities.RollChance(integerChances[p_chanceType], ref log);
        }
        throw new Exception($"No chance set for {p_chanceType}");
    }
    public static int GetChance(CHANCE_TYPE p_chanceType) {
        if (integerChances.ContainsKey(p_chanceType)) {
            return integerChances[p_chanceType];
        }
        throw new Exception($"No chance set for {p_chanceType}");
    }
    public static void SetChance(CHANCE_TYPE p_chanceType, int p_value) {
        if (integerChances.ContainsKey(p_chanceType)) {
            integerChances[p_chanceType] = p_value;
        }
    }
}
