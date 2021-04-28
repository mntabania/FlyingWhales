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
        {CHANCE_TYPE.Base_Cult_Leader_Spawn_Chance, 25},
        {CHANCE_TYPE.Laziness_Nap_Level_2, 2},
        {CHANCE_TYPE.Laziness_Nap_Level_3, 4},
        {CHANCE_TYPE.Unfaithful_Active_Search_Affair, 20},
        {CHANCE_TYPE.Ignore_Urgent_Task, 35},
        {CHANCE_TYPE.Flirt_Acquaintance_Become_Lover_Chance, 25},
        {CHANCE_TYPE.Flirt_Acquaintance_Become_Affair_Chance, 35},
        {CHANCE_TYPE.Flirt_Friend_Become_Lover_Chance, 35},
        {CHANCE_TYPE.Flirt_Friend_Become_Affair_Chance, 50},
        {CHANCE_TYPE.Flirt_On_Sight_Base_Chance, 1},
        {CHANCE_TYPE.Vampire_Lord_Chance, 10},
        {CHANCE_TYPE.Host_Social_Party, 10},
        {CHANCE_TYPE.Demonic_Decor_On_Corrupt, 15},
        {CHANCE_TYPE.Retaliation, 25},
        {CHANCE_TYPE.Harpy_Capture, 15},
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
