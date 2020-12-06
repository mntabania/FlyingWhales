﻿using UnityEngine;
using System.Collections;
using EZObjectPools;
using System.Collections.Generic;
using System;
using System.Linq;
using Interrupts;
using Inner_Maps.Location_Structures;
using Inner_Maps;
using Locations.Settlements;

public class ObjectPoolManager : BaseMonoBehaviour {

    public static ObjectPoolManager Instance = null;

    private Dictionary<string, EZObjectPool> allObjectPools;

    [SerializeField] private GameObject[] UIPrefabs;
    [SerializeField] internal GameObject[] otherPrefabs;
    [SerializeField] private GameObject UIObjectPoolParent;

    public List<GoapNode> goapNodesPool { get; private set; }
    public List<OpinionData> opinionDataPool { get; private set; }
    public List<TraitRemoveSchedule> traitRemoveSchedulePool { get; private set; }
    public List<CombatData> combatDataPool { get; private set; }
    public List<InterruptHolder> _interruptPool { get; private set; }
    public List<Party> _partyPool { get; private set; }
    public List<GoapThread> _goapThreadPool { get; private set; }
    private List<LogDatabaseThread> _logDatabaseThreadPool;
    private List<List<GoapEffect>> _expectedEffectsListPool;
    private List<List<Precondition>> _preconditionsListPool;
    private List<List<Character>> _characterListPool;
    private List<List<HexTile>> _hexTileListPool;
    private List<List<LocationStructure>> _structureListPool;
    private List<List<LocationGridTile>> _tileListPool;
    private List<List<Faction>> _factionListPool;
    private List<List<BaseSettlement>> _settlementListPool;
    private List<GoapPlanJob> _goapJobPool;
    private List<CharacterStateJob> _stateJobPool;
    private List<ConversationData> _conversationDataPool;
    private List<List<ConversationData>> _conversationDataListPool;

    private void Awake() {
        Instance = this;
        allObjectPools = new Dictionary<string, EZObjectPool>();
    }
    public void InitializeObjectPools() {
        for (int i = 0; i < UIPrefabs.Length; i++) {
            GameObject currPrefab = UIPrefabs[i];
            int size = 0;
            if (currPrefab.name == "LogHistoryItem") {
                size = 2000; //automatically create 200 log history items for performance in game
            }
            EZObjectPool newUIPool = CreateNewPool(currPrefab, currPrefab.name, size, true, true, false); //100
            newUIPool.transform.SetParent(UIObjectPoolParent.transform, false);
        }

        for (int i = 0; i < otherPrefabs.Length; i++) {
            GameObject currPrefab = otherPrefabs[i];
            CreateNewPool(currPrefab, currPrefab.name, 0, true, true, false); //50    
        }

        ConstructGoapNodes();
        ConstructOpinionDataPool();
        ConstructTraitRemoveSchedulePool();
        ConstructCombatDataPool();
        ConstructInterruptPool();
        ConstructPartyPool();
        ConstructGoapThreadPool();
        ConstructLogDatabaseThreadPool();
        ConstructExpectedEffectsListPool();
        ConstructPreconditionListPool();
        ConstructCharacterListPool();
        ConstructHexTileListPool();
        ConstructStructureListPool();
        ConstructGridTileListPool();
        ConstructFactionListPool();
        ConstructSettlementListPool();
        ConstructJobPool();
        ConstructConversationPool();
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
                instantiatedObj.transform.localScale = objectPoolToUse.Template.transform.localScale;
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
    public GameObject GetOriginalObjectFromPool(string poolName) {
        poolName = poolName.ToUpper();
        if (!allObjectPools.ContainsKey(poolName)) {
            throw new Exception($"Object Pool does not have key {poolName}");
        }
        EZObjectPool objectPoolToUse = allObjectPools[poolName];
        return objectPoolToUse.Template;
    }
    
    public void DestroyObject(PooledObject pooledObject) {
        PooledObject[] pooledObjects = pooledObject.GetComponents<PooledObject>();
        Messenger.Broadcast(ObjectPoolSignals.POOLED_OBJECT_DESTROYED, pooledObject.gameObject);
        pooledObject.BeforeDestroyActions();
        for (int i = 0; i < pooledObjects.Length; i++) {
            pooledObjects[i].BeforeDestroyActions();
        }
        pooledObject.SendObjectBackToPool();
        for (int i = 0; i < pooledObjects.Length; i++) {
            pooledObjects[i].Reset();
        }
        pooledObject.transform.SetParent(pooledObject.ParentPool.transform);
    }
    public void DestroyObject(GameObject gameObject) {
        PooledObject[] pooledObjects = gameObject.GetComponents<PooledObject>();
        Messenger.Broadcast(ObjectPoolSignals.POOLED_OBJECT_DESTROYED, gameObject);
        for (int i = 0; i < pooledObjects.Length; i++) {
            pooledObjects[i].BeforeDestroyActions();
        }
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

    #region Combat Data
    private void ConstructCombatDataPool() {
        combatDataPool = new List<CombatData>();
    }
    public CombatData CreateNewCombatData() {
        CombatData data = GetCombatDataFromPool();
        data.Initialize();
        return data;
    }
    public void ReturnCombatDataToPool(CombatData data) {
        data.Reset();
        combatDataPool.Add(data);
    }
    private CombatData GetCombatDataFromPool() {
        if (combatDataPool.Count > 0) {
            CombatData data = combatDataPool[0];
            combatDataPool.RemoveAt(0);
            return data;
        }
        return new CombatData();
    }
    #endregion

    #region Interrupts
    private void ConstructInterruptPool() {
        _interruptPool = new List<InterruptHolder>();
    }
    public InterruptHolder CreateNewInterrupt() {
        if (_interruptPool.Count > 0) {
            InterruptHolder data = _interruptPool[0];
            _interruptPool.RemoveAt(0);
            return data;
        }
        return new InterruptHolder();
    }
    public void ReturnInterruptToPool(InterruptHolder data) {
        if (data.shouldNotBeObjectPooled) {
            return;
        }
        data.Reset();
        _interruptPool.Add(data);
    }
    #endregion

    #region Party
    private void ConstructPartyPool() {
        _partyPool = new List<Party>();
    }
    public Party CreateNewParty() {
        if (_partyPool.Count > 0) {
            Party data = _partyPool[0];
            _partyPool.RemoveAt(0);
            return data;
        }
        return new Party();
    }
    public void ReturnPartyToPool(Party data) {
        data.Reset();
        _partyPool.Add(data);
    }
    #endregion
    
    #region Database Thread
    private void ConstructLogDatabaseThreadPool() {
        _logDatabaseThreadPool = new List<LogDatabaseThread>();
    }
    public LogDatabaseThread CreateNewLogDatabaseThread() {
        if (_logDatabaseThreadPool.Count > 0) {
            LogDatabaseThread data = _logDatabaseThreadPool[0];
            _logDatabaseThreadPool.RemoveAt(0);
            return data;
        }
        return new LogDatabaseThread();
    }
    public void ReturnLogDatabaseThreadToPool(LogDatabaseThread data) {
        data.Reset();
        _logDatabaseThreadPool.Add(data);
    }
    #endregion

    #region Goap Thread
    private void ConstructGoapThreadPool() {
        _goapThreadPool = new List<GoapThread>();
    }
    public GoapThread CreateNewGoapThread() {
        if (_goapThreadPool.Count > 0) {
            GoapThread data = _goapThreadPool[0];
            _goapThreadPool.RemoveAt(0);
            return data;
        }
        return new GoapThread();
    }
    public void ReturnGoapThreadToPool(GoapThread data) {
        data.Reset();
        _goapThreadPool.Add(data);
    }
    #endregion

    #region Goap Action Expected Effects
    private void ConstructExpectedEffectsListPool() {
        _expectedEffectsListPool = new List<List<GoapEffect>>();
    }
    public List<GoapEffect> CreateNewExpectedEffectsList() {
        if (_expectedEffectsListPool.Count > 0) {
            List<GoapEffect> data = _expectedEffectsListPool[0];
            _expectedEffectsListPool.RemoveAt(0);
            return data;
        }
        return new List<GoapEffect>();
    }
    public void ReturnExpectedEffectsListToPool(List<GoapEffect> data) {
        data.Clear();
        _expectedEffectsListPool.Add(data);
    }
    #endregion

    #region Goap Action Preconditions
    private void ConstructPreconditionListPool() {
        _preconditionsListPool = new List<List<Precondition>>();
    }
    public List<Precondition> CreateNewPreconditionsList() {
        if (_preconditionsListPool.Count > 0) {
            List<Precondition> data = _preconditionsListPool[0];
            _preconditionsListPool.RemoveAt(0);
            return data;
        }
        return new List<Precondition>();
    }
    public void ReturnPreconditionsListToPool(List<Precondition> data) {
        data.Clear();
        _preconditionsListPool.Add(data);
    }
    #endregion

    #region Characters
    private void ConstructCharacterListPool() {
        _characterListPool = new List<List<Character>>();
    }
    public List<Character> CreateNewCharactersList() {
        if (_characterListPool.Count > 0) {
            List<Character> data = _characterListPool[0];
            _characterListPool.RemoveAt(0);
            return data;
        }
        return new List<Character>();
    }
    public void ReturnCharactersListToPool(List<Character> data) {
        data.Clear();
        _characterListPool.Add(data);
    }
    #endregion

    #region HexTiles
    private void ConstructHexTileListPool() {
        _hexTileListPool = new List<List<HexTile>>();
    }
    public List<HexTile> CreateNewHexTilesList() {
        if (_hexTileListPool.Count > 0) {
            List<HexTile> data = _hexTileListPool[0];
            _hexTileListPool.RemoveAt(0);
            return data;
        }
        return new List<HexTile>();
    }
    public void ReturnHexTilesListToPool(List<HexTile> data) {
        data.Clear();
        _hexTileListPool.Add(data);
    }
    #endregion

    #region Structures
    private void ConstructStructureListPool() {
        _structureListPool = new List<List<LocationStructure>>();
    }
    public List<LocationStructure> CreateNewStructuresList() {
        if (_structureListPool.Count > 0) {
            List<LocationStructure> data = _structureListPool[0];
            _structureListPool.RemoveAt(0);
            return data;
        }
        return new List<LocationStructure>();
    }
    public void ReturnStructuresListToPool(List<LocationStructure> data) {
        data.Clear();
        _structureListPool.Add(data);
    }
    #endregion

    #region Location Grid Tile
    private void ConstructGridTileListPool() {
        _tileListPool = new List<List<LocationGridTile>>();
    }
    public List<LocationGridTile> CreateNewGridTileList() {
        if (_tileListPool.Count > 0) {
            List<LocationGridTile> data = _tileListPool[0];
            _tileListPool.RemoveAt(0);
            return data;
        }
        return new List<LocationGridTile>();
    }
    public void ReturnGridTileListToPool(List<LocationGridTile> data) {
        data.Clear();
        _tileListPool.Add(data);
    }
    #endregion

    #region Faction
    private void ConstructFactionListPool() {
        _factionListPool = new List<List<Faction>>();
    }
    public List<Faction> CreateNewFactionList() {
        if (_factionListPool.Count > 0) {
            List<Faction> data = _factionListPool[0];
            _factionListPool.RemoveAt(0);
            return data;
        }
        return new List<Faction>();
    }
    public void ReturnFactionListToPool(List<Faction> data) {
        data.Clear();
        _factionListPool.Add(data);
    }
    #endregion

    #region Settlement
    private void ConstructSettlementListPool() {
        _settlementListPool = new List<List<BaseSettlement>>();
    }
    public List<BaseSettlement> CreateNewSettlementList() {
        if (_settlementListPool.Count > 0) {
            List<BaseSettlement> data = _settlementListPool[0];
            _settlementListPool.RemoveAt(0);
            return data;
        }
        return new List<BaseSettlement>();
    }
    public void ReturnSettlementListToPool(List<BaseSettlement> data) {
        data.Clear();
        _settlementListPool.Add(data);
    }
    #endregion

    #region Jobs
    private void ConstructJobPool() {
        _goapJobPool = new List<GoapPlanJob>();
        _stateJobPool = new List<CharacterStateJob>();
    }
    public GoapPlanJob CreateNewGoapPlanJob() {
        if (_goapJobPool.Count > 0) {
            GoapPlanJob job = _goapJobPool[0];
            _goapJobPool.RemoveAt(0);
            return job;
        }
        return new GoapPlanJob();
    }
    public void ReturnGoapPlanJobToPool(GoapPlanJob job) {
        job.Reset();
        _goapJobPool.Add(job);
    }
    public CharacterStateJob CreateNewCharacterStateJob() {
        if (_stateJobPool.Count > 0) {
            CharacterStateJob job = _stateJobPool[0];
            _stateJobPool.RemoveAt(0);
            return job;
        }
        return new CharacterStateJob();
    }
    public void ReturnCharacterStateJobToPool(CharacterStateJob job) {
        job.Reset();
        _stateJobPool.Add(job);
    }
    #endregion

    #region Conversation
    private void ConstructConversationPool() {
        _conversationDataPool = new List<ConversationData>();
        _conversationDataListPool = new List<List<ConversationData>>();
    }
    public ConversationData CreateNewConversationData(string text, Character character, DialogItem.Position position) {
        ConversationData data = CreateNewConversationData();
        data.text = text;
        data.character = character;
        data.position = position;
        return data;
    }
    public ConversationData CreateNewConversationData() {
        if (_conversationDataPool.Count > 0) {
            ConversationData data = _conversationDataPool[0];
            _conversationDataPool.RemoveAt(0);
            return data;
        }
        return new ConversationData();
    }
    public void ReturnConversationDataToPool(ConversationData data) {
        data.Reset();
        _conversationDataPool.Add(data);
    }
    public List<ConversationData> CreateNewConversationDataList() {
        if (_conversationDataListPool.Count > 0) {
            List<ConversationData> data = _conversationDataListPool[0];
            _conversationDataListPool.RemoveAt(0);
            return data;
        }
        return new List<ConversationData>();
    }
    public void ReturnConversationDataListToPool(List<ConversationData> data) {
        data.Clear();
        _conversationDataListPool.Add(data);
    }
    #endregion

    protected override void OnDestroy() {
        if (allObjectPools != null) {
            foreach (KeyValuePair<string,EZObjectPool> pool in allObjectPools) {
                pool.Value.ClearPool();
            }
            allObjectPools.Clear();
            allObjectPools = null;
        }
        goapNodesPool?.Clear();
        goapNodesPool = null;
        opinionDataPool?.Clear();
        opinionDataPool = null;
        traitRemoveSchedulePool?.Clear();
        traitRemoveSchedulePool = null;
        combatDataPool?.Clear();
        combatDataPool = null;
        _interruptPool?.Clear();
        _interruptPool = null;
        _goapThreadPool?.Clear();
        _goapThreadPool = null;
        _partyPool?.Clear();
        _partyPool = null;
        _logDatabaseThreadPool?.Clear();
        _logDatabaseThreadPool = null;
        _expectedEffectsListPool?.Clear();
        _expectedEffectsListPool = null;
        _preconditionsListPool?.Clear();
        _preconditionsListPool = null;
        _characterListPool?.Clear();
        _characterListPool = null;
        _hexTileListPool?.Clear();
        _hexTileListPool = null;
        _structureListPool?.Clear();
        _structureListPool = null;
        _tileListPool?.Clear();
        _tileListPool = null;
        _factionListPool?.Clear();
        _factionListPool = null;
        _settlementListPool?.Clear();
        _settlementListPool = null;
        _goapJobPool?.Clear();
        _goapJobPool = null;
        _stateJobPool?.Clear();
        _stateJobPool = null;
        _conversationDataPool?.Clear();
        _conversationDataPool = null;
        _conversationDataListPool?.Clear();
        _conversationDataListPool = null;
        base.OnDestroy();
        Instance = null;
    }
}
