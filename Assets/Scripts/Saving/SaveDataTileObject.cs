using System.Collections.Generic;
using BayatGames.SaveGameFree.Types;
using Inner_Maps;
using Traits;
using UnityEngine;

[System.Serializable]
public class SaveDataTileObject : SaveData<TileObject>, ISavableCounterpart {
    public string persistentID { get; set; }
    public int id;
    public string name;
    public TILE_OBJECT_TYPE tileObjectType;
    public int characterOwnerID;
    public int regionLocationID;
    public string tileLocationID;
    public bool isPreplaced;
    public int[] resourceValues; //food, wood, stone, metal
    public POI_STATE poiState;
    public INTERACTION_TYPE[] advertisedActions; 
    
    //hp
    public int currentHP;
    
    //visuals
    public string spriteName;
    public QuaternionSave rotation;
    
    #region getters
    public OBJECT_TYPE objectType => OBJECT_TYPE.Tile_Object;
    #endregion
    
    public override void Save(TileObject data) {
        persistentID = data.persistentID;
        id = data.id;
        name = data.name;
        tileObjectType = data.tileObjectType;
        characterOwnerID = data.characterOwner?.id ?? -1;
        if (data.gridTileLocation != null) {
            regionLocationID = data.gridTileLocation.parentMap.region.id;
            tileLocationID = data.gridTileLocation.persistentID;    
        } else {
            regionLocationID = -1;
            tileLocationID = string.Empty;
        }
        
        isPreplaced = data.isPreplaced;
        poiState = data.state;
        
        advertisedActions = new INTERACTION_TYPE[data.advertisedActions.Count];
        for (int i = 0; i < advertisedActions.Length; i++) {
            INTERACTION_TYPE interactionType = data.advertisedActions[i];
            advertisedActions[i] = interactionType;
        }
        
        currentHP = data.currentHP;

        if (data.mapObjectVisual == null || data.mapObjectVisual.usedSprite == null) {
            spriteName = string.Empty;
            rotation = Quaternion.identity;
            // Debug.Log($"Tile Object {tileObject} has no map object or visual.");
        } else {
            spriteName = data.mapObjectVisual.usedSprite.name;
            rotation = data.mapObjectVisual.rotation;
        }

        resourceValues = new int[data.storedResources.Count];
        int index = 0;
        foreach (var storedResource in data.storedResources) {
            resourceValues[index] = storedResource.Value;
            index++;
        }
        
    }
    public override TileObject Load() {
        TileObject tileObject = InnerMapManager.Instance.LoadTileObject<TileObject>(this);
        tileObject.Initialize(this);
        return tileObject;
    }
}