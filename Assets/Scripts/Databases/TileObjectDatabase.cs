using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileObjectDatabase {
    public Dictionary<TILE_OBJECT_TYPE, List<TileObject>> allTileObjects { get; private set; }
    public Dictionary<string, TileObject> tileObjectsByGUID { get; private set; }
    public List<TileObject> allTileObjectsList { get; private set; }

    public TileObjectDatabase() {
        allTileObjects = new Dictionary<TILE_OBJECT_TYPE, List<TileObject>>();
        tileObjectsByGUID = new Dictionary<string, TileObject>();
        allTileObjectsList = new List<TileObject>();
    }

    public void RegisterTileObject(TileObject tileObject) {
        if (!allTileObjects.ContainsKey(tileObject.tileObjectType)) {
            allTileObjects.Add(tileObject.tileObjectType, new List<TileObject>());
        }
        allTileObjects[tileObject.tileObjectType].Add(tileObject);
        // Debug.Log($"Added new tile object {tileObject} to database with id {tileObject.persistentID}");
        tileObjectsByGUID.Add(tileObject.persistentID, tileObject);
        allTileObjectsList.Add(tileObject);
    }
    public void UnRegisterTileObject(TileObject tileObject) {
        allTileObjects[tileObject.tileObjectType].Remove(tileObject);
        tileObjectsByGUID.Remove(tileObject.persistentID);
        allTileObjectsList.Remove(tileObject);
    }
    public TileObject GetTileObject(TILE_OBJECT_TYPE type, int id) {
        if (allTileObjects.ContainsKey(type)) {
            for (int i = 0; i < allTileObjects[type].Count; i++) {
                TileObject to = allTileObjects[type][i];
                if(to.id == id) {
                    return to;
                }
            }
        }
        return null;
    }
    public TileObject GetTileObjectByPersistentID(string id) {
        if (tileObjectsByGUID.ContainsKey(id)) {
            return tileObjectsByGUID[id];
        }
        throw new Exception($"Could not find tile object with id {id}");
    }
    public TileObject GetFirstTileObject(TILE_OBJECT_TYPE type) {
        if (allTileObjects.ContainsKey(type)) {
            for (int i = 0; i < allTileObjects[type].Count; i++) {
                TileObject to = allTileObjects[type][i];
                return to;
            }
        }
        return null;
    }
    public TileObject GetFirstArtifact(ARTIFACT_TYPE artifactType) {
        if (allTileObjects.ContainsKey(TILE_OBJECT_TYPE.ARTIFACT)) {
            for (int i = 0; i < allTileObjects[TILE_OBJECT_TYPE.ARTIFACT].Count; i++) {
                TileObject to = allTileObjects[TILE_OBJECT_TYPE.ARTIFACT][i];
                if (to is Artifact artifact && artifact.type == artifactType) {
                    return to;
                }
            }
        }
        return null;
    }
}
