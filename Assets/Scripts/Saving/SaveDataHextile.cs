using System.Collections;
using System.Collections.Generic;
using Locations.Tile_Features;
using UnityEngine;

[System.Serializable]
public class SaveDataHextile : SaveData<HexTile> {
    public string persistentID;
    public int id;
    public int xCoordinate;
    public int yCoordinate;
    public string tileName;

    public float elevationNoise;
    public float moistureNoise;
    public float temperature;
    public BIOMES biomeType;
    public ELEVATION elevationType;

    //Tile Features
    public List<SaveDataTileFeature> tileFeatureSaveData;
    
    public LANDMARK_TYPE landmarkType;
    
    //Components
    public SaveDataHexTileSpellsComponent saveDataHexTileSpellsComponent;

    public override void Save(HexTile tile) {
        persistentID = tile.persistentID;
        id = tile.id;
        xCoordinate = tile.xCoordinate;
        yCoordinate = tile.yCoordinate;
        tileName = tile.tileName;
        elevationNoise = tile.elevationNoise;
        moistureNoise = tile.moistureNoise;
        temperature = tile.temperature;
        biomeType = tile.biomeType;
        elevationType = tile.elevationType;
        landmarkType = tile.landmarkOnTile?.specificLandmarkType ?? LANDMARK_TYPE.NONE;
        
        //tile features
        tileFeatureSaveData = new List<SaveDataTileFeature>();
        for (int i = 0; i < tile.featureComponent.features.Count; i++) {
            TileFeature feature = tile.featureComponent.features[i];
            SaveDataTileFeature saveDataTileFeature = SaveManager.ConvertTileFeatureToSaveData(feature);
            saveDataTileFeature.Save(feature);
            tileFeatureSaveData.Add(saveDataTileFeature);
        }
        saveDataHexTileSpellsComponent = new SaveDataHexTileSpellsComponent();
        saveDataHexTileSpellsComponent.Save(tile.spellsComponent);
    }
    public void Load(HexTile tile) {
        if (string.IsNullOrEmpty(persistentID)) {
            tile.data.persistentID = System.Guid.NewGuid().ToString();
        } else {
            tile.data.persistentID = persistentID;    
        }
        tile.data.id = id;
        tile.data.xCoordinate = xCoordinate;
        tile.data.yCoordinate = yCoordinate;
        tile.data.tileName = tileName;
        tile.data.elevationNoise = elevationNoise;
        tile.data.moistureNoise = moistureNoise;
        tile.data.temperature = temperature;
        tile.data.biomeType = biomeType;
        tile.data.elevationType = elevationType;
    }
}
