using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

[System.Serializable]
public class Save {
    public int width;
    public int height;
    public int borderThickness;
    public List<SaveDataHextile> hextileSaves;
    public List<SaveDataHextile> outerHextileSaves;
    public List<SaveDataLandmark> landmarkSaves;
    public List<SaveDataRegion> regionSaves;
    public List<SaveDataArea> nonPlayerAreaSaves;
    public List<SaveDataFaction> factionSaves;
    public List<SaveDataCharacter> characterSaves;
    public List<SaveDataTileObject> tileObjectSaves;
    // public List<SaveDataSpecialObject> specialObjectSaves;
    public List<SaveDataAreaInnerTileMap> areaMapSaves;
    public List<SaveDataNotification> notificationSaves;

    public SaveDataArea playerAreaSave;
    public SaveDataPlayer playerSave;

    public int month;
    public int day;
    public int year;
    public int tick;
    public int continuousDays;

    public Save(int width, int height, int borderThickness) {
        this.width = width;
        this.height = height;
        this.borderThickness = borderThickness;
    }

    public void SaveHextiles(List<HexTile> tiles) {
        hextileSaves = new List<SaveDataHextile>();
        for (int i = 0; i < tiles.Count; i++) {
            HexTile currTile = tiles[i];
            SaveDataHextile newSaveData = new SaveDataHextile();
            newSaveData.Save(currTile);
            hextileSaves.Add(newSaveData);
            if(currTile.landmarkOnTile != null) {
                SaveLandmark(currTile.landmarkOnTile);
            }
        }
    }
    public void SaveOuterHextiles(List<HexTile> tiles) {
        outerHextileSaves = new List<SaveDataHextile>();
        for (int i = 0; i < tiles.Count; i++) {
            HexTile currTile = tiles[i];
            SaveDataHextile newSaveData = new SaveDataHextile();
            newSaveData.Save(currTile);
            outerHextileSaves.Add(newSaveData);
        }
    }
    private void SaveLandmark(BaseLandmark landmark) {
        if(landmarkSaves == null) {
            landmarkSaves = new List<SaveDataLandmark>();
        }
        var typeName = $"SaveData{landmark.GetType()}";
        System.Type type = System.Type.GetType(typeName);
        SaveDataLandmark newSaveData = null;
        if (type != null) {
            newSaveData = System.Activator.CreateInstance(type) as SaveDataLandmark;
        } else {
            newSaveData = new SaveDataLandmark();
        }
        newSaveData.Save(landmark);
        //SortAddSaveDataLandmark(newSaveData);
        landmarkSaves.Add(newSaveData);
    }
    public void SaveLandmarks(List<HexTile> tiles) {
        landmarkSaves = new List<SaveDataLandmark>();
        for (int i = 0; i < tiles.Count; i++) {
            HexTile currTile = tiles[i];
            if(currTile.landmarkOnTile != null) {
                SaveDataLandmark newSaveData = new SaveDataLandmark();
                newSaveData.Save(currTile.landmarkOnTile);
                //SortAddSaveDataLandmark(newSaveData);
                landmarkSaves.Add(newSaveData);
            }
        }
    }
    private void SortAddSaveDataLandmark(SaveDataLandmark newSaveData) {
        bool hasBeenInserted = false;
        for (int i = 0; i < landmarkSaves.Count; i++) {
            SaveDataLandmark currSaveData = landmarkSaves[i];
            if (newSaveData.id < currSaveData.id) {
                landmarkSaves.Insert(i, newSaveData);
                hasBeenInserted = true;
                break;
            }
        }
        if (!hasBeenInserted) {
            landmarkSaves.Add(newSaveData);
        }
    }
    private SaveDataLandmark GetLandmarkSaveByID(int id) {
        for (int i = 0; i < landmarkSaves.Count; i++) {
            if(landmarkSaves[i].id == id) {
                return landmarkSaves[i];
            }
        }
        return null;
    }
    public void LoadLandmarks() {
        for (int i = 0; i < hextileSaves.Count; i++) {
            SaveDataHextile saveDataHextile = hextileSaves[i];
            if (saveDataHextile.landmarkID != -1) {
                HexTile currTile = GridMap.Instance.normalHexTiles[saveDataHextile.id];
                //We get the index for the appropriate landmark of hextile through (landmarkID - 1) because the list of landmarksaves is properly ordered
                //Example, the save data in index 0 of the list has an id of 1 since all ids in game start at 1, that is why to get the index of the landmark of the tile, we get the true landmark id and subtract it by 1
                //This is done so that we will not loop every time we want to get the save data of a landmark and check all the ids if it will match
                GetLandmarkSaveByID(saveDataHextile.landmarkID).Load(currTile);
            }
        }
    }

    public void SaveRegions(Region[] regions) {
        regionSaves = new List<SaveDataRegion>();
        for (int i = 0; i < regions.Length; i++) {
            SaveDataRegion saveDataRegion = new SaveDataRegion();
            saveDataRegion.Save(regions[i]);
            regionSaves.Add(saveDataRegion);
        }
    }
    public void LoadRegions() {
        Region[] regions = new Region[regionSaves.Count];
        for (int i = 0; i < regionSaves.Count; i++) {
            regions[i] = regionSaves[i].Load();
        }
        GridMap.Instance.SetRegions(regions);
    }
    public void LoadRegionCharacters() {
        for (int i = 0; i < regionSaves.Count; i++) {
            SaveDataRegion data = regionSaves[i];
            data.LoadRegionCharacters(GridMap.Instance.normalHexTiles[data.coreTileID].region);
        }
    }
    public void LoadRegionAdditionalData() {
        for (int i = 0; i < regionSaves.Count; i++) {
            SaveDataRegion data = regionSaves[i];
            data.LoadRegionAdditionalData(GridMap.Instance.normalHexTiles[data.coreTileID].region);
        }
    }
    public void SavePlayerArea(NPCSettlement npcSettlement) {
        playerAreaSave = new SaveDataArea();
        playerAreaSave.Save(npcSettlement);
    }
    public void LoadPlayerArea() {
        playerAreaSave.Load();
    }
    public void LoadPlayerAreaItems() {
        playerAreaSave.LoadAreaItems();
    }
    public void SaveNonPlayerAreas() {
        nonPlayerAreaSaves = new List<SaveDataArea>();
        for (int i = 0; i < LandmarkManager.Instance.allNonPlayerSettlements.Count; i++) {
            NPCSettlement npcSettlement = LandmarkManager.Instance.allNonPlayerSettlements[i];
            SaveDataArea saveDataArea = new SaveDataArea();
            saveDataArea.Save(npcSettlement);
            nonPlayerAreaSaves.Add(saveDataArea);
        }
    }
    public void LoadNonPlayerAreas() {
        for (int i = 0; i < nonPlayerAreaSaves.Count; i++) {
            nonPlayerAreaSaves[i].Load();
        }
    }
    public void LoadNonPlayerAreaItems() {
        for (int i = 0; i < nonPlayerAreaSaves.Count; i++) {
            nonPlayerAreaSaves[i].LoadAreaItems();
        }
    }
    public void LoadAreaStructureEntranceTiles() {
        for (int i = 0; i < nonPlayerAreaSaves.Count; i++) {
            nonPlayerAreaSaves[i].LoadStructureEntranceTiles();
        }
        playerAreaSave.LoadStructureEntranceTiles();
    }
    private void LoadAreaJobs() {
        for (int i = 0; i < nonPlayerAreaSaves.Count; i++) {
            nonPlayerAreaSaves[i].LoadAreaJobs();
        }
        playerAreaSave.LoadAreaJobs();
    }

    public void SaveFactions(List<Faction> factions) {
        factionSaves = new List<SaveDataFaction>();
        for (int i = 0; i < factions.Count; i++) {
            SaveDataFaction saveDataFaction = new SaveDataFaction();
            saveDataFaction.Save(factions[i]);
            factionSaves.Add(saveDataFaction);
        }
    }
    public void LoadFactions() {
        List<BaseLandmark> allLandmarks = LandmarkManager.Instance.GetAllLandmarks();
        for (int i = 0; i < factionSaves.Count; i++) {
            factionSaves[i].Load(allLandmarks);
        }
    }
    public void LoadFactionsActiveQuests() {
        for (int i = 0; i < factionSaves.Count; i++) {
            factionSaves[i].LoadFactionActiveQuest();
        }
    }

    public void SaveCharacters(List<Character> characters) {
        characterSaves = new List<SaveDataCharacter>();
        for (int i = 0; i < characters.Count; i++) {
            SaveDataCharacter saveDataCharacter = new SaveDataCharacter();
            saveDataCharacter.Save(characters[i]);
            characterSaves.Add(saveDataCharacter);
        }
    }
    public void LoadCharacters() {
        for (int i = 0; i < characterSaves.Count; i++) {
            characterSaves[i].Load();
        }
    }
    public void LoadCharacterRelationships() {
        for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
            characterSaves[i].LoadRelationships(CharacterManager.Instance.allCharacters[i]);
        }
    }
    public void LoadCharacterTraits() {
        for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
            characterSaves[i].LoadTraits(CharacterManager.Instance.allCharacters[i]);
        }
    }
    public void LoadCharacterHomeStructures() {
        for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
            characterSaves[i].LoadHomeStructure(CharacterManager.Instance.allCharacters[i]);
        }
    }
    public void LoadCharacterInitialPlacements() {
        for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
            characterSaves[i].LoadCharacterGridTileLocation(CharacterManager.Instance.allCharacters[i]);
        }
    }
    public void LoadCharacterCurrentStates() {
        for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
            characterSaves[i].LoadCharacterCurrentState(CharacterManager.Instance.allCharacters[i]);
        }
    }
    private void LoadCharacterJobs() {
        for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
            characterSaves[i].LoadCharacterJobs(CharacterManager.Instance.allCharacters[i]);
        }
    }
    public void LoadCharacterHistories() {
        for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
            characterSaves[i].LoadCharacterHistory(CharacterManager.Instance.allCharacters[i]);
        }
    }

    public void SavePlayer(Player player) {
        playerSave = new SaveDataPlayer();
        playerSave.Save(player);
    }
    public void LoadPlayer() {
        playerSave.Load();
    }

    public void SaveTileObjects(Dictionary<TILE_OBJECT_TYPE, List<TileObject>> tileObjects) {
        tileObjectSaves = new List<SaveDataTileObject>();
        foreach (KeyValuePair<TILE_OBJECT_TYPE, List<TileObject>> kvp in tileObjects) {
            if(kvp.Key == TILE_OBJECT_TYPE.GENERIC_TILE_OBJECT) {
                continue; //Do not save generic tile object because it will be created again upon loading
            }
            for (int i = 0; i < kvp.Value.Count; i++) {
                TileObject currTileObject = kvp.Value[i];
                SaveDataTileObject data = null;
                System.Type type = System.Type.GetType($"SaveData{currTileObject.GetType()}");
                if (type != null) {
                    data = System.Activator.CreateInstance(type) as SaveDataTileObject;
                } else {
                    if(currTileObject is Artifact) {
                        data = new SaveDataArtifact();
                    } else {
                        data = new SaveDataTileObject();
                    }
                }
                data.Save(currTileObject);
                tileObjectSaves.Add(data);
            }
        }
    }
    public void LoadTileObjects() {
        for (int i = 0; i < tileObjectSaves.Count; i++) {
            tileObjectSaves[i].Load();
        }
    }
    public void LoadTileObjectsPreviousTileAndCurrentTile() {
        for (int i = 0; i < tileObjectSaves.Count; i++) {
            tileObjectSaves[i].LoadPreviousTileAndCurrentTile();
        }
    }
    public void LoadTileObjectTraits() {
        for (int i = 0; i < tileObjectSaves.Count; i++) {
            tileObjectSaves[i].LoadTraits();
        }
    }
    public void LoadTileObjectsDataAfterLoadingAreaMap() {
        for (int i = 0; i < tileObjectSaves.Count; i++) {
            tileObjectSaves[i].LoadAfterLoadingAreaMap();
        }
    }

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

    public void SaveAreaMaps(List<AreaInnerTileMap> areaMaps) {
        areaMapSaves = new List<SaveDataAreaInnerTileMap>();
        for (int i = 0; i < areaMaps.Count; i++) {
            SaveDataAreaInnerTileMap data = new SaveDataAreaInnerTileMap();
            data.Save(areaMaps[i]);
            areaMapSaves.Add(data);
        }
    }
    public void LoadAreaMaps() {
        for (int i = 0; i < areaMapSaves.Count; i++) {
            LandmarkManager.Instance.LoadAreaMap(areaMapSaves[i]);
        }
    }
    public void LoadAreaMapsTileTraits() {
        for (int i = 0; i < areaMapSaves.Count; i++) {
            areaMapSaves[i].LoadTileTraits();
        }
    }
    public void LoadAreaMapsObjectHereOfTiles() {
        for (int i = 0; i < areaMapSaves.Count; i++) {
            areaMapSaves[i].LoadObjectHereOfTiles();
        }
    }

    public void LoadAllJobs() {
        //Loads all jobs except for quest jobs because it will be loaded when the quest is loaded
        LoadAreaJobs();
        LoadCharacterJobs();
    }

    public void SaveCurrentDate() {
        month = GameManager.Instance.month;
        day = GameManager.days;
        year = GameManager.Instance.year;
        tick = GameManager.Instance.tick;
        continuousDays = GameManager.Instance.continuousDays;
    }
    public void LoadCurrentDate() {
        GameManager.Instance.month = month;
        GameManager.days = day;
        GameManager.Instance.year = year;
        GameManager.Instance.tick = tick;
        GameManager.Instance.continuousDays = continuousDays;
    }

    public void SaveNotifications() {
        notificationSaves = new List<SaveDataNotification>();
        for (int i = 0; i < UIManager.Instance.activeNotifications.Count; i++) {
            SaveDataNotification data = new SaveDataNotification();
            data.Save(UIManager.Instance.activeNotifications[i]);
            notificationSaves.Add(data);
        }
    }
    public void LoadNotifications() {
        for (int i = 0; i < notificationSaves.Count; i++) {
            notificationSaves[i].Load();
        }
    }
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