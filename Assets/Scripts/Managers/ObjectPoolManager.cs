using UnityEngine;
using System.Collections;
using EZObjectPools;
using System.Collections.Generic;
using System;
using System.Linq;

public class ObjectPoolManager : MonoBehaviour {

    public static ObjectPoolManager Instance = null;

    private Dictionary<string, EZObjectPool> allObjectPools;

    [SerializeField] private GameObject[] UIPrefabs;
    [SerializeField] internal GameObject[] otherPrefabs;
    [SerializeField] private GameObject UIObjectPoolParent;

    public List<GoapNode> goapNodesPool { get; private set; }
    public List<OpinionData> opinionDataPool { get; private set; }
    public List<TraitRemoveSchedule> traitRemoveSchedulePool { get; private set; }

    private void Awake() {
        Instance = this;
        allObjectPools = new Dictionary<string, EZObjectPool>();
    }
    public void InitializeObjectPools() {
        for (int i = 0; i < UIPrefabs.Length; i++) {
            GameObject currPrefab = UIPrefabs[i];
            EZObjectPool newUIPool = CreateNewPool(currPrefab, currPrefab.name, 0, true, true, false); //100
            newUIPool.transform.SetParent(UIObjectPoolParent.transform, false);
        }

        for (int i = 0; i < otherPrefabs.Length; i++) {
            GameObject currPrefab = otherPrefabs[i];
            CreateNewPool(currPrefab, currPrefab.name, 0, true, true, false); //50
        }

        ConstructGoapNodes();
        ConstructOpinionDataPool();
        ConstructTraitRemoveSchedulePool();
    }

    public GameObject InstantiateObjectFromPool(string poolName, Vector3 position, Quaternion rotation, Transform parent = null, bool isWorldPosition = false) {
        poolName = poolName.ToUpper();
        if (!allObjectPools.ContainsKey(poolName)) {
            throw new Exception($"Object Pool does not have key {poolName}");
        }
        GameObject instantiatedObj = null;
        EZObjectPool objectPoolToUse = allObjectPools[poolName];

        if(ReferenceEquals(objectPoolToUse, null)) {
            throw new Exception($"Cannot find an object pool with name {poolName}");
        } else {
            if(objectPoolToUse.TryGetNextObject(Vector3.zero, rotation, out instantiatedObj)) {
                if(ReferenceEquals(parent, null) == false) {
                    instantiatedObj.transform.SetParent(parent, false);
                }
                if (isWorldPosition) {
                    instantiatedObj.transform.position = position;
                } else {
                    instantiatedObj.transform.localPosition = position;    
                }
                
            }
        }
        instantiatedObj.SetActive(true);
        return instantiatedObj;
    }

    public void DestroyObject(PooledObject pooledObject) {
        PooledObject[] pooledObjects = pooledObject.GetComponents<PooledObject>();
        Messenger.Broadcast(Signals.POOLED_OBJECT_DESTROYED, pooledObject.gameObject);
        pooledObject.SendObjectBackToPool();
        for (int i = 0; i < pooledObjects.Length; i++) {
            pooledObjects[i].Reset();
        }
        pooledObject.transform.SetParent(pooledObject.ParentPool.transform);
    }
    public void DestroyObject(GameObject gameObject) {
        PooledObject[] pooledObjects = gameObject.GetComponents<PooledObject>();
        Messenger.Broadcast(Signals.POOLED_OBJECT_DESTROYED, gameObject);
        pooledObjects[0].SendObjectBackToPool();
        for (int i = 0; i < pooledObjects.Length; i++) {
            pooledObjects[i].Reset();
        }
        pooledObjects[0].transform.SetParent(pooledObjects[0].ParentPool.transform);
    }

    public EZObjectPool CreateNewPool(GameObject template, string poolName, int size, bool autoResize, bool instantiateImmediate, bool shared) {
        poolName = poolName.ToUpper();
        EZObjectPool newPool = EZObjectPool.CreateObjectPool(template, poolName, size, autoResize, instantiateImmediate, shared);
        //try {
            allObjectPools.Add(poolName, newPool);
        //}catch(Exception e) {
        //    throw new Exception(e.Message + " Pool name " + poolName);
        //}
        
        return newPool;
    }

    public bool HasPool(string key) {
        if (allObjectPools.ContainsKey(key)) {
            return true;
        }
        return false;
    }

    #region Goap Node
    private void ConstructGoapNodes() {
        goapNodesPool = new List<GoapNode>();
    }
    public GoapNode CreateNewGoapPlanJob(int cost, int level, GoapAction action, IPointOfInterest target) {
        GoapNode node = GetGoapNodeFromPool();
        node.Initialize(cost, level, action, target);
        return node;
    }
    public void ReturnGoapNodeToPool(GoapNode node) {
        node.Reset();
        goapNodesPool.Add(node);
    }
    private GoapNode GetGoapNodeFromPool() {
        if(goapNodesPool.Count > 0) {
            GoapNode node = goapNodesPool[0];
            goapNodesPool.RemoveAt(0);
            return node;
        }
        return new GoapNode();
    }
    #endregion

    #region Opinion Data
    private void ConstructOpinionDataPool() {
        opinionDataPool = new List<OpinionData>();
    }
    public OpinionData CreateNewOpinionData() {
        OpinionData data = GetOpinionDataFromPool();
        data.Initialize();
        return data;
    }
    public void ReturnOpinionDataToPool(OpinionData data) {
        data.Reset();
        opinionDataPool.Add(data);
    }
    private OpinionData GetOpinionDataFromPool() {
        if (opinionDataPool.Count > 0) {
            OpinionData data = opinionDataPool[0];
            opinionDataPool.RemoveAt(0);
            return data;
        }
        return new OpinionData();
    }
    #endregion

    #region Trait Remove Schedule
    private void ConstructTraitRemoveSchedulePool() {
        traitRemoveSchedulePool = new List<TraitRemoveSchedule>();
    }
    public TraitRemoveSchedule CreateNewTraitRemoveSchedule() {
        TraitRemoveSchedule data = GetTraitRemoveScheduleFromPool();
        data.Initialize();
        return data;
    }
    public void ReturnTraitRemoveScheduleToPool(TraitRemoveSchedule data) {
        data.Reset();
        traitRemoveSchedulePool.Add(data);
    }
    private TraitRemoveSchedule GetTraitRemoveScheduleFromPool() {
        if (traitRemoveSchedulePool.Count > 0) {
            TraitRemoveSchedule data = traitRemoveSchedulePool[0];
            traitRemoveSchedulePool.RemoveAt(0);
            return data;
        }
        return new TraitRemoveSchedule();
    }
    #endregion
}
