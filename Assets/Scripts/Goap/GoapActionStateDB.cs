﻿using System.Collections.Generic;

public static class GoapActionStateDB {

    public static string No_Icon = "None";
    public static string Eat_Icon = "Eat";
    public static string Hostile_Icon = "Hostile";
    public static string Sleep_Icon = "Sleep";
    public static string Social_Icon = "Social";
    public static string Work_Icon = "Work";
    public static string Drink_Icon = "Drink";
    public static string Entertain_Icon = "Entertain";
    public static string Explore_Icon = "Explore";
    public static string FirstAid_Icon = "First Aid";
    public static string Flee_Icon = "Flee";
    public static string Patrol_Icon = "Patrol";
    public static string Watch_Icon = "Watch";
    public static string Anger_Icon = "Anger";
    public static string Approval_Icon = "Approval";
    public static string Build_Icon = "Build";
    public static string Bury_Icon = "Bury";
    public static string Chop_Icon = "Chop";
    public static string Clean_Icon = "Clean";
    public static string Cowering_Icon = "Cowering";
    public static string Cure_Icon = "Cure";
    public static string Douse_Icon = "Douse";
    public static string Harvest_Icon = "Harvest";
    public static string Haul_Icon = "Haul";
    public static string Magic_Icon = "Magic";
    public static string Mine_Icon = "Mine";
    public static string Mock_Icon = "Mock";
    public static string Repair_Icon = "Repair";
    public static string Sad_Icon = "Sad";
    public static string Shock_Icon = "Shock";
    public static string Sick_Icon = "Sick";
    public static string Drink_Blood_Icon = "Drink Blood";
    public static string Flirt_Icon = "Flirt";
    public static string Pray_Icon = "Pray";
    public static string Restrain_Icon = "Restrain";
    public static string Steal_Icon = "Steal";
    public static string Stealth_Icon = "Stealth";
    public static string Joy_Icon = "Joy";
    public static string Fish_Icon = "Fish";
    public static string Happy_Icon = "Happy";
    public static string Inspect_Icon = "Inspect";
    public static string Party_Icon = "Party";
    public static string Heartbroken_Icon = "Heartbroken";
    public static string Injured_Icon = "Injured";
    public static string Gossip_Icon = "Gossip";
    public static string Blueprint_Icon = "Blueprint";
    public static string Cult_Icon = "Cult";
    public static string Butcher_Icon = "Butcher";
    public static string Trap_Icon = "Trap";
    public static string Lycan_Icon = "Lycan";
    public static string Poison_Icon = "Poison";
    public static string Report_Icon = "Report";
    public static string Found_Icon = "Found";
    public static string Divine_Icon = "Divine";
    public static string Burn_Icon = "Burn";
    public static string Train_Icon = "Train";
    public static string Vampire_Turn_Icon = "Vampire Turn";
    public static string Change_Icon = "Change";
    public static string Agree_Icon = "Agree";
    public static string Abduct_Icon = "Abduct";
    public static string Argue_Icon = "Argue";
    public static string Death_Icon = "Death";
    public static string Question_Icon = "Question";
    public static string Daydream_Icon = "Daydream";
    public static string Judge_Icon = "Judge";
    public static string Read_Icon = "Read";
    public static string Sing_Icon = "Sing";


    public static string GetStateResult(INTERACTION_TYPE goapType, string stateName) {
        if (goapActionStates.ContainsKey(goapType)) {
            StateNameAndDuration[] snd = goapActionStates[goapType];
            for (int i = 0; i < snd.Length; i++) {
                StateNameAndDuration currSND = snd[i];
                if (currSND.name == stateName) {
                    return currSND.status;
                }
            }
        }
        return string.Empty;
    }

    public static readonly Dictionary<INTERACTION_TYPE, StateNameAndDuration[]> goapActionStates = new Dictionary<INTERACTION_TYPE, StateNameAndDuration[]>() {
        {INTERACTION_TYPE.EAT, new[]{
            new StateNameAndDuration(){ name = "Eat Success", status = InteractionManager.Goap_State_Success, duration = 10 },
        } },
        {INTERACTION_TYPE.RELEASE_CHARACTER, new[]{
            new StateNameAndDuration(){ name = "Release Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.ASSAULT, new[]{
            new StateNameAndDuration(){ name = "Combat Start", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.MINE_METAL, new[]{
            new StateNameAndDuration(){ name = "Mine Success", status = InteractionManager.Goap_State_Success, duration = 4 },
        } },
        {INTERACTION_TYPE.MINE_STONE, new[]{
            new StateNameAndDuration(){ name = "Mine Success", status = InteractionManager.Goap_State_Success, duration = 20 },
        } },
        {INTERACTION_TYPE.MINE, new[]{
            new StateNameAndDuration(){ name = "Mine Success", status = InteractionManager.Goap_State_Success, duration = 4 },
        } },
        {INTERACTION_TYPE.SLEEP, new[]{
            new StateNameAndDuration(){ name = "Rest Success", status = InteractionManager.Goap_State_Success, duration = 480 },
        } },
        {INTERACTION_TYPE.SLEEP_OUTSIDE, new[]{
            new StateNameAndDuration(){ name = "Rest Success", status = InteractionManager.Goap_State_Success, duration = 120, animationName = "Sleep Ground" },
        } },
        {INTERACTION_TYPE.PICK_UP, new[]{
            new StateNameAndDuration(){ name = "Take Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.DAYDREAM, new[]{
            new StateNameAndDuration(){ name = "Daydream Success", status = InteractionManager.Goap_State_Success, duration = 30 },
        } },
        {INTERACTION_TYPE.PLAY, new[]{
            new StateNameAndDuration(){ name = "Play Success", status = InteractionManager.Goap_State_Success, duration = 10 },
        } },
        {INTERACTION_TYPE.PLAY_GUITAR, new[]{
            new StateNameAndDuration(){ name = "Play Success", status = InteractionManager.Goap_State_Success, duration = 10 },
        } },
        {INTERACTION_TYPE.RETURN_HOME, new[]{
            new StateNameAndDuration(){ name = "Return Home Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.DRINK, new[]{
            new StateNameAndDuration(){ name = "Drink Success", status = InteractionManager.Goap_State_Success, duration = 30 },
        } },
        {INTERACTION_TYPE.REMOVE_POISON, new[]{
            new StateNameAndDuration(){ name = "Remove Success", status = InteractionManager.Goap_State_Success, duration = 3 },
        } },
        {INTERACTION_TYPE.REMOVE_TRAP, new[]{
            new StateNameAndDuration(){ name = "Remove Success", status = InteractionManager.Goap_State_Success, duration = 3 },
        } },
        {INTERACTION_TYPE.REMOVE_FREEZING, new[]{
            new StateNameAndDuration(){ name = "Remove Success", status = InteractionManager.Goap_State_Success, duration = 5 },
        } },
        {INTERACTION_TYPE.REMOVE_UNCONSCIOUS, new[]{
            new StateNameAndDuration(){ name = "Remove Success", status = InteractionManager.Goap_State_Success, duration = 5 },
        } },
        {INTERACTION_TYPE.REMOVE_RESTRAINED, new[]{
            new StateNameAndDuration(){ name = "Remove Success", status = InteractionManager.Goap_State_Success, duration = 5 },
        } },
        {INTERACTION_TYPE.POISON, new[]{
            new StateNameAndDuration(){ name = "Poison Success", status = InteractionManager.Goap_State_Success, duration = 5 },
        } },
        {INTERACTION_TYPE.PRAY, new[]{
            new StateNameAndDuration(){ name = "Pray Success", status = InteractionManager.Goap_State_Success, duration = 5 },
        } },
        {INTERACTION_TYPE.CHOP_WOOD, new[]{
            new StateNameAndDuration(){ name = "Chop Success", status = InteractionManager.Goap_State_Success, duration = 20 },
        } },
        {INTERACTION_TYPE.STEAL, new[]{
            new StateNameAndDuration(){ name = "Steal Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.PICKPOCKET, new[]{
            new StateNameAndDuration(){ name = "Pickpocket Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.STEAL_COINS, new[]{
            new StateNameAndDuration(){ name = "Steal Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.SCRAP, new[]{
            new StateNameAndDuration(){ name = "Scrap Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.DEPOSIT_RESOURCE_PILE, new[]{
            new StateNameAndDuration(){ name = "Deposit Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.DROP_RESOURCE, new[]{
            new StateNameAndDuration(){ name = "Drop Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.TAKE_RESOURCE, new[]{
            new StateNameAndDuration(){ name = "Take Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.RETURN_HOME_LOCATION, new[]{
            new StateNameAndDuration(){ name = "Return Home Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.TRANSFORM_TO_WOLF_FORM, new[]{
            new StateNameAndDuration(){ name = "Transform Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
        } },
        {INTERACTION_TYPE.REVERT_TO_NORMAL_FORM, new[]{
            new StateNameAndDuration(){ name = "Transform Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
        } },
        {INTERACTION_TYPE.RESTRAIN_CHARACTER, new[]{
            new StateNameAndDuration(){ name = "Restrain Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.FIRST_AID_CHARACTER, new[]{
            new StateNameAndDuration(){ name = "First Aid Success", status = InteractionManager.Goap_State_Success, duration = 3 },
        } },
        {INTERACTION_TYPE.CURE_CHARACTER, new[]{
            new StateNameAndDuration(){ name = "Cure Success", status = InteractionManager.Goap_State_Success, duration = 5 },
        } },
        {INTERACTION_TYPE.JUDGE_CHARACTER, new[]{
            new StateNameAndDuration(){ name = "Judge Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.FEED, new[]{
            new StateNameAndDuration(){ name = "Feed Success", status = InteractionManager.Goap_State_Success, duration = 10 },
        } },
        // {INTERACTION_TYPE.DROP_ITEM, new[]{
        //     new StateNameAndDuration(){ name = "Drop Success", status = InteractionManager.Goap_State_Success, duration = 1 },
        // } },
        {INTERACTION_TYPE.ASK_FOR_HELP_SAVE_CHARACTER, new[]{
            new StateNameAndDuration(){ name = "Ask Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.ASK_FOR_HELP_REMOVE_POISON_TABLE, new[]{
            new StateNameAndDuration(){ name = "Ask Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.STAND, new[]{
            new StateNameAndDuration(){ name = "Stand Success", status = InteractionManager.Goap_State_Success, duration = 5 },
        } },
        {INTERACTION_TYPE.STAND_STILL, new[]{
            new StateNameAndDuration(){ name = "Stand Success", status = InteractionManager.Goap_State_Success, duration = 5 },
        } },
        {INTERACTION_TYPE.SIT, new[]{
            new StateNameAndDuration(){ name = "Sit Success", status = InteractionManager.Goap_State_Success, duration = 5 },
        } },
        {INTERACTION_TYPE.NAP, new[]{
            new StateNameAndDuration(){ name = "Nap Success", status = InteractionManager.Goap_State_Success, duration = 10 },
        } },
        {INTERACTION_TYPE.BURY_CHARACTER, new[]{
            new StateNameAndDuration(){ name = "Bury Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.REMEMBER_FALLEN, new[]{
            new StateNameAndDuration(){ name = "Remember Success", status = InteractionManager.Goap_State_Success, duration = 10 },
        } },
        // {INTERACTION_TYPE.CRAFT_ITEM, new[]{
        //     new StateNameAndDuration(){ name = "Craft Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
        // } },
        {INTERACTION_TYPE.SPIT, new[]{
            new StateNameAndDuration(){ name = "Spit Success", status = InteractionManager.Goap_State_Success, duration = 1 },
        } },
        {INTERACTION_TYPE.INVITE, new[]{
            new StateNameAndDuration(){ name = "Invite Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.MAKE_LOVE, new[]{
            new StateNameAndDuration(){ name = "Make Love Success", status = InteractionManager.Goap_State_Success, duration = 10 },
        } },
        {INTERACTION_TYPE.DRINK_BLOOD, new[]{
            new StateNameAndDuration(){ name = "Drink Success", status = InteractionManager.Goap_State_Success, duration = 3 },
        } },
        {INTERACTION_TYPE.REPLACE_TILE_OBJECT, new[]{
            new StateNameAndDuration(){ name = "Replace Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.TANTRUM, new[]{
            new StateNameAndDuration(){ name = "Tantrum Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.BREAK_UP, new[]{
            new StateNameAndDuration(){ name = "Break Up Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.SHARE_INFORMATION, new[]{
            new StateNameAndDuration(){ name = "Share Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.REPORT_CRIME, new[]{
            new StateNameAndDuration(){ name = "Report Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        //{INTERACTION_TYPE.WATCH, new[]{
        //    new StateNameAndDuration(){ name = "Watch Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },//-1
        //} },
        {INTERACTION_TYPE.INSPECT, new[]{
            new StateNameAndDuration(){ name = "Inspect Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        //{ INTERACTION_TYPE.PUKE, new[]{
        //    new StateNameAndDuration(){ name = "Puke Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
        //} },
        //{ INTERACTION_TYPE.SEPTIC_SHOCK, new[]{
        //    new StateNameAndDuration(){ name = "Septic Shock Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
        //} },
        //{ INTERACTION_TYPE.ZOMBIE_DEATH, new[]{
        //    new StateNameAndDuration(){ name = "Zombie Death Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
        //} },
        {INTERACTION_TYPE.CARRY, new[]{
            new StateNameAndDuration(){ name = "Carry Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.DROP, new[]{
            new StateNameAndDuration(){ name = "Drop Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.CARRY_CORPSE, new[]{
            new StateNameAndDuration(){ name = "Carry Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.DROP_CORPSE, new[]{
            new StateNameAndDuration(){ name = "Drop Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.KNOCKOUT_CHARACTER, new[]{
            new StateNameAndDuration(){ name = "Knockout Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.RITUAL_KILLING, new[]{
            new StateNameAndDuration(){ name = "Killing Success", status = InteractionManager.Goap_State_Success, duration = 5 },
        } },
        {INTERACTION_TYPE.RESOLVE_CONFLICT, new[]{
            new StateNameAndDuration(){ name = "Resolve Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        { INTERACTION_TYPE.ACCIDENT, new[]{
            new StateNameAndDuration(){ name = "Accident Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        //{ INTERACTION_TYPE.STUMBLE, new[]{
        //    new StateNameAndDuration(){ name = "Stumble Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(10), animationName = "Sleep Ground" },
        //} },
        {INTERACTION_TYPE.BUTCHER, new[]{
            new StateNameAndDuration(){ name = "Transform Success", status = InteractionManager.Goap_State_Success, duration = 20 },
        } },
        {INTERACTION_TYPE.ASK_TO_STOP_JOB, new[]{
            new StateNameAndDuration(){ name = "Ask Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.WELL_JUMP, new[]{
            new StateNameAndDuration(){ name = "Well Jump Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.STRANGLE, new[]{
            new StateNameAndDuration(){ name = "Strangle Success", status = InteractionManager.Goap_State_Success, duration = 5 },
        } },
        {INTERACTION_TYPE.REPAIR, new[]{
            new StateNameAndDuration(){ name = "Repair Success", status = InteractionManager.Goap_State_Success, duration = 5 },
        } },
        //{ INTERACTION_TYPE.NARCOLEPTIC_NAP, new[]{
        //    new StateNameAndDuration(){ name = "Nap Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1), animationName = "Sleep Ground" },
        //} },
        // { INTERACTION_TYPE.SHOCK, new[]{
        //     new StateNameAndDuration(){ name = "Shock Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
        // } },
        { INTERACTION_TYPE.CRY, new[]{
            new StateNameAndDuration(){ name = "Cry Success", status = InteractionManager.Goap_State_Success, duration = 5 },
        } },
        {INTERACTION_TYPE.CRAFT_TILE_OBJECT, new[]{
            new StateNameAndDuration(){ name = "Craft Success", status = InteractionManager.Goap_State_Success, duration = 5 },
        } },
        {INTERACTION_TYPE.PRAY_TILE_OBJECT, new[]{
            new StateNameAndDuration(){ name = "Pray Success", status = InteractionManager.Goap_State_Success, duration = 5 },
        } },
        {INTERACTION_TYPE.HAVE_AFFAIR, new[]{
            new StateNameAndDuration(){ name = "Affair Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.SLAY_CHARACTER, new[]{
            new StateNameAndDuration(){ name = "Slay Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        //{INTERACTION_TYPE.FEELING_CONCERNED, new[]{
        //    new StateNameAndDuration(){ name = "Concerned Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(10) },
        //} },
        //{INTERACTION_TYPE.LAUGH_AT, new[]{
        //    new StateNameAndDuration(){ name = "Laugh Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(10) },
        //} },
        //{INTERACTION_TYPE.TEASE, new[]{
        //    new StateNameAndDuration(){ name = "Tease Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(10) },
        //} },
        //{INTERACTION_TYPE.FEELING_SPOOKED, new[]{
        //    new StateNameAndDuration(){ name = "Spooked Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
        //} },
        //{INTERACTION_TYPE.FEELING_BROKENHEARTED, new[]{
        //    new StateNameAndDuration(){ name = "Brokenhearted Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
        //} },
        //{INTERACTION_TYPE.GRIEVING, new[]{
        //    new StateNameAndDuration(){ name = "Grieving Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
        //} },
        {INTERACTION_TYPE.GO_TO, new[]{
            new StateNameAndDuration(){ name = "Goto Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.SING, new[]{
            new StateNameAndDuration(){ name = "Sing Success", status = InteractionManager.Goap_State_Success, duration = 5 },
        } },
        {INTERACTION_TYPE.DANCE, new[]{
            new StateNameAndDuration(){ name = "Dance Success", status = InteractionManager.Goap_State_Success, duration = 10 },
        } },
        //{INTERACTION_TYPE.DESTROY_RESOURCE, new[]{
        //    new StateNameAndDuration(){ name = "Destroy Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
        //} },
        {INTERACTION_TYPE.SCREAM_FOR_HELP, new[]{
            new StateNameAndDuration(){ name = "Scream Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(9) },
        } },
        {INTERACTION_TYPE.REACT_TO_SCREAM, new[]{
            new StateNameAndDuration(){ name = "React Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.RESOLVE_COMBAT, new[]{
            new StateNameAndDuration(){ name = "Combat Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.CHANGE_CLASS, new[]{
            new StateNameAndDuration(){ name = "Change Class Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.VISIT, new[]{
            new StateNameAndDuration(){ name = "Visit Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.PLACE_BLUEPRINT, new[]{
            new StateNameAndDuration(){ name = "Place Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.BUILD_BLUEPRINT, new[]{
            new StateNameAndDuration(){ name = "Build Success", status = InteractionManager.Goap_State_Success, duration = 20 },
        } },
        {INTERACTION_TYPE.STEALTH_TRANSFORM, new[]{
            new StateNameAndDuration(){ name = "Transform Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.HARVEST_PLANT, new[]{
            new StateNameAndDuration(){ name = "Harvest Success", status = InteractionManager.Goap_State_Success, duration = 4 },
        } },
        {INTERACTION_TYPE.REPAIR_STRUCTURE, new[]{
            new StateNameAndDuration(){ name = "Repair Success", status = InteractionManager.Goap_State_Success, duration = 5 },
        } },
        {INTERACTION_TYPE.NEUTRALIZE, new[]{
            new StateNameAndDuration(){ name = "Neutralize Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.ROAM, new[]{
            new StateNameAndDuration(){ name = "Roam Success", status = InteractionManager.Goap_State_Success, duration = 3 },
        } },
        //{INTERACTION_TYPE.STUDY_MONSTER, new[]{
        //    new StateNameAndDuration(){ name = "Study Success", status = InteractionManager.Goap_State_Success, duration = 3 },
        //} },
        {INTERACTION_TYPE.DROP_ITEM, new[]{
            new StateNameAndDuration(){ name = "Drop Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.DESTROY_RESOURCE_AMOUNT, new[]{
            new StateNameAndDuration(){ name = "Destroy Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.CREATE_HEALING_POTION, new[]{
            new StateNameAndDuration(){ name = "Create Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.CREATE_ANTIDOTE, new[]{
            new StateNameAndDuration(){ name = "Create Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.CREATE_POISON_FLASK, new[]{
            new StateNameAndDuration(){ name = "Create Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        { INTERACTION_TYPE.EXTRACT_ITEM, new[]{
            new StateNameAndDuration(){ name = "Extract Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.BOOBY_TRAP, new[]{
            new StateNameAndDuration(){ name = "Trap Success", status = InteractionManager.Goap_State_Success, duration = 1 },
        } },
        {INTERACTION_TYPE.REPORT_CORRUPTED_STRUCTURE, new[]{
            new StateNameAndDuration(){ name = "Report Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.FISH, new[]{
            new StateNameAndDuration(){ name = "Fish Success", status = InteractionManager.Goap_State_Success, duration = 5 },
        } },
        {INTERACTION_TYPE.DOUSE_FIRE, new[]{
            new StateNameAndDuration(){ name = "Douse Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.ATTACK_DEMONIC_STRUCTURE, new[]{
            new StateNameAndDuration(){ name = "Attack Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.HEAL_SELF, new[]{
            new StateNameAndDuration(){ name = "Heal Success", status = InteractionManager.Goap_State_Success, duration = 3 },
        } },
        {INTERACTION_TYPE.OPEN, new[]{
            new StateNameAndDuration(){ name = "Open Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.EXILE, new[]{
            new StateNameAndDuration(){ name = "Exile Success", status = InteractionManager.Goap_State_Success, duration = 1 },
        } },
        {INTERACTION_TYPE.EXECUTE, new[]{
            new StateNameAndDuration(){ name = "Execute Success", status = InteractionManager.Goap_State_Success, duration = 1 },
        } },
        {INTERACTION_TYPE.ABSOLVE, new[]{
            new StateNameAndDuration(){ name = "Absolve Success", status = InteractionManager.Goap_State_Success, duration = 1 },
        } },
        {INTERACTION_TYPE.WHIP, new[]{
            new StateNameAndDuration(){ name = "Whip Success", status = InteractionManager.Goap_State_Success, duration = 1 },
        } },
        //{INTERACTION_TYPE.TEND, new[]{
        //    new StateNameAndDuration(){ name = "Tend Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(5) },
        //} },
        //{INTERACTION_TYPE.START_TEND, new[]{
        //    new StateNameAndDuration(){ name = "Start Tend Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        //} },
        {INTERACTION_TYPE.START_DOUSE, new[]{
            new StateNameAndDuration(){ name = "Start Douse Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.START_CLEANSE, new[]{
            new StateNameAndDuration(){ name = "Start Cleanse Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.START_DRY, new[]{
            new StateNameAndDuration(){ name = "Start Dry Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.START_PATROL, new[]{
            new StateNameAndDuration(){ name = "Start Patrol Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.CLEANSE_TILE, new[]{
            new StateNameAndDuration(){ name = "Cleanse Success", status = InteractionManager.Goap_State_Success, duration = 2 },
            new StateNameAndDuration(){ name = "Ice Cleanse Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.CLEAN_UP, new[]{
            new StateNameAndDuration(){ name = "Clean Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.PATROL, new[]{
            new StateNameAndDuration(){ name = "Patrol Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.DIG, new[]{
            new StateNameAndDuration(){ name = "Dig Success", status = InteractionManager.Goap_State_Success, duration = 3 },
        } },
        {INTERACTION_TYPE.BUILD_LAIR, new[]{
            new StateNameAndDuration(){ name = "Build Success", status = InteractionManager.Goap_State_Success, duration = 20 },
        } },
        {INTERACTION_TYPE.ABSORB_LIFE, new[]{
            new StateNameAndDuration(){ name = "Absorb Success", status = InteractionManager.Goap_State_Success, duration = 10 },
        } },
        {INTERACTION_TYPE.ABSORB_POWER, new[]{
            new StateNameAndDuration(){ name = "Absorb Success", status = InteractionManager.Goap_State_Success, duration = 10 },
        } },
        {INTERACTION_TYPE.SPAWN_SKELETON, new[]{
            new StateNameAndDuration(){ name = "Spawn Success", status = InteractionManager.Goap_State_Success, duration = 10 },
        } },
        {INTERACTION_TYPE.RAISE_CORPSE, new[]{
            new StateNameAndDuration(){ name = "Raise Success", status = InteractionManager.Goap_State_Success, duration = 10 },
        } },
        {INTERACTION_TYPE.PLACE_FREEZING_TRAP, new[]{
            new StateNameAndDuration(){ name = "Place Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.EAT_CORPSE, new[]{
            new StateNameAndDuration(){ name = "Eat Success", status = InteractionManager.Goap_State_Success, duration = 6 },
        } },
        {INTERACTION_TYPE.BEGIN_MINE, new[]{
            new StateNameAndDuration(){ name = "Begin Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.READ_NECRONOMICON, new[]{
            new StateNameAndDuration(){ name = "Read Success", status = InteractionManager.Goap_State_Success, duration = 10 },
        } },
        {INTERACTION_TYPE.MEDITATE, new[]{
            new StateNameAndDuration(){ name = "Meditate Success", status = InteractionManager.Goap_State_Success, duration = 10 },
        } },
        {INTERACTION_TYPE.REGAIN_ENERGY, new[]{
            new StateNameAndDuration(){ name = "Regain Success", status = InteractionManager.Goap_State_Success, duration = 20 },
        } },
        {INTERACTION_TYPE.MURDER, new[]{
            new StateNameAndDuration(){ name = "Murder Success", status = InteractionManager.Goap_State_Success, duration = 3 },
        } },
        {INTERACTION_TYPE.EAT_ALIVE, new[]{
            new StateNameAndDuration(){ name = "Eat Alive Success", status = InteractionManager.Goap_State_Success, duration = 5 },
        } },
        {INTERACTION_TYPE.REMOVE_BUFF, new[]{
            new StateNameAndDuration(){ name = "Remove Buff Success", status = InteractionManager.Goap_State_Success, duration = 3 },
        } },
        {INTERACTION_TYPE.CREATE_CULTIST_KIT, new[]{
            new StateNameAndDuration(){ name = "Create Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.IS_CULTIST, new[]{
            new StateNameAndDuration(){ name = "Cultist Success", status = InteractionManager.Goap_State_Success, duration = 1 },
        } },
        {INTERACTION_TYPE.SPAWN_POISON_CLOUD, new[]{
            new StateNameAndDuration(){ name = "Spawn Success", status = InteractionManager.Goap_State_Success, duration = 3 },
        } },
        {INTERACTION_TYPE.DECREASE_MOOD, new[]{
            new StateNameAndDuration(){ name = "Decrease Success", status = InteractionManager.Goap_State_Success, duration = 1 },
        } },
        {INTERACTION_TYPE.GO_TO_TILE, new[]{
            new StateNameAndDuration(){ name = "Go Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.GO_TO_SPECIFIC_TILE, new[]{
            new StateNameAndDuration(){ name = "Go Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.DISABLE, new[]{
            new StateNameAndDuration(){ name = "Disable Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.LAY_EGG, new[]{
            new StateNameAndDuration(){ name = "Lay Success", status = InteractionManager.Goap_State_Success, duration = 5 },
        } },
        {INTERACTION_TYPE.BURN, new[]{
            new StateNameAndDuration(){ name = "Burn Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.TAKE_SHELTER, new[]{
            new StateNameAndDuration(){ name = "Take Shelter Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.IS_PLAGUED, new[]{
            new StateNameAndDuration(){ name = "Plague Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.DARK_RITUAL, new[]{
            new StateNameAndDuration(){ name = "Ritual Success", status = InteractionManager.Goap_State_Success, duration = 10 },
        } },
        {INTERACTION_TYPE.DRAW_MAGIC_CIRCLE, new[]{
            new StateNameAndDuration(){ name = "Draw Success", status = InteractionManager.Goap_State_Success, duration = 3 },
        } },
        {INTERACTION_TYPE.CULTIST_TRANSFORM, new[]{
            new StateNameAndDuration(){ name = "Transform Success", status = InteractionManager.Goap_State_Success, duration = 1 },
        } },
        {INTERACTION_TYPE.JOIN_GATHERING, new[]{
            new StateNameAndDuration(){ name = "Join Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        //{INTERACTION_TYPE.EXPLORE, new[]{
        //    new StateNameAndDuration(){ name = "Explore Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        //} },
        //{INTERACTION_TYPE.EXTERMINATE, new[]{
        //    new StateNameAndDuration(){ name = "Exterminate Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        //} },
        //{INTERACTION_TYPE.RESCUE, new[]{
        //    new StateNameAndDuration(){ name = "Rescue Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        //} },
        //{INTERACTION_TYPE.COUNTERATTACK_ACTION, new[]{
        //    new StateNameAndDuration(){ name = "Counter Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        //} },
        {INTERACTION_TYPE.MONSTER_INVADE, new[]{
            new StateNameAndDuration(){ name = "Invade Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.DISGUISE, new[]{
            new StateNameAndDuration(){ name = "Disguise Success", status = InteractionManager.Goap_State_Success, duration = 3 },
        } },
        {INTERACTION_TYPE.RECRUIT, new[]{
            new StateNameAndDuration(){ name = "Recruit Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        //{INTERACTION_TYPE.RAID, new[]{
        //    new StateNameAndDuration(){ name = "Raid Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        //} },
        {INTERACTION_TYPE.COOK, new[]{
            new StateNameAndDuration(){ name = "Cook Success", status = InteractionManager.Goap_State_Success, duration = 10 },
        } },
        {INTERACTION_TYPE.BUILD_TROLL_CAULDRON, new[]{
            new StateNameAndDuration(){ name = "Build Success", status = InteractionManager.Goap_State_Success, duration = 5 },
        } },
        {INTERACTION_TYPE.FLEE_CRIME, new[]{
            new StateNameAndDuration(){ name = "Flee Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.HOST_SOCIAL_PARTY, new[]{
            new StateNameAndDuration(){ name = "Host Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.PLAY_CARDS, new[]{
            new StateNameAndDuration(){ name = "Play Success", status = InteractionManager.Goap_State_Success, duration = 10 },
        } },
        {INTERACTION_TYPE.BUILD_WOLF_LAIR, new[]{
            new StateNameAndDuration(){ name = "Build Success", status = InteractionManager.Goap_State_Success, duration = 20 },
        } },
        {INTERACTION_TYPE.BUILD_CAMPFIRE, new[]{
            new StateNameAndDuration(){ name = "Build Success", status = InteractionManager.Goap_State_Success, duration = 3 },
        } },
        {INTERACTION_TYPE.WARM_UP, new[]{
            new StateNameAndDuration(){ name = "Warm Success", status = InteractionManager.Goap_State_Success, duration = 5 },
        } },
        {INTERACTION_TYPE.TRESPASSING, new[]{
            new StateNameAndDuration(){ name = "Trespass Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.EVANGELIZE, new[]{
            new StateNameAndDuration(){ name = "Evangelize Success", status = InteractionManager.Goap_State_Success, duration = 5 },
        } },
        {INTERACTION_TYPE.HUNT_HEIRLOOM, new[]{
            new StateNameAndDuration(){ name = "Hunt Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.REMOVE_ENSNARED, new[]{
            new StateNameAndDuration(){ name = "Remove Success", status = InteractionManager.Goap_State_Success, duration = 5 },
        } },
        {INTERACTION_TYPE.VAMPIRIC_EMBRACE, new[]{
            new StateNameAndDuration(){ name = "Embrace Success", status = InteractionManager.Goap_State_Success, duration = 5 },
        } },
        {INTERACTION_TYPE.BUILD_VAMPIRE_CASTLE, new[]{
            new StateNameAndDuration(){ name = "Build Success", status = InteractionManager.Goap_State_Success, duration = 20 },
        } },
        {INTERACTION_TYPE.BURN_AT_STAKE, new[]{
            new StateNameAndDuration(){ name = "Burn Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.IS_VAMPIRE, new[]{
            new StateNameAndDuration(){ name = "Vampire Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.FEED_SELF, new[]{
            new StateNameAndDuration(){ name = "Feed Success", status = InteractionManager.Goap_State_Success, duration = 5 },
        } },
        {INTERACTION_TYPE.CARRY_RESTRAINED, new[]{
            new StateNameAndDuration(){ name = "Carry Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.DROP_RESTRAINED, new[]{
            new StateNameAndDuration(){ name = "Drop Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.BUILD_NEW_VILLAGE, new[]{
            new StateNameAndDuration(){ name = "Build Success", status = InteractionManager.Goap_State_Success, duration = 20 },
        } },
        {INTERACTION_TYPE.IS_WEREWOLF, new[]{
            new StateNameAndDuration(){ name = "Werewolf Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.DISPEL, new[]{
            new StateNameAndDuration(){ name = "Dispel Success", status = InteractionManager.Goap_State_Success, duration = 3 },
        } },
        {INTERACTION_TYPE.SUMMON_BONE_GOLEM, new[]{
            new StateNameAndDuration(){ name = "Summon Success", status = InteractionManager.Goap_State_Success, duration = 10 },
        } },
        {INTERACTION_TYPE.BIRTH_RATMAN, new[]{
            new StateNameAndDuration(){ name = "Birth Success", status = InteractionManager.Goap_State_Success, duration = 5 },
        } },
        {INTERACTION_TYPE.TORTURE, new[]{
            new StateNameAndDuration(){ name = "Torture Success", status = InteractionManager.Goap_State_Success, duration = 5 },
        } },
        {INTERACTION_TYPE.CARRY_PATIENT, new[]{
            new StateNameAndDuration(){ name = "Carry Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.QUARANTINE, new[]{
            new StateNameAndDuration(){ name = "Quarantine Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.START_PLAGUE_CARE, new[]{
            new StateNameAndDuration(){ name = "Care Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.CARE, new[]{
            new StateNameAndDuration(){ name = "Care Success", status = InteractionManager.Goap_State_Success, duration = 3 },
        } },
        {INTERACTION_TYPE.LONG_STAND_STILL, new[]{
            new StateNameAndDuration(){ name = "Stand Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(8) },
        } },
        {INTERACTION_TYPE.BURROW, new[]{
            new StateNameAndDuration(){ name = "Burrow Success", status = InteractionManager.Goap_State_Success, duration = 3 },
        } },
        {INTERACTION_TYPE.DISPOSE_FOOD, new[]{
            new StateNameAndDuration(){ name = "Dispose Success", status = InteractionManager.Goap_State_Success, duration = 1 },
        } },
        {INTERACTION_TYPE.IS_IMPRISONED, new[]{
            new StateNameAndDuration(){ name = "Imprisoned Success", status = InteractionManager.Goap_State_Success, duration = 1 },
        } },
        {INTERACTION_TYPE.STEAL_ANYTHING, new[]{
            new StateNameAndDuration(){ name = "Steal Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.ABSORB_POWER_CRYSTAL, new[]{
            new StateNameAndDuration(){ name = "Absorb Crystal Success", status = InteractionManager.Goap_State_Success, duration = 20 },
        } },
        {INTERACTION_TYPE.MINE_ORE, new[]{
            new StateNameAndDuration(){ name = "Mine Ore Success", status = InteractionManager.Goap_State_Success, duration = 20 },
        } },
        {INTERACTION_TYPE.FIND_FISH, new[]{
            new StateNameAndDuration(){ name = "Find Fish Success", status = InteractionManager.Goap_State_Success, duration = 20 },
        } },
        {INTERACTION_TYPE.TILL_TILE, new[]{
            new StateNameAndDuration(){ name = "Till Tile Success", status = InteractionManager.Goap_State_Success, duration = 20 },
        } },
        {INTERACTION_TYPE.SHEAR_ANIMAL, new[]{
            new StateNameAndDuration(){ name = "Shear Animal Success", status = InteractionManager.Goap_State_Success, duration = 20 },
        } },
        {INTERACTION_TYPE.SKIN_ANIMAL, new[]{
            new StateNameAndDuration(){ name = "Skin Animal Success", status = InteractionManager.Goap_State_Success, duration = 20 },
        } },
        {INTERACTION_TYPE.HARVEST_CROPS, new[]{
            new StateNameAndDuration(){ name = "Harvest Crops Success", status = InteractionManager.Goap_State_Success, duration = 20 },
        } },
        {INTERACTION_TYPE.RECUPERATE, new[]{
            new StateNameAndDuration(){ name = "Recuperate Success", status = InteractionManager.Goap_State_Success, duration = 480 },
        } },
        {INTERACTION_TYPE.HEALER_CURE, new[]{
            new StateNameAndDuration(){ name = "Healer Cure Success", status = InteractionManager.Goap_State_Success, duration = 10 },
        } },
        {INTERACTION_TYPE.GATHER_HERB, new[]{
            new StateNameAndDuration(){ name = "Gather Herb Success", status = InteractionManager.Goap_State_Success, duration = 10 },
        } },
        {INTERACTION_TYPE.CREATE_HOSPICE_POTION, new[]{
            new StateNameAndDuration(){ name = "Create Hospice Potion Success", status = InteractionManager.Goap_State_Success, duration = 10 },
        } },
        {INTERACTION_TYPE.CREATE_HOSPICE_ANTIDOTE, new[]{
            new StateNameAndDuration(){ name = "Create Hospice Antidote Success", status = InteractionManager.Goap_State_Success, duration = 10 },
        } },
        {INTERACTION_TYPE.BUY_FOOD, new[]{
            new StateNameAndDuration(){ name = "Buy Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Take Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.STOCKPILE_FOOD, new[]{
            new StateNameAndDuration(){ name = "Stockpile Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.CRAFT_FURNITURE_WOOD, new[]{
            new StateNameAndDuration(){ name = "Craft Success", status = InteractionManager.Goap_State_Success, duration = 5 },
        } },
        {INTERACTION_TYPE.BUY_WOOD, new[]{
            new StateNameAndDuration(){ name = "Buy Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Take Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.CRAFT_FURNITURE_STONE, new[]{
            new StateNameAndDuration(){ name = "Craft Success", status = InteractionManager.Goap_State_Success, duration = 5 },
        } },
        {INTERACTION_TYPE.BUY_STONE, new[]{
            new StateNameAndDuration(){ name = "Buy Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Take Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.BUY_ITEM, new[]{
            new StateNameAndDuration(){ name = "Buy Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Take Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.CRAFT_EQUIPMENT, new[]{
            new StateNameAndDuration(){ name = "Craft Equipment Success", status = InteractionManager.Goap_State_Success, duration = 30 },
        } },
        {INTERACTION_TYPE.DROP_RESOURCE_TO_WORK_STRUCTURE, new[]{
            new StateNameAndDuration(){ name = "Drop Resource To Work Structure Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.DRINK_WATER, new[]{
            new StateNameAndDuration(){ name = "Drink Success", status = InteractionManager.Goap_State_Success, duration = 5 },
        } }
    };
}

public struct StateNameAndDuration {
    public string name;
    public string status;
    public int duration;
    public string animationName;
}
