using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Locations.Settlements;
using Traits;
using UnityEngine;
using Interrupts;

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
            { OBJECT_TYPE.Log, new SaveDataLogHub() },
            { OBJECT_TYPE.Tile_Object, new SaveDataTileObjectHub() },
            { OBJECT_TYPE.Action, new SaveDataActionHub() },
            { OBJECT_TYPE.Interrupt, new SaveDataInterruptHub() },
            { OBJECT_TYPE.Party, new SaveDataPartyHub() },
            { OBJECT_TYPE.Crime, new SaveDataCrimeHub() },
            { OBJECT_TYPE.Character, new SaveDataCharacterHub() },
            { OBJECT_TYPE.Trait, new SaveDataTraitHub() },
            { OBJECT_TYPE.Job, new SaveDataJobHub() },
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
    public void SaveFactions() {
        for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++) {
            Faction faction = FactionManager.Instance.allFactions[i];
            SaveDataFaction saveData = new SaveDataFaction();
            saveData.Save(faction);
            AddToSaveHub(saveData, saveData.objectType);
        }
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
    public void SaveJobs() {
        for (int i = 0; i < DatabaseManager.Instance.jobDatabase.allJobs.Count; i++) {
            JobQueueItem jobQueueItem = DatabaseManager.Instance.jobDatabase.allJobs[i];
            AddToSaveHub(jobQueueItem);
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
    #endregion

    #region Tile Objects
    public void SaveTileObjects(List<TileObject> tileObjects) {
        //tile objects
        List<TileObject> finishedObjects = new List<TileObject>();
        for (int i = 0; i < tileObjects.Count; i++) {
            TileObject tileObject = tileObjects[i];
            // if (tileObject.gridTileLocation == null && tileObject.isBeingCarriedBy == null) {
            //     // Debug.LogWarning($"Grid tile location of {tileObject} is null! Not saving that...");
            //     continue; //skip tile objects without grid tile location that are not being carried.
            // }
            if (finishedObjects.Contains(tileObject)) {
                // Debug.LogWarning($"{tileObject} has a duplicate value in tile object list!");
                continue; //skip    
            }
            SaveDataTileObject saveDataTileObject = CreateNewSaveDataForTileObject(tileObject);
            saveDataTileObject.Save(tileObject);
            AddToSaveHub(saveDataTileObject, saveDataTileObject.objectType);
            //if (tileObject is Artifact artifact) {
            //    string tileObjectTypeName = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(artifact.type.ToString());
            //    SaveDataTileObject saveDataTileObject = createNewSaveDataForArtifact(tileObjectTypeName);
            //    saveDataTileObject.Save(tileObject);
            //    AddToSaveHub(saveDataTileObject, saveDataTileObject.objectType);    
            //} else {
            //    string tileObjectTypeName = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(tileObject.tileObjectType.ToString());
            //    SaveDataTileObject saveDataTileObject = CreateNewSaveDataForTileObject(tileObjectTypeName);
            //    saveDataTileObject.Save(tileObject);
            //    AddToSaveHub(saveDataTileObject, saveDataTileObject.objectType);    
            //}
            if (tileObject.mapObjectState == MAP_OBJECT_STATE.UNBUILT) {
                Debug.Log($"Saved unbuilt object {tileObject}");
            }
            finishedObjects.Add(tileObject);
        }
        finishedObjects.Clear();
        finishedObjects = null;
    }
    public IEnumerator SaveTileObjectsCoroutine() {
        UIManager.Instance.optionsMenu.UpdateSaveMessage("Saving Objects...");
        int batchCount = 0;
        //tile objects
        // HashSet<TileObject> finishedObjects = new HashSet<TileObject>();
        for (int i = 0; i < DatabaseManager.Instance.tileObjectDatabase.allTileObjectsList.Count; i++) {
            TileObject tileObject = DatabaseManager.Instance.tileObjectDatabase.allTileObjectsList[i];
            // if (tileObject.gridTileLocation == null && tileObject.isBeingCarriedBy == null) {
            //     // Debug.LogWarning($"Grid tile location of {tileObject} is null! Not saving that...");
            //     continue; //skip tile objects without grid tile location that are not being carried.
            // }
            // if (finishedObjects.Contains(tileObject)) {
            //     // Debug.LogWarning($"{tileObject} has a duplicate value in tile object list!");
            //     continue; //skip    
            // }
            // finishedObjects.Add(tileObject);
            SaveDataTileObject saveDataTileObject = CreateNewSaveDataForTileObject(tileObject);
            saveDataTileObject.Save(tileObject);
            AddToSaveHub(saveDataTileObject, saveDataTileObject.objectType);

            //if (tileObject is Artifact artifact) {
            //    string tileObjectTypeName = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(artifact.type.ToString());
            //    SaveDataTileObject saveDataTileObject = createNewSaveDataForArtifact(tileObjectTypeName);
            //    saveDataTileObject.Save(tileObject);
            //    AddToSaveHub(saveDataTileObject, saveDataTileObject.objectType);    
            //} else {
            //    string tileObjectTypeName = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(tileObject.tileObjectType.ToString());
            //    SaveDataTileObject saveDataTileObject = CreateNewSaveDataForTileObject(tileObjectTypeName);
            //    saveDataTileObject.Save(tileObject);
            //    AddToSaveHub(saveDataTileObject, saveDataTileObject.objectType);    
            //}
            
            batchCount++;
            if (batchCount >= SaveManager.TileObject_Save_Batches || i + 1 == DatabaseManager.Instance.tileObjectDatabase.allTileObjectsList.Count) {
                batchCount = 0;
                yield return null;    
            }
        }
    }
    public static SaveDataTileObject CreateNewSaveDataForTileObject(TileObject tileObject) {
        //var typeName = $"SaveData{tileObjectTypeString}, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        SaveDataTileObject obj = System.Activator.CreateInstance(tileObject.serializedData) as SaveDataTileObject;
        return obj;
    }
    //public static SaveDataTileObject CreateNewSaveDataForTileObject(string tileObjectTypeString) {
    //    var typeName = $"SaveData{tileObjectTypeString}, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
    //    System.Type type = System.Type.GetType(typeName);
    //    if (type != null) {
    //        SaveDataTileObject obj = System.Activator.CreateInstance(type) as SaveDataTileObject;
    //        return obj;
    //    }
    //    return new SaveDataTileObject(); //if no special save data for tile object was found, then just use the generic one
    //}
    //private SaveDataTileObject createNewSaveDataForArtifact(string tileObjectTypeString) {
    //    var typeName = $"SaveData{tileObjectTypeString}, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
    //    System.Type type = System.Type.GetType(typeName);
    //    if (type != null) {
    //        SaveDataTileObject obj = System.Activator.CreateInstance(type) as SaveDataTileObject;
    //        return obj;
    //    }
    //    return new SaveDataArtifact(); //if no special save data for tile object was found, then just use the generic one
    //}
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
                Dictionary<string, SaveDataJobQueueItem> saveDataTraits = hub.hub;
                foreach (SaveDataJobQueueItem data in saveDataTraits.Values) {
                    data.Load();
                }
            }
        }
    }
    public void LoadActions() {
        if (objectHub.ContainsKey(OBJECT_TYPE.Action)) {
            if (objectHub[OBJECT_TYPE.Action] is SaveDataActionHub hub) {
                Dictionary<string, SaveDataActualGoapNode> saved = hub.hub;
                foreach (SaveDataActualGoapNode data in saved.Values) {
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
    public void LoadLogs() {
        if (objectHub.ContainsKey(OBJECT_TYPE.Log)) {
            if (objectHub[OBJECT_TYPE.Log] is SaveDataLogHub hub) {
                Dictionary<string, SaveDataLog> saved = hub.hub;
                foreach (SaveDataLog data in saved.Values) {
                    Log log = data.Load();
                    DatabaseManager.Instance.logDatabase.AddLog(log);
                }
            }
        }
    }
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
    public Player LoadPlayer() {
        return playerSave.Load();
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
        foreach (KeyValuePair<string, ActualGoapNode> item in DatabaseManager.Instance.actionDatabase.allActions) {
            SaveDataActualGoapNode saveData = GetFromSaveHub<SaveDataActualGoapNode>(OBJECT_TYPE.Action, item.Key);
            item.Value.LoadReferences(saveData);
        }
    }
    public void LoadInterruptReferences() {
        foreach (KeyValuePair<string, InterruptHolder> item in DatabaseManager.Instance.interruptDatabase.allInterrupts) {
            SaveDataInterruptHolder saveData = GetFromSaveHub<SaveDataInterruptHolder>(OBJECT_TYPE.Interrupt, item.Key);
            item.Value.LoadReferences(saveData);
        }
    }
    public void LoadLogReferences() {
        foreach (KeyValuePair<string, Log> item in DatabaseManager.Instance.logDatabase.allLogs) {
            SaveDataLog saveData = GetFromSaveHub<SaveDataLog>(OBJECT_TYPE.Log, item.Key);
            item.Value.LoadReferences(saveData);
        }
    }
    public void LoadPartyReferences() {
        foreach (KeyValuePair<string, Party> item in DatabaseManager.Instance.partyDatabase.allParties) {
            SaveDataParty saveData = GetFromSaveHub<SaveDataParty>(OBJECT_TYPE.Party, item.Key);
            item.Value.LoadReferences(saveData);
        }
    }
    public void LoadCrimeReferences() {
        foreach (KeyValuePair<string, CrimeData> item in DatabaseManager.Instance.crimeDatabase.allCrimes) {
            SaveDataCrimeData saveData = GetFromSaveHub<SaveDataCrimeData>(OBJECT_TYPE.Crime, item.Key);
            item.Value.LoadReferences(saveData);
        }
    }
    public void LoadTraitsSecondWave() {
        foreach (KeyValuePair<string, Trait> item in DatabaseManager.Instance.traitDatabase.traitsByGUID) {
            SaveDataTrait saveData = GetFromSaveHub<SaveDataTrait>(OBJECT_TYPE.Trait, item.Key);
            item.Value.LoadSecondWaveInstancedTrait(saveData);
        }
        //if (objectHub.ContainsKey(OBJECT_TYPE.Trait)){
        //    if(objectHub[OBJECT_TYPE.Trait] is SaveDataTraitHub hub) {
        //        Dictionary<string, SaveDataTrait> saveDataTraits = hub.hub;
        //        foreach (SaveDataTrait data in saveDataTraits.Values) {
        //            Trait trait = DatabaseManager.Instance.traitDatabase.GetTraitByPersistentID(data.persistentID);
        //            trait.LoadSecondWaveInstancedTrait(data);
        //        }
        //    }
        //}
    }
    #endregion

    #region Clean Up
    public void CleanUp() {
        worldMapSave?.CleanUp();
        worldSettingsData = null;
        familyTreeDatabase = null;
        playerSave?.CleanUp();
        objectHub?.Clear();
        objectHub = null;
    }
    #endregion
}

[System.Serializable]
public class SaveDataNotification {
    public SaveDataLog log;
    public int tickShown;

    public void Save(PlayerNotificationItem notif) {
        log = new SaveDataLog();
        log.Save(notif.shownLog);

        tickShown = notif.tickShown;
    }

    public void Load() {
        UIManager.Instance.ShowPlayerNotification(log.Load(), tickShown);
    }
}