using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Locations.Settlements;
using Traits;
using UnityEngine;
using Interrupts;
using Object_Pools;
using Quests;
using UnityEngine.Assertions;
using UtilityScripts;

public class SaveDataCurrentProgress {
    public System.DateTime timeStamp;
    public string gameVersion;

    public int month;
    public int day;
    public int year;
    public int tick;
    public int continuousDays;

    public WorldMapSave worldMapSave;

    //World Settings
    public WorldSettingsData worldSettingsData;

    //family trees
    public FamilyTreeDatabase familyTreeDatabase;
    
    //Player
    public SaveDataPlayerGame playerSave;

    //Plague
    public bool hasPlagueDisease;
    public SaveDataPlagueDisease savedPlagueDisease;
    
    //Win Conditions
    public SaveDataWinConditionTracker saveDataWinConditionTracker;

    //Pool of all saved objects
    public Dictionary<OBJECT_TYPE, BaseSaveDataHub> objectHub;

    #region General
    public void Initialize() {
        timeStamp = System.DateTime.Now;
        gameVersion = Application.version;
        if (objectHub == null) {
            ConstructObjectHub();
        }
    }
    private void ConstructObjectHub() {
        objectHub = new Dictionary<OBJECT_TYPE, BaseSaveDataHub>() {
            { OBJECT_TYPE.Faction, new SaveDataFactionHub() },
            // { OBJECT_TYPE.Log, new SaveDataLogHub() },
            { OBJECT_TYPE.Tile_Object, new SaveDataTileObjectHub() },
            { OBJECT_TYPE.Action, new SaveDataActionHub() },
            { OBJECT_TYPE.Interrupt, new SaveDataInterruptHub() },
            { OBJECT_TYPE.Party, new SaveDataPartyHub() },
            { OBJECT_TYPE.Party_Quest, new SaveDataPartyQuestHub() },
            { OBJECT_TYPE.Crime, new SaveDataCrimeHub() },
            { OBJECT_TYPE.Character, new SaveDataCharacterHub() },
            { OBJECT_TYPE.Trait, new SaveDataTraitHub() },
            { OBJECT_TYPE.Job, new SaveDataJobHub() },
            { OBJECT_TYPE.Gathering, new SaveDataGatheringHub() },
            { OBJECT_TYPE.Reaction_Quest, new SaveDataReactionQuestHub() },
        };
    }
    #endregion

    #region Hub
    public bool AddToSaveHub<T>(T data) where T : ISavable {
        if (objectHub.ContainsKey(data.objectType)) {
            if (objectHub[data.objectType].GetData(data.persistentID) == null) {
                //only save data if hub doesn't already have the saved data.
                SaveData<T> obj = (SaveData<T>) System.Activator.CreateInstance(data.serializedData);
                obj.Save(data);
                return AddToSaveHub(obj, data.objectType);        
            }
            return false;
        }
        throw new System.NullReferenceException("Trying to add object type " + data.objectType.ToString() + " in Object Hub but there is no entry for it. Make sure you add it in ConstructObjectHub");
    }
    private bool AddToSaveHub<T>(T data, OBJECT_TYPE objectType) {
        if (objectHub.ContainsKey(objectType)) { //The object type must always be present in the object hub dictionary if it is not, add it in ConstructObjectHub
            return objectHub[objectType].AddToSave(data);
        } else {
            throw new System.NullReferenceException("Trying to add object type " + objectType.ToString() + " in Object Hub but there is no entry for it. Make sure you add it in ConstructObjectHub");
        }
    }
    private bool RemoveFromSaveHub<T>(T data, OBJECT_TYPE objectType) {
        if (objectHub.ContainsKey(objectType)) { //The object type must always be present in the object hub dictionary if it is not, add it in ConstructObjectHub
            return objectHub[objectType].RemoveFromSave(data);
        } else {
            throw new System.NullReferenceException("Trying to remove object type " + objectType.ToString() + " in Object Hub but there is no entry for it. Make sure you add it in ConstructObjectHub");
        }
    }
    public T GetFromSaveHub<T>(OBJECT_TYPE objectType, string persistenID) {
        if (objectHub.ContainsKey(objectType)) {
            return (T) objectHub[objectType].GetData(persistenID);
        } else {
            throw new System.NullReferenceException("Trying to get object type " + objectType.ToString() + " in Object Hub but there is no entry for it. Make sure you add it in ConstructObjectHub");
        }
    }
    #endregion

    #region Saving
    public void SaveDate() {
        GameDate today = GameManager.Instance.Today();
        month = today.month;
        day = today.day;
        year = today.year;
        tick = today.tick;
        continuousDays = GameManager.Instance.continuousDays;
    }
    public void SaveWorldSettings() {
        worldSettingsData = WorldSettings.Instance.worldSettingsData;
    }
    public void SavePlayer() {
        playerSave = new SaveDataPlayerGame();
        playerSave.Save();
    }
    public IEnumerator SaveFactionsCoroutine() {
        UIManager.Instance.optionsMenu.UpdateSaveMessage("Saving factions...");
        for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++) {
            Faction faction = FactionManager.Instance.allFactions[i];
            SaveDataFaction saveData = new SaveDataFaction();
            saveData.Save(faction);
            AddToSaveHub(saveData, saveData.objectType);
            yield return null;
        }
    }
    public void SaveFactions() {
        for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++) {
            Faction faction = FactionManager.Instance.allFactions[i];
            SaveDataFaction saveData = new SaveDataFaction();
            saveData.Save(faction);
            AddToSaveHub(saveData, saveData.objectType);
        }
    }
    public IEnumerator SaveCharactersCoroutine() {
        UIManager.Instance.optionsMenu.UpdateSaveMessage("Saving characters...");
        int batchCount = 0;
        for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
            Character character = CharacterManager.Instance.allCharacters[i];
            SaveDataCharacter saveData = CharacterManager.Instance.CreateNewSaveDataCharacter(character);
            AddToSaveHub(saveData, saveData.objectType);
            batchCount++;
            if (batchCount >= SaveManager.Character_Save_Batches) {
                batchCount = 0;
                yield return null;    
            }
        }
        
        batchCount = 0;
        for (int i = 0; i < CharacterManager.Instance.limboCharacters.Count; i++) {
            Character character = CharacterManager.Instance.limboCharacters[i];
            SaveDataCharacter saveData = CharacterManager.Instance.CreateNewSaveDataCharacter(character);
            AddToSaveHub(saveData, saveData.objectType);
            batchCount++;
            if (batchCount >= SaveManager.Character_Save_Batches) {
                batchCount = 0;
                yield return null;    
            }
        }
    }
    public void SaveCharacters() {
        for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
            Character character = CharacterManager.Instance.allCharacters[i];
            SaveDataCharacter saveData = CharacterManager.Instance.CreateNewSaveDataCharacter(character);
            AddToSaveHub(saveData, saveData.objectType);
        }

        for (int i = 0; i < CharacterManager.Instance.limboCharacters.Count; i++) {
            Character character = CharacterManager.Instance.limboCharacters[i];
            SaveDataCharacter saveData = CharacterManager.Instance.CreateNewSaveDataCharacter(character);
            AddToSaveHub(saveData, saveData.objectType);
        }
    }
    public IEnumerator SaveJobsCoroutine() {
        UIManager.Instance.optionsMenu.UpdateSaveMessage("Saving jobs...");
        int batchCount = 0;
        for (int i = 0; i < DatabaseManager.Instance.jobDatabase.allJobs.Count; i++) {
            JobQueueItem jobQueueItem = DatabaseManager.Instance.jobDatabase.allJobs[i];
            if (jobQueueItem.jobType == JOB_TYPE.NONE) {
                continue; //skip
            }
            AddToSaveHub(jobQueueItem);
            batchCount++;
            if (batchCount >= SaveManager.Job_Save_Batches) {
                batchCount = 0;
                yield return null;    
            }
        }
    }
    public void SaveJobs() {
        for (int i = 0; i < DatabaseManager.Instance.jobDatabase.allJobs.Count; i++) {
            JobQueueItem jobQueueItem = DatabaseManager.Instance.jobDatabase.allJobs[i];
            if (jobQueueItem.jobType == JOB_TYPE.NONE) {
                continue; //skip
            }
            AddToSaveHub(jobQueueItem);
        }
    }
    public IEnumerator SaveReactionQuestsCoroutine() {
        UIManager.Instance.optionsMenu.UpdateSaveMessage("Saving quests...");
        int batchCount = 0;
        for (int i = 0; i < QuestManager.Instance.activeQuests.Count; i++) {
            Quest quest = QuestManager.Instance.activeQuests[i];
            if (quest is ReactionQuest reactionQuest) {
                AddToSaveHub(reactionQuest);
                batchCount++;
                if (batchCount >= SaveManager.Reaction_Quest_Save_Batches) {
                    batchCount = 0;
                    yield return null;    
                }   
            }
        }
    }
    public void SaveReactionQuests() {
        for (int i = 0; i < QuestManager.Instance.activeQuests.Count; i++) {
            Quest quest = QuestManager.Instance.activeQuests[i];
            if (quest is ReactionQuest reactionQuest) {
                AddToSaveHub(reactionQuest);
            }
        }
    }
    public void SavePlagueDisease() {
        hasPlagueDisease = PlagueDisease.HasInstance();
        if (hasPlagueDisease) {
            savedPlagueDisease = new SaveDataPlagueDisease();
            savedPlagueDisease.Save();
        }
    }
    public void SaveWinConditionTracker() {
        if (QuestManager.Instance.winConditionTracker != null) {
            saveDataWinConditionTracker = CreateNewSaveDataForWinConditionTracker(QuestManager.Instance.winConditionTracker);
            saveDataWinConditionTracker.Save(QuestManager.Instance.winConditionTracker);    
        }
    }
    private SaveDataWinConditionTracker CreateNewSaveDataForWinConditionTracker(WinConditionTracker winConditionTracker) {
        SaveDataWinConditionTracker obj = System.Activator.CreateInstance(winConditionTracker.serializedData) as SaveDataWinConditionTracker;
        return obj;
    }
    #endregion

    #region Tile Objects
    public IEnumerator SaveTileObjectsCoroutine() {
        yield return null;
        //UIManager.Instance.optionsMenu.UpdateSaveMessage("Saving Objects...");
        //int batchCount = 0;

        //HashSet<TileObject> allTileObjects = DatabaseManager.Instance.tileObjectDatabase.allTileObjectsList;
        //for (int i = 0; i < allTileObjects.Count; i++) {
        //    TileObject tileObject = allTileObjects.ElementAt(i);
        //    if (!SaveTileObject(tileObject)) {
        //        continue;
        //    }
        //    batchCount++;
        //    if (batchCount >= SaveManager.TileObject_Save_Batches) {
        //        batchCount = 0;
        //        yield return null;    
        //    }
        //}

        ////We copy the destroyedTileObjects in a separate list so that we wont have race conditions if it is still processing in the multithread
        //List<WeakReference> copyOfDestroyedTileObjects = RuinarchListPool<WeakReference>.Claim();
        //copyOfDestroyedTileObjects.AddRange(DatabaseManager.Instance.tileObjectDatabase.destroyedTileObjects);
        //for (int i = 0; i < copyOfDestroyedTileObjects.Count; i++) {
        //    WeakReference wr = copyOfDestroyedTileObjects[i];
        //    if (!wr.IsAlive) {
        //        continue;
        //    }
        //    TileObject t = wr.Target as TileObject;
        //    if (t != null) {
        //        if (!SaveTileObject(t)) {
        //            continue;
        //        }
        //    }
        //    batchCount++;
        //    if (batchCount >= SaveManager.TileObject_Save_Batches) {
        //        batchCount = 0;
        //        yield return null;
        //    }
        //}
        //RuinarchListPool<WeakReference>.Release(copyOfDestroyedTileObjects);
    }
    public void SaveTileObjects(List<TileObject> allTileObjects) {
        for (int i = 0; i < allTileObjects.Count; i++) {
            TileObject tileObject = allTileObjects[i];
            SaveTileObject(tileObject);
        }
    }
    public void SaveGenericTileObjects(List<TileObject> allTileObjects) {
        for (int i = 0; i < allTileObjects.Count; i++) {
            GenericTileObject tileObject = allTileObjects[i] as GenericTileObject;
            if (!SaveGenericTileObject(tileObject)) {
                continue;
            }
        }
    }

    public void SaveDestroyedTileObjects() {
        //We copy the destroyedTileObjects in a separate list so that we wont have race conditions if it is still processing in the multithread
        List<WeakReference> copyOfDestroyedTileObjects = RuinarchListPool<WeakReference>.Claim();
        copyOfDestroyedTileObjects.AddRange(DatabaseManager.Instance.tileObjectDatabase.destroyedTileObjects);
        for (int i = 0; i < copyOfDestroyedTileObjects.Count; i++) {
            WeakReference wr = copyOfDestroyedTileObjects[i];
            if (!wr.IsAlive) {
                continue;
            }
            TileObject t = wr.Target as TileObject;
            if (t != null) {
                if (!SaveDestroyedTileObject(t)) {
                    continue;
                }
            }
        }
        RuinarchListPool<WeakReference>.Release(copyOfDestroyedTileObjects);
    }
    private void SaveTileObject(TileObject tileObject) {
        lock (SaveCurrentProgressManager.THREAD_LOCKER) {
            SaveDataTileObject saveDataTileObject = CreateNewSaveDataForTileObject(tileObject);
            saveDataTileObject.Save(tileObject);
            AddToSaveHub(saveDataTileObject, saveDataTileObject.objectType);
        }

        //Wag na isave dito yung kapartner na wurm hole kasi dadaanan din yun since nasa database din naman sya
        //if (tileObject is WurmHole wurmHole) {
        //    //special case for wurm hole because connected wurm hole cannot be saved inside other wurm hole because it will produce a stack overflow exception
        //    SaveDataTileObject otherWurmHoleSaveData = CreateNewSaveDataForTileObject(wurmHole.wurmHoleConnection);
        //    otherWurmHoleSaveData.Save(wurmHole.wurmHoleConnection);
        //    SaveManager.Instance.saveCurrentProgressManager.currentSaveDataProgress.AddToSaveHub(otherWurmHoleSaveData, otherWurmHoleSaveData.objectType);
        //}
    }
    private bool SaveGenericTileObject(GenericTileObject tileObject) {
        if (tileObject.gridTileLocation.isDefault) {
            //if tile object is a Generic Tile Object and its parent tile is set as default then do not save it.
            return false;
        }
        lock (SaveCurrentProgressManager.THREAD_LOCKER) {
            SaveDataTileObject saveDataTileObject = CreateNewSaveDataForTileObject(tileObject);
            saveDataTileObject.Save(tileObject);
            AddToSaveHub(saveDataTileObject, saveDataTileObject.objectType);
        }
        return true;
    }
    private bool SaveDestroyedTileObject(TileObject tileObject) {
        if (tileObject is GenericTileObject genericTileObject && genericTileObject.gridTileLocation.isDefault) {
            //if tile object is a Generic Tile Object and its parent tile is set as default then do not save it.
            return false;
        }
        lock (SaveCurrentProgressManager.THREAD_LOCKER) {
            SaveDataTileObject saveDataTileObject = CreateNewSaveDataForTileObject(tileObject);
            saveDataTileObject.Save(tileObject);
            AddToSaveHub(saveDataTileObject, saveDataTileObject.objectType);
        }
        return true;
    }
    private static SaveDataTileObject CreateNewSaveDataForTileObject(TileObject tileObject) {
        SaveDataTileObject obj = System.Activator.CreateInstance(tileObject.serializedData) as SaveDataTileObject;
        return obj;
    }
    #endregion

    #region First Wave Loading
    //FIRST WAVE LOADING - this is always the firsts to load, this loading does not require references from others and thus, only needs itself to load
    //This typically populates data in the databases of objects
    public void LoadDate() {
        GameDate today = GameManager.Instance.Today();
        today.day = day;
        today.month = month;
        today.year = year;
        today.tick = tick;
        GameManager.Instance.continuousDays = continuousDays;
        GameManager.Instance.SetToday(today);
    }
    public void LoadFactions() {
        if (objectHub.ContainsKey(OBJECT_TYPE.Faction)){
            if(objectHub[OBJECT_TYPE.Faction] is SaveDataFactionHub factionHub) {
                Dictionary<string, SaveDataFaction> saved = factionHub.hub;
                foreach (SaveDataFaction data in saved.Values) {
                    data.Load();
                }
            }
        }
    }
    public void LoadTileObjects() {
        if (objectHub.ContainsKey(OBJECT_TYPE.Tile_Object)){
            if(objectHub[OBJECT_TYPE.Tile_Object] is SaveDataTileObjectHub hub) {
                Dictionary<string, SaveDataTileObject> saveDataTileObjects = hub.hub;
                foreach (SaveDataTileObject data in saveDataTileObjects.Values) {
                    //Special Case: Do not load generic tile objects here, since they were already loaded during region inner map generation.
                    if (data.tileObjectType != TILE_OBJECT_TYPE.GENERIC_TILE_OBJECT) {
                        data.Load();
                    }
                }
            }
        }
    }
    public void LoadTraits() {
        if (objectHub.ContainsKey(OBJECT_TYPE.Trait)){
            if(objectHub[OBJECT_TYPE.Trait] is SaveDataTraitHub hub) {
                Dictionary<string, SaveDataTrait> saveDataTraits = hub.hub;
                foreach (SaveDataTrait data in saveDataTraits.Values) {
                    data.Load();
                }
            }
        }
    }
    public void LoadJobs() {
        if (objectHub.ContainsKey(OBJECT_TYPE.Job)){
            if(objectHub[OBJECT_TYPE.Job] is SaveDataJobHub hub) {
                Dictionary<string, SaveDataJobQueueItem> saveData = hub.hub;
                foreach (SaveDataJobQueueItem data in saveData.Values) {
                    data.Load();
                }
            }
        }
    }
    public void LoadActions() {
        if (objectHub.ContainsKey(OBJECT_TYPE.Action)) {
            if (objectHub[OBJECT_TYPE.Action] is SaveDataActionHub hub) {
                Dictionary<string, SaveDataActualGoapNode> saveData = hub.hub;
                foreach (SaveDataActualGoapNode data in saveData.Values) {
                    ActualGoapNode action = data.Load();
                    DatabaseManager.Instance.actionDatabase.AddAction(action);
                }
            }
        }
    }
    public void LoadInterrupts() {
        if (objectHub.ContainsKey(OBJECT_TYPE.Interrupt)) {
            if (objectHub[OBJECT_TYPE.Interrupt] is SaveDataInterruptHub hub) {
                Dictionary<string, SaveDataInterruptHolder> saved = hub.hub;
                foreach (SaveDataInterruptHolder data in saved.Values) {
                    InterruptHolder interrupt = data.Load();
                    DatabaseManager.Instance.interruptDatabase.AddInterrupt(interrupt);
                }
            }
        }
    }
    // public void LoadLogs() {
    //     if (objectHub.ContainsKey(OBJECT_TYPE.Log)) {
    //         if (objectHub[OBJECT_TYPE.Log] is SaveDataLogHub hub) {
    //             Dictionary<string, SaveDataLog> saved = hub.hub;
    //             foreach (SaveDataLog data in saved.Values) {
    //                 Log log = data.Load();
    //                 DatabaseManager.Instance.logDatabase.AddLog(log);
    //             }
    //         }
    //     }
    // }
    public void LoadParties() {
        if (objectHub.ContainsKey(OBJECT_TYPE.Party)) {
            if (objectHub[OBJECT_TYPE.Party] is SaveDataPartyHub hub) {
                Dictionary<string, SaveDataParty> saved = hub.hub;
                foreach (SaveDataParty data in saved.Values) {
                    Party party = data.Load();
                    DatabaseManager.Instance.partyDatabase.AddParty(party);
                }
            }
        }
    }
    public void LoadPartyQuests() {
        if (objectHub.ContainsKey(OBJECT_TYPE.Party_Quest)) {
            if (objectHub[OBJECT_TYPE.Party_Quest] is SaveDataPartyQuestHub hub) {
                Dictionary<string, SaveDataPartyQuest> saved = hub.hub;
                foreach (SaveDataPartyQuest data in saved.Values) {
                    PartyQuest quest = data.Load();
                    DatabaseManager.Instance.partyQuestDatabase.AddPartyQuest(quest);
                }
            }
        }
    }
    public void LoadCrimes() {
        if (objectHub.ContainsKey(OBJECT_TYPE.Crime)) {
            if (objectHub[OBJECT_TYPE.Crime] is SaveDataCrimeHub hub) {
                Dictionary<string, SaveDataCrimeData> saved = hub.hub;
                foreach (SaveDataCrimeData data in saved.Values) {
                    CrimeData crime = data.Load();
                    DatabaseManager.Instance.crimeDatabase.AddCrime(crime);
                }
            }
        }
    }
    public void LoadCharacters() {
        if (objectHub.ContainsKey(OBJECT_TYPE.Character)) {
            if (objectHub[OBJECT_TYPE.Character] is SaveDataCharacterHub hub) {
                Dictionary<string, SaveDataCharacter> saved = hub.hub;
                foreach (SaveDataCharacter data in saved.Values) {
                    Character character = data.Load();
                }
            }
        }
    }
    public void LoadGatherings() {
        if (objectHub.ContainsKey(OBJECT_TYPE.Gathering)) {
            if (objectHub[OBJECT_TYPE.Gathering] is SaveDataGatheringHub hub) {
                Dictionary<string, SaveDataGathering> saved = hub.hub;
                foreach (SaveDataGathering data in saved.Values) {
                    Gathering gathering = data.Load();
                    DatabaseManager.Instance.gatheringDatabase.AddGathering(gathering);
                }
            }
        }
    }
    public Player LoadPlayer() {
        return playerSave.Load();
    }
    public void LoadPlagueDisease() {
        if (hasPlagueDisease) {
            new PlagueDisease(savedPlagueDisease);
        }
    }
    #endregion

    #region Second Wave Loading
    //SECOND WAVE LOADING - This loading should always be after the FIRST WAVE LOADING, almost all this requires references from other objects and thus, the object must be initiated first before any of these functions are called
    //Initiation of objects are done in FIRST WAVE LOADING
    public void LoadFactionReferences() {
        for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++) {
            Faction faction = FactionManager.Instance.allFactions[i];
            SaveDataFaction saveData = GetFromSaveHub<SaveDataFaction>(OBJECT_TYPE.Faction, faction.persistentID);
            faction.LoadReferences(saveData);
        }
    }
    public void LoadPlayerReferences() {
        PlayerManager.Instance.player.LoadReferences(playerSave);
    }
    public void LoadCharacterReferences() {
        for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
            Character character = CharacterManager.Instance.allCharacters[i];
            SaveDataCharacter saveData = GetFromSaveHub<SaveDataCharacter>(OBJECT_TYPE.Character, character.persistentID);
            character.LoadReferences(saveData);
        }
        for (int i = 0; i < CharacterManager.Instance.limboCharacters.Count; i++) {
            Character character = CharacterManager.Instance.limboCharacters[i];
            SaveDataCharacter saveData = GetFromSaveHub<SaveDataCharacter>(OBJECT_TYPE.Character, character.persistentID);
            character.LoadReferences(saveData);
        }
    }
    public void LoadActionReferences() {
        //List<string> allCorruptedActions = RuinarchListPool<string>.Claim();
        foreach (KeyValuePair<string, ActualGoapNode> item in DatabaseManager.Instance.actionDatabase.allActions) {
            SaveDataActualGoapNode saveData = GetFromSaveHub<SaveDataActualGoapNode>(OBJECT_TYPE.Action, item.Key);
            if (!item.Value.LoadReferences(saveData)) {
                //allCorruptedActions.Add(item.Key);
            }
        }
        //for (int i = 0; i < allCorruptedActions.Count; i++) {
        //    DatabaseManager.Instance.actionDatabase.RemoveAction(allCorruptedActions[i]);
        //}
        //RuinarchListPool<string>.Release(allCorruptedActions);
    }
    public void LoadAdditionalActionReferences() {
        foreach (KeyValuePair<string, ActualGoapNode> item in DatabaseManager.Instance.actionDatabase.allActions) {
            SaveDataActualGoapNode saveData = GetFromSaveHub<SaveDataActualGoapNode>(OBJECT_TYPE.Action, item.Key);
            item.Value.LoadAdditionalReferences(saveData);
        }
    }
    public void LoadInterruptReferences() {
        //List<string> allCorruptedInterrupts = RuinarchListPool<string>.Claim();
        foreach (KeyValuePair<string, InterruptHolder> item in DatabaseManager.Instance.interruptDatabase.allInterrupts) {
            SaveDataInterruptHolder saveData = GetFromSaveHub<SaveDataInterruptHolder>(OBJECT_TYPE.Interrupt, item.Key);
            if (!item.Value.LoadReferences(saveData)) {
                //allCorruptedInterrupts.Add(item.Key);
            }
        }
        //for (int i = 0; i < allCorruptedInterrupts.Count; i++) {
        //    DatabaseManager.Instance.interruptDatabase.RemoveInterrupt(allCorruptedInterrupts[i]);
        //}
        //RuinarchListPool<string>.Release(allCorruptedInterrupts);
    }
    // public void LoadLogReferences() {
    //     foreach (KeyValuePair<string, Log> item in DatabaseManager.Instance.logDatabase.allLogs) {
    //         SaveDataLog saveData = GetFromSaveHub<SaveDataLog>(OBJECT_TYPE.Log, item.Key);
    //         item.Value.LoadReferences(saveData);
    //     }
    // }
    public void LoadPartyReferences() {
        foreach (KeyValuePair<string, Party> item in DatabaseManager.Instance.partyDatabase.allParties) {
            SaveDataParty saveData = GetFromSaveHub<SaveDataParty>(OBJECT_TYPE.Party, item.Key);
            item.Value.LoadReferences(saveData);
        }
    }
    public void LoadPartyQuestReferences() {
        foreach (KeyValuePair<string, PartyQuest> item in DatabaseManager.Instance.partyQuestDatabase.allPartyQuests) {
            SaveDataPartyQuest saveData = GetFromSaveHub<SaveDataPartyQuest>(OBJECT_TYPE.Party_Quest, item.Key);
            item.Value.LoadReferences(saveData);
        }
    }
    public void LoadCrimeReferences() {
        foreach (KeyValuePair<string, CrimeData> item in DatabaseManager.Instance.crimeDatabase.allCrimes) {
            SaveDataCrimeData saveData = GetFromSaveHub<SaveDataCrimeData>(OBJECT_TYPE.Crime, item.Key);
            item.Value.LoadReferences(saveData);
        }
    }
    public void LoadGatheringReferences() {
        foreach (KeyValuePair<string, Gathering> item in DatabaseManager.Instance.gatheringDatabase.allGatherings) {
            SaveDataGathering saveData = GetFromSaveHub<SaveDataGathering>(OBJECT_TYPE.Gathering, item.Key);
            item.Value.LoadReferences(saveData);
        }
    }
    public void LoadTraitsSecondWave() {
        foreach (KeyValuePair<string, Trait> item in DatabaseManager.Instance.traitDatabase.traitsByGUID) {
            SaveDataTrait saveData = GetFromSaveHub<SaveDataTrait>(OBJECT_TYPE.Trait, item.Key);
            if (saveData != null) {
                item.Value.LoadSecondWaveInstancedTrait(saveData);    
            }
        }
    }
    public void LoadCharactersCurrentAction() {
        for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
            Character character = CharacterManager.Instance.allCharacters[i];
            character.LoadCurrentlyDoingAction();
        }
    }
    public void LoadReactionQuests() {
        if (objectHub.ContainsKey(OBJECT_TYPE.Reaction_Quest)){
            if(objectHub[OBJECT_TYPE.Reaction_Quest] is SaveDataReactionQuestHub hub) {
                Dictionary<string, SaveDataReactionQuest> saveDataTileObjects = hub.hub;
                foreach (SaveDataReactionQuest data in saveDataTileObjects.Values) {
                    ReactionQuest reactionQuest = data.Load();
                    QuestManager.Instance.ActivateQuest(reactionQuest);
                }
            }
        }
    }
    public void LoadWinConditionTracker() {
        QuestManager.Instance.LoadWinConditionTracker(saveDataWinConditionTracker);
    }
    #endregion

    #region Clean Up
    public void CleanUp() {
        familyTreeDatabase = null;
        playerSave?.CleanUp();
        objectHub?.Clear();
        objectHub = null;
    }
    #endregion
}

[System.Serializable]
public class SaveDataNotification {
    public string logID;
    public int tickShown;

    public bool isIntel;
    public bool isActionIntel;
    public SaveDataActionIntel actionIntel;
    public SaveDataInterruptIntel interruptIntel;

    public void Save(PlayerNotificationItem notif) {
        logID = notif.logPersistentID;
        
        Log log = DatabaseManager.Instance.mainSQLDatabase.GetLogWithPersistentID(logID);
        if (log == null) {
            Debug.LogError($"Log with id {logID} is not present in database. Log is {notif.currentTextDisplayed}");
            return;
        }
        // logID = notif.shownLog.persistentID;
        // tickShown = notif.tickShown;
        // SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(notif.shownLog);
        // if (notif.shownLog.node != null) {
        //     SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(notif.shownLog.node);
        // }

        if(notif is IntelNotificationItem intelNotif) {
            isIntel = true;
            if(intelNotif.intel is ActionIntel action) {
                isActionIntel = true;
                actionIntel = new SaveDataActionIntel();
                actionIntel.Save(action);
            } else if (intelNotif.intel is InterruptIntel interrupt) {
                isActionIntel = false;
                interruptIntel = new SaveDataInterruptIntel();
                interruptIntel.Save(interrupt);
            }
        }
    }

    public void Load() {
        Log log = DatabaseManager.Instance.mainSQLDatabase.GetLogWithPersistentID(logID);
        if (isIntel) {
            IIntel intel = null;
            if (isActionIntel) {
                ActualGoapNode node = DatabaseManager.Instance.actionDatabase.GetActionByPersistentID(actionIntel.node);
                intel = new ActionIntel(node);
            } else {
                InterruptHolder interrupt = DatabaseManager.Instance.interruptDatabase.GetInterruptByPersistentID(interruptIntel.interruptHolder);
                intel = new InterruptIntel(interrupt);
            }
            Assert.IsNotNull(intel, $"Intel Notification for loading is null!");
            UIManager.Instance.ShowPlayerNotification(intel, log, tickShown);
        } else {
            UIManager.Instance.ShowPlayerNotification(log, tickShown);
        }
        if (log != null) {
            LogPool.Release(log);
        }
    }
}