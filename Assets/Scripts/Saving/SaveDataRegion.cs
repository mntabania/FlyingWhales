using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BayatGames.SaveGameFree.Types;
using Inner_Maps.Location_Structures;

[System.Serializable]
public class SaveDataRegion : SaveData<Region> {
    public int id;
    public string name;
    public int coreTileID;
    public ColorSave regionColor;
    public RegionTemplate regionTemplate;
    public int[] residentIDs;
    public SaveDataLocationStructure[] structureSaveData;

    public SaveDataInnerMap innerMapSave;
    
    public void Save(Region region, bool saveInnerMap = true) {
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
        if (saveInnerMap) {
            innerMapSave = new SaveDataInnerMap();
            innerMapSave.Save(region.innerMap);    
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
}
