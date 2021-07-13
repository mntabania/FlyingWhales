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
        {CHANCE_TYPE.Host_Social_Party, 5},
        {CHANCE_TYPE.Demonic_Decor_On_Corrupt, 15},
        {CHANCE_TYPE.Retaliation_Character_Death, 35},
        {CHANCE_TYPE.Retaliation_Structure_Destroy, 30},
        {CHANCE_TYPE.Retaliation_Resource_Pile, 25},
        {CHANCE_TYPE.Harpy_Capture, 15},
        {CHANCE_TYPE.Lycanthrope_Transform_Chance, 25},
        {CHANCE_TYPE.Visit_Friend, 25},
        {CHANCE_TYPE.Ent_Spawn, 3},
        {CHANCE_TYPE.Mimic_Spawn, 5},
        {CHANCE_TYPE.Vampire_Lycan_Visit_Hospice, 5},
        {CHANCE_TYPE.Settlement_Ruler_Default_Facility_Chance, 2},
        {CHANCE_TYPE.Kidnap_Chance, 35},
        {CHANCE_TYPE.Raid_Chance, 25},
        //{CHANCE_TYPE.Rescue_Chance, 50},
        {CHANCE_TYPE.Find_Fish, 5},
        {CHANCE_TYPE.Party_Quest_First_Knockout, 25},
        {CHANCE_TYPE.Change_Intent, 15},
        {CHANCE_TYPE.Change_Intent_Kleptomania, 25},
        {CHANCE_TYPE.Change_Intent_Vampire, 25},
        {CHANCE_TYPE.Change_Intent_Cultist, 25},
        {CHANCE_TYPE.Plauged_Injured_Visit_Hospice, 15},
        //{CHANCE_TYPE.Hunt_Chance, 50},
        {CHANCE_TYPE.Free_Time_Obtain_Want, 20},
        {CHANCE_TYPE.Do_Work_Chance, 85},
        {CHANCE_TYPE.Monster_Migration, 15},
        {CHANCE_TYPE.Socialize_Chance, 20},
        {CHANCE_TYPE.Visit_Village_Chance, 10},
        {CHANCE_TYPE.Create_Change_Class_Combatant, 35},
        {CHANCE_TYPE.Personal_Combatant_Change_Class, 5},
        {CHANCE_TYPE.Kobold_Place_Freezing_Trap, 10},
        {CHANCE_TYPE.Base_Create_Faction_Chance, 2},
        {CHANCE_TYPE.Vagrant_Join_Or_Create_Faction, 15},
        {CHANCE_TYPE.Vampire_Hunt_Drink_Blood_Chance, 15},
        {CHANCE_TYPE.Werewolf_Hunt_On_See_Werewolf, 25},
        {CHANCE_TYPE.Werewolf_Hunt_Mangled, 25},
        {CHANCE_TYPE.Plagued_Event_Lethargic, 5},
        {CHANCE_TYPE.Plagued_Event_Paralyzed, 15},
        {CHANCE_TYPE.Plagued_Event_Heart_Attck, 15},
        {CHANCE_TYPE.Plagued_Event_Pneumonia, 15},
        {CHANCE_TYPE.Plagued_Event_Puke, 5},
        {CHANCE_TYPE.Plagued_Event_Seizure, 5},
        {CHANCE_TYPE.Plagued_Event_Septic_Shock, 15},
        {CHANCE_TYPE.Plagued_Event_Sneeze, 5},
        {CHANCE_TYPE.Plagued_Event_Stroke, 15},
        {CHANCE_TYPE.Plagued_Event_Organ_Failure, 15},
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
