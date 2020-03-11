using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class RaceManager : MonoBehaviour {
    public static RaceManager Instance;

    private Dictionary<string, RaceSetting> _racesDictionary;
    //private Dictionary<RACE, INTERACTION_TYPE[]> _npcRaceInteractions;

    #region getters/setters
    public Dictionary<string, RaceSetting> racesDictionary {
        get { return _racesDictionary; }
    }
    #endregion

    void Awake() {
        Instance = this;    
    }

    public void Initialize() {
        ConstructAllRaces();
        //ConstructNPCRaceInteractions();
    }

    private void ConstructAllRaces() {
        _racesDictionary = new Dictionary<string, RaceSetting>();
        string path = $"{UtilityScripts.Utilities.dataPath}RaceSettings/";
        string[] races = System.IO.Directory.GetFiles(path, "*.json");
        for (int i = 0; i < races.Length; i++) {
            RaceSetting currentRace = JsonUtility.FromJson<RaceSetting>(System.IO.File.ReadAllText(races[i]));
            _racesDictionary.Add(currentRace.race.ToString(), currentRace);
        }
    }

    #region NPC Interaction
    //private void ConstructNPCRaceInteractions() {
    //    _npcRaceInteractions = new Dictionary<RACE, INTERACTION_TYPE[]>() {
    //         { RACE.HUMANS, new INTERACTION_TYPE[] {
    //            INTERACTION_TYPE.RELEASE_CHARACTER,
    //            //INTERACTION_TYPE.CRAFT_TOOL,
    //            INTERACTION_TYPE.PICK_UP,
    //            //INTERACTION_TYPE.MINE_ACTION,
    //            INTERACTION_TYPE.EAT,
    //            INTERACTION_TYPE.EAT,
    //            INTERACTION_TYPE.EAT,
    //            INTERACTION_TYPE.SLEEP,
    //            INTERACTION_TYPE.DAYDREAM,
    //            INTERACTION_TYPE.PLAY_GUITAR,
    //            INTERACTION_TYPE.CHAT_CHARACTER,
    //            //INTERACTION_TYPE.ARGUE_CHARACTER,
    //            //INTERACTION_TYPE.CRAFT_ITEM,
    //            //INTERACTION_TYPE.STROLL,
    //            INTERACTION_TYPE.RETURN_HOME,
    //            INTERACTION_TYPE.DRINK,
    //            INTERACTION_TYPE.SLEEP_OUTSIDE,
    //            //INTERACTION_TYPE.EXPLORE,
    //            INTERACTION_TYPE.REMOVE_POISON,
    //            INTERACTION_TYPE.POISON,
    //            INTERACTION_TYPE.PRAY,
    //            //INTERACTION_TYPE.CHOP_WOOD,
    //            //INTERACTION_TYPE.PATROL,
    //            //INTERACTION_TYPE.STEAL,
    //            //INTERACTION_TYPE.STEAL_CHARACTER,
    //            //INTERACTION_TYPE.SCRAP,
    //            //INTERACTION_TYPE.GET_SUPPLY,
    //            INTERACTION_TYPE.DROP_RESOURCE,
    //            INTERACTION_TYPE.DROP_FOOD,
    //            INTERACTION_TYPE.OBTAIN_RESOURCE,
    //            //INTERACTION_TYPE.TRAVEL,
    //            INTERACTION_TYPE.TRANSFORM_TO_WOLF_FORM,
    //            INTERACTION_TYPE.REVERT_TO_NORMAL_FORM,
    //            INTERACTION_TYPE.RESTRAIN_CHARACTER,
    //            INTERACTION_TYPE.FIRST_AID_CHARACTER,
    //            INTERACTION_TYPE.CURE_CHARACTER,
    //            INTERACTION_TYPE.CURSE_CHARACTER,
    //            INTERACTION_TYPE.DISPEL_MAGIC,
    //            INTERACTION_TYPE.JUDGE_CHARACTER,
    //            INTERACTION_TYPE.FEED,
    //            INTERACTION_TYPE.DROP_ITEM,
    //            INTERACTION_TYPE.ASK_FOR_HELP_SAVE_CHARACTER,
    //            INTERACTION_TYPE.ASK_FOR_HELP_REMOVE_POISON_TABLE,
    //            INTERACTION_TYPE.STAND,
    //            INTERACTION_TYPE.SIT,
    //            INTERACTION_TYPE.NAP,
    //            INTERACTION_TYPE.BURY_CHARACTER,
    //            INTERACTION_TYPE.REMEMBER_FALLEN,
    //            INTERACTION_TYPE.SPIT,
    //            INTERACTION_TYPE.INVITE,
    //            INTERACTION_TYPE.MAKE_LOVE,
    //            INTERACTION_TYPE.DRINK_BLOOD,
    //            INTERACTION_TYPE.REPLACE_TILE_OBJECT,
    //            INTERACTION_TYPE.TANTRUM,
    //            INTERACTION_TYPE.EAT,
    //            INTERACTION_TYPE.BREAK_UP,
    //            INTERACTION_TYPE.SHARE_INFORMATION,
    //            INTERACTION_TYPE.WATCH,
    //            INTERACTION_TYPE.INSPECT,
    //            INTERACTION_TYPE.PUKE,
    //            INTERACTION_TYPE.SEPTIC_SHOCK,
    //            INTERACTION_TYPE.CARRY,
    //            INTERACTION_TYPE.DROP,
    //            INTERACTION_TYPE.KNOCKOUT_CHARACTER,
    //            INTERACTION_TYPE.RITUAL_KILLING,
    //            INTERACTION_TYPE.RESOLVE_CONFLICT,
    //            INTERACTION_TYPE.STUMBLE,
    //            INTERACTION_TYPE.ACCIDENT,
    //            INTERACTION_TYPE.BUTCHER,
    //            INTERACTION_TYPE.ASK_TO_STOP_JOB,
    //        } },
    //        { RACE.ELVES, new INTERACTION_TYPE[] {
    //            INTERACTION_TYPE.RELEASE_CHARACTER,
    //            //INTERACTION_TYPE.CRAFT_TOOL,
    //            INTERACTION_TYPE.PICK_UP,
    //            //INTERACTION_TYPE.MINE_ACTION,
    //            INTERACTION_TYPE.EAT,
    //            INTERACTION_TYPE.EAT,
    //            INTERACTION_TYPE.EAT,
    //            INTERACTION_TYPE.SLEEP,
    //            //INTERACTION_TYPE.ASSAULT_ACTION_NPC,
    //            INTERACTION_TYPE.DAYDREAM,
    //            INTERACTION_TYPE.PLAY_GUITAR,
    //            INTERACTION_TYPE.CHAT_CHARACTER,
    //            //INTERACTION_TYPE.ARGUE_CHARACTER,
    //            //INTERACTION_TYPE.CRAFT_ITEM,
    //            //INTERACTION_TYPE.STROLL,
    //            INTERACTION_TYPE.RETURN_HOME,
    //            INTERACTION_TYPE.DRINK,
    //            INTERACTION_TYPE.SLEEP_OUTSIDE,
    //            //INTERACTION_TYPE.EXPLORE,
    //            INTERACTION_TYPE.REMOVE_POISON,
    //            INTERACTION_TYPE.POISON,
    //            INTERACTION_TYPE.PRAY,
    //            //INTERACTION_TYPE.CHOP_WOOD,
    //            //INTERACTION_TYPE.PATROL,
    //            //INTERACTION_TYPE.SCRAP,
    //            //INTERACTION_TYPE.GET_SUPPLY,
    //            INTERACTION_TYPE.DROP_RESOURCE,
    //            INTERACTION_TYPE.DROP_FOOD,
    //            INTERACTION_TYPE.OBTAIN_RESOURCE,
    //            //INTERACTION_TYPE.TRAVEL,
    //            INTERACTION_TYPE.TRANSFORM_TO_WOLF_FORM,
    //            INTERACTION_TYPE.REVERT_TO_NORMAL_FORM,
    //            INTERACTION_TYPE.RESTRAIN_CHARACTER,
    //            INTERACTION_TYPE.FIRST_AID_CHARACTER,
    //            INTERACTION_TYPE.CURE_CHARACTER,
    //            INTERACTION_TYPE.CURSE_CHARACTER,
    //            INTERACTION_TYPE.DISPEL_MAGIC,
    //            INTERACTION_TYPE.JUDGE_CHARACTER,
    //            INTERACTION_TYPE.FEED,
    //            INTERACTION_TYPE.DROP_ITEM,
    //            INTERACTION_TYPE.ASK_FOR_HELP_SAVE_CHARACTER,
    //            INTERACTION_TYPE.ASK_FOR_HELP_REMOVE_POISON_TABLE,
    //            INTERACTION_TYPE.STAND,
    //            INTERACTION_TYPE.SIT,
    //            INTERACTION_TYPE.NAP,
    //            INTERACTION_TYPE.BURY_CHARACTER,
    //            INTERACTION_TYPE.REMEMBER_FALLEN,
    //            INTERACTION_TYPE.SPIT,
    //            INTERACTION_TYPE.INVITE,
    //            INTERACTION_TYPE.MAKE_LOVE,
    //            INTERACTION_TYPE.DRINK_BLOOD,
    //            INTERACTION_TYPE.REPLACE_TILE_OBJECT,
    //            INTERACTION_TYPE.TANTRUM,
    //            INTERACTION_TYPE.EAT,
    //            INTERACTION_TYPE.BREAK_UP,
    //            INTERACTION_TYPE.SHARE_INFORMATION,
    //            INTERACTION_TYPE.WATCH,
    //            INTERACTION_TYPE.INSPECT,
    //            INTERACTION_TYPE.PUKE,
    //            INTERACTION_TYPE.SEPTIC_SHOCK,
    //            INTERACTION_TYPE.CARRY,
    //            INTERACTION_TYPE.DROP,
    //            INTERACTION_TYPE.KNOCKOUT_CHARACTER,
    //            INTERACTION_TYPE.RITUAL_KILLING,
    //            INTERACTION_TYPE.RESOLVE_CONFLICT,
    //            INTERACTION_TYPE.STUMBLE,
    //            INTERACTION_TYPE.ACCIDENT,
    //            INTERACTION_TYPE.BUTCHER,
    //            INTERACTION_TYPE.ASK_TO_STOP_JOB,
    //        } },
    //        { RACE.GOBLIN, new INTERACTION_TYPE[] {
    //            INTERACTION_TYPE.RELEASE_CHARACTER,
    //            INTERACTION_TYPE.PICK_UP,
    //            INTERACTION_TYPE.EAT,
    //            INTERACTION_TYPE.EAT,
    //            INTERACTION_TYPE.EAT,
    //            INTERACTION_TYPE.SLEEP,
    //            INTERACTION_TYPE.DAYDREAM,
    //            INTERACTION_TYPE.PLAY_GUITAR,
    //            INTERACTION_TYPE.CHAT_CHARACTER,
    //            INTERACTION_TYPE.RETURN_HOME,
    //            INTERACTION_TYPE.DRINK,
    //            INTERACTION_TYPE.SLEEP_OUTSIDE,
    //            INTERACTION_TYPE.REMOVE_POISON,
    //            INTERACTION_TYPE.POISON,
    //            INTERACTION_TYPE.PRAY,
    //            INTERACTION_TYPE.DROP_RESOURCE,
    //            INTERACTION_TYPE.DROP_FOOD,
    //            INTERACTION_TYPE.OBTAIN_RESOURCE,
    //            INTERACTION_TYPE.TRANSFORM_TO_WOLF_FORM,
    //            INTERACTION_TYPE.REVERT_TO_NORMAL_FORM,
    //            INTERACTION_TYPE.RESTRAIN_CHARACTER,
    //            INTERACTION_TYPE.CURSE_CHARACTER,
    //            INTERACTION_TYPE.JUDGE_CHARACTER,
    //            INTERACTION_TYPE.FEED,
    //            INTERACTION_TYPE.DROP_ITEM,
    //            INTERACTION_TYPE.ASK_FOR_HELP_SAVE_CHARACTER,
    //            INTERACTION_TYPE.ASK_FOR_HELP_REMOVE_POISON_TABLE,
    //            INTERACTION_TYPE.STAND,
    //            INTERACTION_TYPE.SIT,
    //            INTERACTION_TYPE.NAP,
    //            INTERACTION_TYPE.BURY_CHARACTER,
    //            INTERACTION_TYPE.REMEMBER_FALLEN,
    //            INTERACTION_TYPE.SPIT,
    //            INTERACTION_TYPE.INVITE,
    //            INTERACTION_TYPE.MAKE_LOVE,
    //            INTERACTION_TYPE.DRINK_BLOOD,
    //            INTERACTION_TYPE.REPLACE_TILE_OBJECT,
    //            INTERACTION_TYPE.TANTRUM,
    //            INTERACTION_TYPE.EAT,
    //            INTERACTION_TYPE.BREAK_UP,
    //            INTERACTION_TYPE.SHARE_INFORMATION,
    //            INTERACTION_TYPE.WATCH,
    //            INTERACTION_TYPE.INSPECT,
    //            INTERACTION_TYPE.PUKE,
    //            INTERACTION_TYPE.SEPTIC_SHOCK,
    //            INTERACTION_TYPE.CARRY,
    //            INTERACTION_TYPE.DROP,
    //            INTERACTION_TYPE.KNOCKOUT_CHARACTER,
    //            INTERACTION_TYPE.RITUAL_KILLING,
    //            INTERACTION_TYPE.RESOLVE_CONFLICT,
    //            INTERACTION_TYPE.STUMBLE,
    //            INTERACTION_TYPE.ACCIDENT,
    //            INTERACTION_TYPE.BUTCHER,
    //            INTERACTION_TYPE.ASK_TO_STOP_JOB,
    //        } },
    //        { RACE.FAERY, new INTERACTION_TYPE[] {
    //            INTERACTION_TYPE.RELEASE_CHARACTER,
    //            INTERACTION_TYPE.PICK_UP,
    //            INTERACTION_TYPE.EAT,
    //            INTERACTION_TYPE.EAT,
    //            INTERACTION_TYPE.EAT,
    //            INTERACTION_TYPE.SLEEP,
    //            INTERACTION_TYPE.DAYDREAM,
    //            INTERACTION_TYPE.PLAY_GUITAR,
    //            INTERACTION_TYPE.CHAT_CHARACTER,
    //            INTERACTION_TYPE.RETURN_HOME,
    //            INTERACTION_TYPE.DRINK,
    //            INTERACTION_TYPE.SLEEP_OUTSIDE,
    //            INTERACTION_TYPE.REMOVE_POISON,
    //            INTERACTION_TYPE.POISON,
    //            INTERACTION_TYPE.PRAY,
    //            INTERACTION_TYPE.DROP_RESOURCE,
    //            INTERACTION_TYPE.DROP_FOOD,
    //            INTERACTION_TYPE.OBTAIN_RESOURCE,
    //            INTERACTION_TYPE.TRANSFORM_TO_WOLF_FORM,
    //            INTERACTION_TYPE.REVERT_TO_NORMAL_FORM,
    //            INTERACTION_TYPE.RESTRAIN_CHARACTER,
    //            INTERACTION_TYPE.CURSE_CHARACTER,
    //            INTERACTION_TYPE.JUDGE_CHARACTER,
    //            INTERACTION_TYPE.FEED,
    //            INTERACTION_TYPE.DROP_ITEM,
    //            INTERACTION_TYPE.ASK_FOR_HELP_SAVE_CHARACTER,
    //            INTERACTION_TYPE.ASK_FOR_HELP_REMOVE_POISON_TABLE,
    //            INTERACTION_TYPE.STAND,
    //            INTERACTION_TYPE.SIT,
    //            INTERACTION_TYPE.NAP,
    //            INTERACTION_TYPE.BURY_CHARACTER,
    //            INTERACTION_TYPE.REMEMBER_FALLEN,
    //            INTERACTION_TYPE.SPIT,
    //            INTERACTION_TYPE.INVITE,
    //            INTERACTION_TYPE.MAKE_LOVE,
    //            INTERACTION_TYPE.DRINK_BLOOD,
    //            INTERACTION_TYPE.REPLACE_TILE_OBJECT,
    //            INTERACTION_TYPE.TANTRUM,
    //            INTERACTION_TYPE.EAT,
    //            INTERACTION_TYPE.BREAK_UP,
    //            INTERACTION_TYPE.SHARE_INFORMATION,
    //            INTERACTION_TYPE.WATCH,
    //            INTERACTION_TYPE.INSPECT,
    //            INTERACTION_TYPE.PUKE,
    //            INTERACTION_TYPE.SEPTIC_SHOCK,
    //            INTERACTION_TYPE.CARRY,
    //            INTERACTION_TYPE.DROP,
    //            INTERACTION_TYPE.KNOCKOUT_CHARACTER,
    //            INTERACTION_TYPE.RITUAL_KILLING,
    //            INTERACTION_TYPE.RESOLVE_CONFLICT,
    //            INTERACTION_TYPE.STUMBLE,
    //            INTERACTION_TYPE.ACCIDENT,
    //            INTERACTION_TYPE.BUTCHER,
    //            INTERACTION_TYPE.ASK_TO_STOP_JOB,
    //        } },
    //        { RACE.SKELETON, new INTERACTION_TYPE[] {
    //            INTERACTION_TYPE.RELEASE_CHARACTER,
    //            INTERACTION_TYPE.PICK_UP,
    //            INTERACTION_TYPE.EAT,
    //            INTERACTION_TYPE.EAT,
    //            INTERACTION_TYPE.EAT,
    //            INTERACTION_TYPE.SLEEP,
    //            INTERACTION_TYPE.DAYDREAM,
    //            INTERACTION_TYPE.PLAY_GUITAR,
    //            INTERACTION_TYPE.RETURN_HOME,
    //            INTERACTION_TYPE.DRINK,
    //            INTERACTION_TYPE.SLEEP_OUTSIDE,
    //            INTERACTION_TYPE.REMOVE_POISON,
    //            INTERACTION_TYPE.POISON,
    //            INTERACTION_TYPE.PRAY,
    //            INTERACTION_TYPE.DROP_RESOURCE,
    //            INTERACTION_TYPE.DROP_FOOD,
    //            INTERACTION_TYPE.OBTAIN_RESOURCE,
    //            INTERACTION_TYPE.RESTRAIN_CHARACTER,
    //            INTERACTION_TYPE.CURSE_CHARACTER,
    //            INTERACTION_TYPE.JUDGE_CHARACTER,
    //            INTERACTION_TYPE.FEED,
    //            INTERACTION_TYPE.DROP_ITEM,
    //            INTERACTION_TYPE.ASK_FOR_HELP_SAVE_CHARACTER,
    //            INTERACTION_TYPE.ASK_FOR_HELP_REMOVE_POISON_TABLE,
    //            INTERACTION_TYPE.STAND,
    //            INTERACTION_TYPE.SIT,
    //            INTERACTION_TYPE.NAP,
    //            INTERACTION_TYPE.BURY_CHARACTER,
    //            INTERACTION_TYPE.DRINK_BLOOD,
    //            INTERACTION_TYPE.PLAY,
    //            INTERACTION_TYPE.TANTRUM,
    //            INTERACTION_TYPE.EAT,
    //            INTERACTION_TYPE.BREAK_UP,
    //            INTERACTION_TYPE.SHARE_INFORMATION,
    //            INTERACTION_TYPE.WATCH,
    //            INTERACTION_TYPE.INSPECT,
    //            INTERACTION_TYPE.CARRY,
    //            INTERACTION_TYPE.DROP,
    //            INTERACTION_TYPE.STUMBLE,
    //            INTERACTION_TYPE.ACCIDENT,
    //        } },
    //        { RACE.SPIDER, new INTERACTION_TYPE[] {
    //            INTERACTION_TYPE.RELEASE_CHARACTER,
    //            INTERACTION_TYPE.PICK_UP,
    //            INTERACTION_TYPE.EAT,
    //            INTERACTION_TYPE.RETURN_HOME,
    //            INTERACTION_TYPE.SLEEP_OUTSIDE,
    //            INTERACTION_TYPE.PLAY,
    //            INTERACTION_TYPE.FEED,
    //            INTERACTION_TYPE.DRINK_BLOOD,
    //            INTERACTION_TYPE.EAT,
    //            INTERACTION_TYPE.CARRY,
    //            INTERACTION_TYPE.DROP,
    //            INTERACTION_TYPE.STUMBLE,
    //            INTERACTION_TYPE.ACCIDENT,
    //        } },
    //        { RACE.WOLF, new INTERACTION_TYPE[] {
    //            INTERACTION_TYPE.RELEASE_CHARACTER,
    //            INTERACTION_TYPE.PICK_UP,
    //            INTERACTION_TYPE.EAT,
    //            INTERACTION_TYPE.EAT,
    //            INTERACTION_TYPE.RETURN_HOME,
    //            INTERACTION_TYPE.SLEEP_OUTSIDE,
    //            INTERACTION_TYPE.PLAY,
    //            INTERACTION_TYPE.FEED,
    //            INTERACTION_TYPE.REVERT_TO_NORMAL_FORM,
    //            INTERACTION_TYPE.DRINK_BLOOD,
    //            INTERACTION_TYPE.EAT,
    //            INTERACTION_TYPE.CARRY,
    //            INTERACTION_TYPE.DROP,
    //            INTERACTION_TYPE.STUMBLE,
    //            INTERACTION_TYPE.ACCIDENT,
    //        } },
    //        { RACE.DRAGON, new INTERACTION_TYPE[] {
    //            INTERACTION_TYPE.RELEASE_CHARACTER,
    //            INTERACTION_TYPE.PICK_UP,
    //            INTERACTION_TYPE.EAT,
    //            INTERACTION_TYPE.RETURN_HOME,
    //            INTERACTION_TYPE.SLEEP_OUTSIDE,
    //            INTERACTION_TYPE.PLAY,
    //            INTERACTION_TYPE.FEED,
    //            INTERACTION_TYPE.DRINK_BLOOD,
    //            INTERACTION_TYPE.EAT,
    //            INTERACTION_TYPE.CARRY,
    //            INTERACTION_TYPE.DROP,
    //            INTERACTION_TYPE.STUMBLE,
    //            INTERACTION_TYPE.ACCIDENT,
    //        } },
    //    };
    //}
    //public List<INTERACTION_TYPE> GetNPCInteractionsOfCharacter(Character character) {
    //    List<INTERACTION_TYPE> interactions = new List<INTERACTION_TYPE>(); //Get interactions of all races first
    //    if (_npcRaceInteractions.ContainsKey(character.race)) {
    //        for (int i = 0; i < _npcRaceInteractions[character.race].Length; i++) {
    //            interactions.Add(_npcRaceInteractions[character.race][i]);
    //        }
    //    }
    //    if (character.role.allowedInteractions != null) {
    //        for (int i = 0; i < character.role.allowedInteractions.Length; i++) {
    //            interactions.Add(character.role.allowedInteractions[i]);
    //        }
    //    }
    //    for (int i = 0; i < character.currentInteractionTypes.Count; i++) {
    //        interactions.Add(character.currentInteractionTypes[i]);
    //    }
    //    return interactions;
    //}
    public bool CanCharacterDoGoapAction(Character character, INTERACTION_TYPE goapType) {
        bool isTrue = false;
        Dictionary<INTERACTION_TYPE, GoapAction> goapActionData = InteractionManager.Instance.goapActionData;
        if (goapActionData.ContainsKey(goapType)) {
            isTrue = goapActionData[goapType].DoesCharacterMatchRace(character);
        }
        //if (!isTrue) {
        //    if (character.role.allowedInteractions != null) {
        //        isTrue = character.role.allowedInteractions.Contains(goapType);
        //    }
        //}
        //if (!isTrue) {
        //    isTrue = character.currentInteractionTypes.Contains(goapType);
        //}
        return isTrue;
    }
    #endregion
}
