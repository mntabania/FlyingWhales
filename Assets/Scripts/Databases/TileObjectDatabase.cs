using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UtilityScripts;
public class TileObjectDatabase {
    public Dictionary<TILE_OBJECT_TYPE, List<TileObject>> allTileObjects { get; private set; }
    public Dictionary<string, TileObject> tileObjectsByGUID { get; private set; }
    public HashSet<TileObject> allTileObjectsList { get; private set; }
    public List<WeakReference> destroyedTileObjects { get; private set; }
    public Dictionary<string, WeakReference> destroyedTileObjectsDictionary { get; private set; }
    public List<WeakReference> pendingDestroyedTileObjects { get; private set; }
    public CleanUpTileObjectsThread cleanUpThread { get; private set; }

    public TileObjectDatabase() {
        allTileObjects = new Dictionary<TILE_OBJECT_TYPE, List<TileObject>>();
        tileObjectsByGUID = new Dictionary<string, TileObject>();
        allTileObjectsList = new HashSet<TileObject>();
        destroyedTileObjects = new List<WeakReference>();
        pendingDestroyedTileObjects = new List<WeakReference>();
        destroyedTileObjectsDictionary = new Dictionary<string, WeakReference>();
        cleanUpThread = new CleanUpTileObjectsThread();
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
    public void UnRegisterTileObject(TileObject tileObject, bool processCleanUp = true) {
        allTileObjects[tileObject.tileObjectType].Remove(tileObject);
        tileObjectsByGUID.Remove(tileObject.persistentID);
        allTileObjectsList.Remove(tileObject);
        tileObject.SetIsDeadReference(true);
        AddDestroyedTileObject(tileObject, processCleanUp);
    }

    public TileObject GetTileObject(TILE_OBJECT_TYPE type, int id) {
        if (allTileObjects.ContainsKey(type)) {
            for (int i = 0; i < allTileObjects[type].Count; i++) {
                TileObject to = allTileObjects[type][i];
                if (to.id == id) {
                    return to;
                }
            }
        }
        return null;
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
    public TileObject GetTileObjectByPersistentID(string id) {
        if (tileObjectsByGUID.ContainsKey(id)) {
            return tileObjectsByGUID[id];
        } else if (destroyedTileObjectsDictionary.ContainsKey(id)) {
            WeakReference wr = destroyedTileObjectsDictionary[id];
            if (wr.IsAlive) {
                return wr.Target as TileObject;
            }
        }
        //return null;
        //Removed exception because this can now return null since we delete objects that are destroyed from the master list
        throw new Exception($"Could not find tile object with id {id}");
    }
    public TileObject GetTileObjectByPersistentIDSafe(string id) {
        if (tileObjectsByGUID.ContainsKey(id)) {
            return tileObjectsByGUID[id];
        } else if (destroyedTileObjectsDictionary.ContainsKey(id)) {
            WeakReference wr = destroyedTileObjectsDictionary[id];
            if (wr.IsAlive) {
                return wr.Target as TileObject;
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

    #region Clean Up
    private void AddDestroyedTileObject(TileObject tileObject, bool processCleanUp) {
        WeakReference wr = new WeakReference(tileObject);
        if (cleanUpThread.isProcessing) {
            pendingDestroyedTileObjects.Add(wr);
        } else {
            AddDestroyedTileObject(tileObject.persistentID, wr);
            if (processCleanUp) {
                ProcessCleanUpDestroyedTileObjects();
            }
        }
    }
    private void AddDestroyedTileObject(string id, WeakReference wr) {
        if (destroyedTileObjectsDictionary.ContainsKey(id)) {
#if DEBUG_LOG
            Debug.LogError($"Tile Object {wr.Target.ToString()} has already been added to destroyed tile object list");
#endif
        } else {
#if DEBUG_LOG
            Debug.Log($"Tile Object {wr.Target.ToString()} added to destroyed tile object list");
#endif
            destroyedTileObjectsDictionary.Add(id, wr);
            destroyedTileObjects.Add(wr);    
        }
    }
    private void ProcessCleanUpDestroyedTileObjects() {
        //if (!cleanUpThread.isProcessing) {
        //    cleanUpThread.SetIsProcessing(true);
        //    cleanUpThread.SetListToBeCleanedUp(destroyedTileObjects);
        //    cleanUpThread.SetDictionaryToBeCleanedUp(destroyedTileObjectsDictionary);
        //    MultiThreadPool.Instance.AddToThreadPool(cleanUpThread);
        //}
    }
    public void DoneProcessCleanUpDestroyedTileObjects(Dictionary<string, WeakReference> cleanDictionary) {
        //Switch the destroyedTileObjectsDictionary with the clean dictionary and put the old one in the object pool
        RuinarchCleanUpDictionaryPool.Release(destroyedTileObjectsDictionary);
        destroyedTileObjectsDictionary = cleanDictionary;
    }
    public void AfterDone() {
        destroyedTileObjectsDictionary.ToString();
        if (pendingDestroyedTileObjects.Count > 0) {
            for (int i = 0; i < pendingDestroyedTileObjects.Count; i++) {
                WeakReference wr = pendingDestroyedTileObjects[i];
                if (wr.IsAlive) {
                    AddDestroyedTileObject((wr.Target as TileObject).persistentID, wr);
                }
            }
            pendingDestroyedTileObjects.Clear();
            ProcessCleanUpDestroyedTileObjects();
        }
    }
    #endregion
}
