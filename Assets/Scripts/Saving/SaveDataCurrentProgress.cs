using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Locations.Settlements;
using UnityEngine;

public class SaveDataCurrentProgress {
    //public int width;
    //public int height;
    //public int borderThickness;
    //public List<SaveDataHextile> hextileSaves;
    //public List<SaveDataHextile> outerHextileSaves;
    //public List<SaveDataLandmark> landmarkSaves;
    //public List<SaveDataRegion> regionSaves;
    //public List<SaveDataArea> nonPlayerAreaSaves;
    
    //public List<SaveDataCharacter> characterSaves;
    //public List<SaveDataTileObject> tileObjectSaves;
    //// public List<SaveDataSpecialObject> specialObjectSaves;
    //// public List<SaveDataAreaInnerTileMap> areaMapSaves;
    //public List<SaveDataNotification> notificationSaves;

    //public SaveDataArea playerAreaSave;
    //public SaveDataPlayer playerSave;

    public int month;
    public int day;
    public int year;
    public int tick;
    public int continuousDays;

    public WorldMapSave worldMapSave;

    //Player
    public SaveDataPlayerGame playerSave;

    //Pool of all saved objects
    public Dictionary<OBJECT_TYPE, BaseSaveDataHub> objectHub;

    #region General
    public void Initialize() {
        if (objectHub == null) {
            ConstructObjectHub();
        }
    }
    private void ConstructObjectHub() {
        objectHub = new Dictionary<OBJECT_TYPE, BaseSaveDataHub>() {
            { OBJECT_TYPE.Faction, new SaveDataFactionHub() },
            { OBJECT_TYPE.Log, new SaveDataLogHub() },
            { OBJECT_TYPE.Tile_Object, new SaveDataTileObjectHub() },
        };
    }
    #endregion

    #region Hub
    public bool AddToSaveHub<T>(T data) where T : ISavable {
        if(data is ISavable savable) {
            SaveData<T> obj = (SaveData<T>) System.Activator.CreateInstance(data.serializedData);
            obj.Save(data);
            return AddToSaveHub(obj, savable.objectType);
        }
        return false;
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
    #endregion
    
    #region Tile Objects
    public void SaveTileObjects(List<TileObject> tileObjects) {
        //tile objects
        List<TileObject> finishedObjects = new List<TileObject>();
        for (int i = 0; i < tileObjects.Count; i++) {
            TileObject tileObject = tileObjects[i];
            if (tileObject.gridTileLocation == null && tileObject.isBeingCarriedBy == null) {
                // Debug.LogWarning($"Grid tile location of {tileObject} is null! Not saving that...");
                continue; //skip tile objects without grid tile location that are not being carried.
            }
            if (finishedObjects.Contains(tileObject)) {
                // Debug.LogWarning($"{tileObject} has a duplicate value in tile object list!");
                continue; //skip    
            }
            if (tileObject is GenericTileObject) {
                continue; //do not place save data of generic tile objects here, since they are loaded alongside their respective LocationGridTiles  
            }
            if (tileObject is Artifact artifact) {
                string tileObjectTypeName = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(artifact.type.ToString());
                SaveDataTileObject saveDataTileObject = createNewSaveDataForArtifact(tileObjectTypeName);
                saveDataTileObject.Save(tileObject);
                AddToSaveHub(saveDataTileObject, saveDataTileObject.objectType);    
            } else {
                string tileObjectTypeName = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(tileObject.tileObjectType.ToString());
                SaveDataTileObject saveDataTileObject = CreateNewSaveDataForTileObject(tileObjectTypeName);
                saveDataTileObject.Save(tileObject);
                AddToSaveHub(saveDataTileObject, saveDataTileObject.objectType);    
            }
            finishedObjects.Add(tileObject);
        }
        finishedObjects.Clear();
        finishedObjects = null;
    }
    public static SaveDataTileObject CreateNewSaveDataForTileObject(string tileObjectTypeString) {
        var typeName = $"SaveData{tileObjectTypeString}, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        System.Type type = System.Type.GetType(typeName);
        if (type != null) {
            SaveDataTileObject obj = System.Activator.CreateInstance(type) as SaveDataTileObject;
            return obj;
        }
        return new SaveDataTileObject(); //if no special save data for tile object was found, then just use the generic one
    }
    private SaveDataTileObject createNewSaveDataForArtifact(string tileObjectTypeString) {
        var typeName = $"SaveData{tileObjectTypeString}, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        System.Type type = System.Type.GetType(typeName);
        if (type != null) {
            SaveDataTileObject obj = System.Activator.CreateInstance(type) as SaveDataTileObject;
            return obj;
        }
        return new SaveDataArtifact(); //if no special save data for tile object was found, then just use the generic one
    }
    #endregion

    #region Independent Loading
    public Player LoadPlayer() {
        return playerSave.Load();
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
                Dictionary<string, SaveDataFaction> savedFactions = factionHub.hub;
                foreach (SaveDataFaction data in savedFactions.Values) {
                    data.Load();
                }
            }
        }
    }
    public void LoadTileObjects() {
        if (objectHub.ContainsKey(OBJECT_TYPE.Tile_Object)){
            if(objectHub[OBJECT_TYPE.Tile_Object] is SaveDataTileObjectHub hub) {
                Dictionary<string, SaveDataTileObject> savedFactions = hub.hub;
                foreach (SaveDataTileObject data in savedFactions.Values) {
                    data.Load();
                }
            }
        }
    }
    #endregion

    #region Second Wave Loading
    //SECOND WAVE LOADING - This loading should always be after the FIRST WAVE LOADING, almost all this requires references from other objects and thus, the object must be initiated first before any of these functions are called
    //Initiation of objects are done in FIRST WAVE LOADING
    public void LoadFactionCharacters() {
        for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++) {
            string persistentID = FactionManager.Instance.allFactions[i].persistentID;
            SaveDataFaction saveData = GetFromSaveHub<SaveDataFaction>(OBJECT_TYPE.Faction, persistentID);
            saveData.LoadCharacters();
        }
    }
    public void LoadFactionRelationships() {
        for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++) {
            string persistentID = FactionManager.Instance.allFactions[i].persistentID;
            SaveDataFaction saveData = GetFromSaveHub<SaveDataFaction>(OBJECT_TYPE.Faction, persistentID);
            saveData.LoadRelationships();
        }
    }
    public void LoadFactionLogs() {
        for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++) {
            string persistentID = FactionManager.Instance.allFactions[i].persistentID;
            SaveDataFaction saveData = GetFromSaveHub<SaveDataFaction>(OBJECT_TYPE.Faction, persistentID);
            saveData.LoadLogs();
        }
    }
    #endregion
    
    //public void SaveHextiles(List<HexTile> tiles) {
    //    hextileSaves = new List<SaveDataHextile>();
    //    for (int i = 0; i < tiles.Count; i++) {
    //        HexTile currTile = tiles[i];
    //        SaveDataHextile newSaveData = new SaveDataHextile();
    //        newSaveData.Save(currTile);
    //        hextileSaves.Add(newSaveData);
    //        if(currTile.landmarkOnTile != null) {
    //            SaveLandmark(currTile.landmarkOnTile);
    //        }
    //    }
    //}
    //public void SaveOuterHextiles(List<HexTile> tiles) {
    //    outerHextileSaves = new List<SaveDataHextile>();
    //    for (int i = 0; i < tiles.Count; i++) {
    //        HexTile currTile = tiles[i];
    //        SaveDataHextile newSaveData = new SaveDataHextile();
    //        newSaveData.Save(currTile);
    //        outerHextileSaves.Add(newSaveData);
    //    }
    //}
    //private void SaveLandmark(BaseLandmark landmark) {
    //    if(landmarkSaves == null) {
    //        landmarkSaves = new List<SaveDataLandmark>();
    //    }
    //    var typeName = $"SaveData{landmark.GetType()}, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
    //    System.Type type = System.Type.GetType(typeName);
    //    SaveDataLandmark newSaveData = null;
    //    if (type != null) {
    //        newSaveData = System.Activator.CreateInstance(type) as SaveDataLandmark;
    //    } else {
    //        newSaveData = new SaveDataLandmark();
    //    }
    //    newSaveData.Save(landmark);
    //    //SortAddSaveDataLandmark(newSaveData);
    //    landmarkSaves.Add(newSaveData);
    //}
    //public void SaveLandmarks(List<HexTile> tiles) {
    //    landmarkSaves = new List<SaveDataLandmark>();
    //    for (int i = 0; i < tiles.Count; i++) {
    //        HexTile currTile = tiles[i];
    //        if(currTile.landmarkOnTile != null) {
    //            SaveDataLandmark newSaveData = new SaveDataLandmark();
    //            newSaveData.Save(currTile.landmarkOnTile);
    //            //SortAddSaveDataLandmark(newSaveData);
    //            landmarkSaves.Add(newSaveData);
    //        }
    //    }
    //}
    //private void SortAddSaveDataLandmark(SaveDataLandmark newSaveData) {
    //    bool hasBeenInserted = false;
    //    for (int i = 0; i < landmarkSaves.Count; i++) {
    //        SaveDataLandmark currSaveData = landmarkSaves[i];
    //        if (newSaveData.id < currSaveData.id) {
    //            landmarkSaves.Insert(i, newSaveData);
    //            hasBeenInserted = true;
    //            break;
    //        }
    //    }
    //    if (!hasBeenInserted) {
    //        landmarkSaves.Add(newSaveData);
    //    }
    //}
    //private SaveDataLandmark GetLandmarkSaveByID(int id) {
    //    for (int i = 0; i < landmarkSaves.Count; i++) {
    //        if(landmarkSaves[i].id == id) {
    //            return landmarkSaves[i];
    //        }
    //    }
    //    return null;
    //}
    //public void LoadLandmarks() {
    //    for (int i = 0; i < hextileSaves.Count; i++) {
    //        SaveDataHextile saveDataHextile = hextileSaves[i];
    //        if (saveDataHextile.landmarkID != -1) {
    //            HexTile currTile = GridMap.Instance.normalHexTiles[saveDataHextile.id];
    //            //We get the index for the appropriate landmark of hextile through (landmarkID - 1) because the list of landmarksaves is properly ordered
    //            //Example, the save data in index 0 of the list has an id of 1 since all ids in game start at 1, that is why to get the index of the landmark of the tile, we get the true landmark id and subtract it by 1
    //            //This is done so that we will not loop every time we want to get the save data of a landmark and check all the ids if it will match
    //            GetLandmarkSaveByID(saveDataHextile.landmarkID).Load(currTile);
    //        }
    //    }
    //}

    //public void SaveRegions(Region[] regions) {
    //    regionSaves = new List<SaveDataRegion>();
    //    for (int i = 0; i < regions.Length; i++) {
    //        SaveDataRegion saveDataRegion = new SaveDataRegion();
    //        saveDataRegion.Save(regions[i]);
    //        regionSaves.Add(saveDataRegion);
    //    }
    //}
    //public void LoadRegions() {
    //    Region[] regions = new Region[regionSaves.Count];
    //    for (int i = 0; i < regionSaves.Count; i++) {
    //        regions[i] = regionSaves[i].Load();
    //    }
    //    GridMap.Instance.SetRegions(regions);
    //}
    //public void LoadRegionCharacters() {
    //    for (int i = 0; i < regionSaves.Count; i++) {
    //        SaveDataRegion data = regionSaves[i];
    //        data.LoadRegionCharacters(GridMap.Instance.normalHexTiles[data.coreTileID].region);
    //    }
    //}
    //public void LoadRegionAdditionalData() {
    //    for (int i = 0; i < regionSaves.Count; i++) {
    //        SaveDataRegion data = regionSaves[i];
    //        data.LoadRegionAdditionalData(GridMap.Instance.normalHexTiles[data.coreTileID].region);
    //    }
    //}
    //public void SavePlayerArea(NPCSettlement npcSettlement) {
    //    playerAreaSave = new SaveDataArea();
    //    playerAreaSave.Save(npcSettlement);
    //}
    //public void LoadPlayerArea() {
    //    playerAreaSave.Load();
    //}
    //public void LoadPlayerAreaItems() {
    //    playerAreaSave.LoadAreaItems();
    //}
    //public void SaveNonPlayerAreas() {
    //    nonPlayerAreaSaves = new List<SaveDataArea>();
    //    for (int i = 0; i < LandmarkManager.Instance.allNonPlayerSettlements.Count; i++) {
    //        NPCSettlement npcSettlement = LandmarkManager.Instance.allNonPlayerSettlements[i];
    //        SaveDataArea saveDataArea = new SaveDataArea();
    //        saveDataArea.Save(npcSettlement);
    //        nonPlayerAreaSaves.Add(saveDataArea);
    //    }
    //}
    //public void LoadNonPlayerAreas() {
    //    for (int i = 0; i < nonPlayerAreaSaves.Count; i++) {
    //        nonPlayerAreaSaves[i].Load();
    //    }
    //}
    //public void LoadNonPlayerAreaItems() {
    //    for (int i = 0; i < nonPlayerAreaSaves.Count; i++) {
    //        nonPlayerAreaSaves[i].LoadAreaItems();
    //    }
    //}
    //public void LoadAreaStructureEntranceTiles() {
    //    for (int i = 0; i < nonPlayerAreaSaves.Count; i++) {
    //        nonPlayerAreaSaves[i].LoadStructureEntranceTiles();
    //    }
    //    playerAreaSave.LoadStructureEntranceTiles();
    //}
    //private void LoadAreaJobs() {
    //    for (int i = 0; i < nonPlayerAreaSaves.Count; i++) {
    //        nonPlayerAreaSaves[i].LoadAreaJobs();
    //    }
    //    playerAreaSave.LoadAreaJobs();
    //}

    //public void SaveFactions(List<Faction> factions) {
    //    factionSaves = new List<SaveDataFaction>();
    //    for (int i = 0; i < factions.Count; i++) {
    //        SaveDataFaction saveDataFaction = new SaveDataFaction();
    //        saveDataFaction.Save(factions[i]);
    //        factionSaves.Add(saveDataFaction);
    //    }
    //}
    //public void LoadFactions() {
    //    List<BaseLandmark> allLandmarks = LandmarkManager.Instance.GetAllLandmarks();
    //    for (int i = 0; i < factionSaves.Count; i++) {
    //        factionSaves[i].Load(allLandmarks);
    //    }
    //}
    //public void SaveCharacters(List<Character> characters) {
    //    characterSaves = new List<SaveDataCharacter>();
    //    for (int i = 0; i < characters.Count; i++) {
    //        SaveDataCharacter saveDataCharacter = new SaveDataCharacter();
    //        saveDataCharacter.Save(characters[i]);
    //        characterSaves.Add(saveDataCharacter);
    //    }
    //}
    //public void LoadCharacters() {
    //    for (int i = 0; i < characterSaves.Count; i++) {
    //        characterSaves[i].Load();
    //    }
    //}
    //public void LoadCharacterRelationships() {
    //    for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
    //        characterSaves[i].LoadRelationships(CharacterManager.Instance.allCharacters[i]);
    //    }
    //}
    //public void LoadCharacterTraits() {
    //    for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
    //        characterSaves[i].LoadTraits(CharacterManager.Instance.allCharacters[i]);
    //    }
    //}
    //public void LoadCharacterHomeStructures() {
    //    for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
    //        characterSaves[i].LoadHomeStructure(CharacterManager.Instance.allCharacters[i]);
    //    }
    //}
    //public void LoadCharacterInitialPlacements() {
    //    for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
    //        characterSaves[i].LoadCharacterGridTileLocation(CharacterManager.Instance.allCharacters[i]);
    //    }
    //}
    //public void LoadCharacterCurrentStates() {
    //    for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
    //        characterSaves[i].LoadCharacterCurrentState(CharacterManager.Instance.allCharacters[i]);
    //    }
    //}
    //private void LoadCharacterJobs() {
    //    for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
    //        characterSaves[i].LoadCharacterJobs(CharacterManager.Instance.allCharacters[i]);
    //    }
    //}
    //public void LoadCharacterHistories() {
    //    for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
    //        characterSaves[i].LoadCharacterHistory(CharacterManager.Instance.allCharacters[i]);
    //    }
    //}

    //public void SavePlayer(Player player) {
    //    playerSave = new SaveDataPlayer();
    //    playerSave.Save(player);
    //}
    //public void LoadPlayer() {
    //    playerSave.Load();
    //}

    //public void SaveTileObjects(Dictionary<TILE_OBJECT_TYPE, List<TileObject>> tileObjects) {
    //    tileObjectSaves = new List<SaveDataTileObject>();
    //    foreach (KeyValuePair<TILE_OBJECT_TYPE, List<TileObject>> kvp in tileObjects) {
    //        if(kvp.Key == TILE_OBJECT_TYPE.GENERIC_TILE_OBJECT) {
    //            continue; //Do not save generic tile object because it will be created again upon loading
    //        }
    //        for (int i = 0; i < kvp.Value.Count; i++) {
    //            TileObject currTileObject = kvp.Value[i];
    //            SaveDataTileObject data = null;
    //            System.Type type = System.Type.GetType($"SaveData{currTileObject.GetType()}");
    //            if (type != null) {
    //                data = System.Activator.CreateInstance(type) as SaveDataTileObject;
    //            } else {
    //                if(currTileObject is Artifact) {
    //                    data = new SaveDataArtifact();
    //                } else {
    //                    data = new SaveDataTileObject();
    //                }
    //            }
    //            data.Save(currTileObject);
    //            tileObjectSaves.Add(data);
    //        }
    //    }
    //}
    //public void LoadTileObjects() {
    //    for (int i = 0; i < tileObjectSaves.Count; i++) {
    //        tileObjectSaves[i].Load();
    //    }
    //}
    //public void LoadTileObjectsPreviousTileAndCurrentTile() {
    //    for (int i = 0; i < tileObjectSaves.Count; i++) {
    //        tileObjectSaves[i].LoadPreviousTileAndCurrentTile();
    //    }
    //}
    //public void LoadTileObjectTraits() {
    //    for (int i = 0; i < tileObjectSaves.Count; i++) {
    //        tileObjectSaves[i].LoadTraits();
    //    }
    //}
    //public void LoadTileObjectsDataAfterLoadingAreaMap() {
    //    for (int i = 0; i < tileObjectSaves.Count; i++) {
    //        tileObjectSaves[i].LoadAfterLoadingAreaMap();
    //    }
    //}

    // public void SaveSpecialObjects(List<SpecialObject> specialObjects) {
    //     specialObjectSaves = new List<SaveDataSpecialObject>();
    //     for (int i = 0; i < specialObjects.Count; i++) {
    //         SpecialObject currSpecialObject = specialObjects[i];
    //         SaveDataSpecialObject data = null;
    //         System.Type type = System.Type.GetType("SaveData" + currSpecialObject.GetType().ToString());
    //         if (type != null) {
    //             data = System.Activator.CreateInstance(type) as SaveDataSpecialObject;
    //         } else {
    //             data = new SaveDataSpecialObject();
    //         }
    //         data.Save(currSpecialObject);
    //         specialObjectSaves.Add(data);
    //     }
    // }
    // public void LoadSpecialObjects() {
    //     for (int i = 0; i < specialObjectSaves.Count; i++) {
    //         specialObjectSaves[i].Load();
    //     }
    // }

    // public void SaveAreaMaps(List<AreaInnerTileMap> areaMaps) {
    //     areaMapSaves = new List<SaveDataAreaInnerTileMap>();
    //     for (int i = 0; i < areaMaps.Count; i++) {
    //         SaveDataAreaInnerTileMap data = new SaveDataAreaInnerTileMap();
    //         data.Save(areaMaps[i]);
    //         areaMapSaves.Add(data);
    //     }
    // }
    public void LoadAreaMaps() {
        // for (int i = 0; i < areaMapSaves.Count; i++) {
        //     LandmarkManager.Instance.LoadAreaMap(areaMapSaves[i]);
        // }
    }
    public void LoadAreaMapsTileTraits() {
        // for (int i = 0; i < areaMapSaves.Count; i++) {
        //     areaMapSaves[i].LoadTileTraits();
        // }
    }
    public void LoadAreaMapsObjectHereOfTiles() {
        // for (int i = 0; i < areaMapSaves.Count; i++) {
        //     areaMapSaves[i].LoadObjectHereOfTiles();
        // }
    }

    //public void LoadAllJobs() {
    //    //Loads all jobs except for quest jobs because it will be loaded when the quest is loaded
    //    LoadAreaJobs();
    //    LoadCharacterJobs();
    //}
    //public void SaveNotifications() {
    //    notificationSaves = new List<SaveDataNotification>();
    //    for (int i = 0; i < UIManager.Instance.activeNotifications.Count; i++) {
    //        SaveDataNotification data = new SaveDataNotification();
    //        data.Save(UIManager.Instance.activeNotifications[i]);
    //        notificationSaves.Add(data);
    //    }
    //}
    //public void LoadNotifications() {
    //    for (int i = 0; i < notificationSaves.Count; i++) {
    //        notificationSaves[i].Load();
    //    }
    //}
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