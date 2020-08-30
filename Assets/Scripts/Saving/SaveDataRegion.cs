using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BayatGames.SaveGameFree.Types;
using Inner_Maps.Location_Structures;
using UnityEngine.Assertions;

[System.Serializable]
public class SaveDataRegion : SaveData<Region> {
    public string persistentID;
    public int id;
    public string name;
    public int coreTileID;
    public ColorSave regionColor;
    public RegionTemplate regionTemplate;
    public int[] residentIDs;
    public SaveDataLocationStructure[] structureSaveData;
    public SaveDataInnerMap innerMapSave;
    public List<SaveDataTileObject> tileObjectSaves;
    
    public void Save(Region region) {
        persistentID = region.persistentID;
        id = region.id;
        name = region.name;
        coreTileID = region.coreTile.id;
        regionColor = region.regionColor;
        regionTemplate = region.regionTemplate;
        
        //residents
        residentIDs = new int[region.residents.Count];
        for (int i = 0; i < region.residents.Count; i++) {
            Character character = region.residents[i];
            residentIDs[i] = character.id;
        }
        
        //structures
        structureSaveData = new SaveDataLocationStructure[region.allStructures.Count];
        for (int i = 0; i < region.allStructures.Count; i++) {
            LocationStructure structure = region.allStructures[i];
            SaveDataLocationStructure saveDataLocationStructure = CreateNewSaveDataFor(structure);
            saveDataLocationStructure.Save(structure);
            structureSaveData[i] = saveDataLocationStructure;
        }
        innerMapSave = new SaveDataInnerMap();
        innerMapSave.Save(region.innerMap);    
        
        //tile objects
        tileObjectSaves = new List<SaveDataTileObject>();
        for (int i = 0; i < region.allStructures.Count; i++) {
            LocationStructure structure = region.allStructures[i];
            foreach (var groupedTileObject in structure.groupedTileObjects) {
                if (groupedTileObject.Key == TILE_OBJECT_TYPE.ARTIFACT) {
                    // //save process for artifacts
                    // for (int j = 0; j < groupedTileObject.Value.tileObjects.Count; j++) {
                    //     TileObject tileObject = groupedTileObject.Value.tileObjects[j];
                    //     Artifact artifact = tileObject as Artifact;
                    //     Assert.IsNotNull(artifact, $"'Grouped object in artifact is not actually an artifact! {tileObject}");
                    //     string tileObjectTypeName = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(artifact.type.ToString());
                    //     SaveDataTileObject saveDataTileObject = CreateNewSaveDataForTileObject(tileObjectTypeName);
                    //     saveDataTileObject.Save(tileObject);
                    //     tileObjectSaves.Add(saveDataTileObject);
                    // }    
                } else {
                    //save process for normal tile objects
                    string tileObjectTypeName = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(groupedTileObject.Key.ToString());
                    for (int j = 0; j < groupedTileObject.Value.tileObjects.Count; j++) {
                        TileObject tileObject = groupedTileObject.Value.tileObjects[j];
                        SaveDataTileObject saveDataTileObject = CreateNewSaveDataForTileObject(tileObjectTypeName);
                        saveDataTileObject.Save(tileObject);
                        tileObjectSaves.Add(saveDataTileObject);
                    }    
                }
                
            }
        }
    }

    #region Structure
    private SaveDataLocationStructure CreateNewSaveDataFor(LocationStructure structure) {
        if (structure is DemonicStructure) {
            return new SaveDataDemonicStructure();
        } else if (structure is NaturalStructure) {
            return new SaveDataNaturalStructure();
        } else {
            return new SaveDataManMadeStructure();
        }
    }
    public void InitialLoadStructures(Region location) {
        location.CreateStructureList();
        for (int i = 0; i < structureSaveData.Length; i++) {
            SaveDataLocationStructure saveDataLocationStructure = structureSaveData[i];
            LocationStructure createdStructure = saveDataLocationStructure.InitialLoad(location);
            if (createdStructure != null) {
                location.AddStructure(createdStructure);
            }
            saveDataLocationStructure.Load();
        }
    }
    #endregion

    #region Tile Objects
    private SaveDataTileObject CreateNewSaveDataForTileObject(string tileObjectTypeString) {
        var typeName = $"SaveData{tileObjectTypeString}, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        System.Type type = System.Type.GetType(typeName);
        if (type != null) {
            SaveDataTileObject obj = System.Activator.CreateInstance(type) as SaveDataTileObject;
            return obj;
        }
        return new SaveDataTileObject(); //if no special save data for tile object was found, then just use the generic one
    }
    #endregion
}
