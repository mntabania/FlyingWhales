using System.Collections.Generic;
using BayatGames.SaveGameFree.Types;
using Inner_Maps;
using Traits;
using UnityEngine;

[System.Serializable]
public class SaveDataTileObject : SaveData<TileObject> {
    public string persistentID;
    public int id;
    public string name;
    public TILE_OBJECT_TYPE tileObjectType;
    public int characterOwnerID;
    public int regionLocationID;
    public Point tileLocation;
    public bool isPreplaced;
    public int[] resourceValues; //food, wood, stone, metal
    public POI_STATE poiState;
    public INTERACTION_TYPE[] advertisedActions; 
    
    //hp
    public int currentHP;
    
    //visuals
    public string spriteName;
    public QuaternionSave rotation;
    
    public override void Save(TileObject tileObject) {
        persistentID = tileObject.persistentID;
        id = tileObject.id;
        name = tileObject.name;
        tileObjectType = tileObject.tileObjectType;
        characterOwnerID = tileObject.characterOwner?.id ?? -1;
        regionLocationID = tileObject.gridTileLocation.parentMap.region.id;
        tileLocation = new Point(tileObject.gridTileLocation.localPlace.x, tileObject.gridTileLocation.localPlace.y);
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
            Debug.Log($"Tile Object {tileObject} has no map object or visual.");
        } else {
            spriteName = tileObject.mapObjectVisual.usedSprite.name;
        }
        rotation = tileObject.mapObjectVisual.rotation;

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