using System.Collections;
using System.Collections.Generic;
using Locations.Area_Features;
using UnityEngine;

[System.Serializable]
public class SaveDataHextile : SaveData<Area> {
    public string persistentID;
    public int id;
    public int xCoordinate;
    public int yCoordinate;
    public string tileName;
    
    //Tile Features
    public List<SaveDataAreaFeature> areaFeatureSaveData;
    
    //Components
    //public SaveDataHexTileSpellsComponent saveDataHexTileSpellsComponent;

    public override void Save(Area tile) {
        persistentID = tile.persistentID;
        id = tile.id;
        xCoordinate = tile.areaData.xCoordinate;
        yCoordinate = tile.areaData.yCoordinate;
        tileName = tile.areaData.areaName;
        
        //tile features
        areaFeatureSaveData = new List<SaveDataAreaFeature>();
        for (int i = 0; i < tile.featureComponent.features.Count; i++) {
            AreaFeature feature = tile.featureComponent.features[i];
            SaveDataAreaFeature saveDataTileFeature = SaveManager.ConvertAreaFeatureToSaveData(feature);
            saveDataTileFeature.Save(feature);
            areaFeatureSaveData.Add(saveDataTileFeature);
        }
        //saveDataHexTileSpellsComponent = new SaveDataHexTileSpellsComponent();
        //saveDataHexTileSpellsComponent.Save(tile.spellsComponent);
    }
    public void Load(Area tile) {
        if (string.IsNullOrEmpty(persistentID)) {
            tile.areaData.persistentID = System.Guid.NewGuid().ToString();
        } else {
            tile.areaData.persistentID = persistentID;    
        }
        tile.areaData.id = id;
        tile.areaData.xCoordinate = xCoordinate;
        tile.areaData.yCoordinate = yCoordinate;
        tile.areaData.areaName = tileName;
    }
}