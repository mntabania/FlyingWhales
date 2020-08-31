using System.Collections.Generic;
using BayatGames.SaveGameFree.Types;
using Inner_Maps;
using Traits;
using UnityEngine;

[System.Serializable]
public class SaveDataTileObject : SaveData<TileObject>, ISavableCounterpart {
    public string _persistentID;
    public OBJECT_TYPE _objectType;
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
    public string persistentID => _persistentID;
    public OBJECT_TYPE objectType => _objectType;
    #endregion
    
    public override void Save(TileObject tileObject) {
        _persistentID = tileObject.persistentID;
        _objectType = tileObject.objectType;
        id = tileObject.id;
        name = tileObject.name;
        tileObjectType = tileObject.tileObjectType;
        characterOwnerID = tileObject.characterOwner?.id ?? -1;
        if (tileObject.gridTileLocation != null) {
            regionLocationID = tileObject.gridTileLocation.parentMap.region.id;
            tileLocationID = tileObject.gridTileLocation.persistentID;    
        } else {
            regionLocationID = -1;
            tileLocationID = string.Empty;
        }
        
        isPreplaced = tileObject.isPreplaced;
        poiState = tileObject.state;
        
        advertisedActions = new INTERACTION_TYPE[tileObject.advertisedActions.Count];
        for (int i = 0; i < advertisedActions.Length; i++) {
            INTERACTION_TYPE interactionType = tileObject.advertisedActions[i];
            advertisedActions[i] = interactionType;
        }
        
        currentHP = tileObject.currentHP;

        if (tileObject.mapObjectVisual == null || tileObject.mapObjectVisual.usedSprite == null) {
            spriteName = string.Empty;
            rotation = Quaternion.identity;
            // Debug.Log($"Tile Object {tileObject} has no map object or visual.");
        } else {
            spriteName = tileObject.mapObjectVisual.usedSprite.name;
            rotation = tileObject.mapObjectVisual.rotation;
        }

        resourceValues = new int[tileObject.storedResources.Count];
        int index = 0;
        foreach (var storedResource in tileObject.storedResources) {
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