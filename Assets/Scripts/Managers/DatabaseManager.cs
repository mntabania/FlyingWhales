using System;
using System.Collections;
using System.Collections.Generic;
using Databases;
using Databases.SQLDatabase;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DatabaseManager : MonoBehaviour {
    public static DatabaseManager Instance;

    public AreaDatabase areaDatabase { get; private set; }
    public RegionDatabase regionDatabase { get; private set; }
    public CharacterDatabase characterDatabase { get; private set; }
    public FactionDatabase factionDatabase { get; private set; }
    public TileObjectDatabase tileObjectDatabase { get; private set; }
    public LocationGridTileDatabase locationGridTileDatabase { get; private set; }
    public SettlementDatabase settlementDatabase { get; private set; }
    public LocationStructureDatabase structureDatabase { get; private set; }
    public TraitDatabase traitDatabase { get; private set; }
    public BurningSourceDatabase burningSourceDatabase { get; private set; }
    public JobDatabase jobDatabase { get; private set; }
    public FamilyTreeDatabase familyTreeDatabase { get; private set; }
    public PartyDatabase partyDatabase { get; private set; }

    //These databases are only used when loading from a saved game, and therefore must be cleared out when loading is complete to save memory
    public ActionDatabase actionDatabase { get; private set; }
    public PartyQuestDatabase partyQuestDatabase { get; private set; }
    public GatheringDatabase gatheringDatabase { get; private set; }
    public InterruptDatabase interruptDatabase { get; private set; }
    public LogDatabase logDatabase { get; private set; }
    public CrimeDatabase crimeDatabase { get; private set; }
    
    //SQL Databases
    public RuinarchSQLDatabase mainSQLDatabase { get; private set; } 
    
    void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        } else {
            Destroy(gameObject);
        }
    }

    //Use this for initialization
    public void Initialize() {
        //Called in InitializeDataBeforeWorldCreation
        areaDatabase = new AreaDatabase();
        regionDatabase = new RegionDatabase();
        characterDatabase = new CharacterDatabase();
        factionDatabase = new FactionDatabase();
        tileObjectDatabase = new TileObjectDatabase();
        locationGridTileDatabase = new LocationGridTileDatabase();
        settlementDatabase = new SettlementDatabase();
        structureDatabase = new LocationStructureDatabase();
        traitDatabase = new TraitDatabase();
        burningSourceDatabase = new BurningSourceDatabase();
        jobDatabase = new JobDatabase();
        familyTreeDatabase = new FamilyTreeDatabase();
        actionDatabase = new ActionDatabase();
        interruptDatabase = new InterruptDatabase();
        logDatabase = new LogDatabase();
        partyDatabase = new PartyDatabase();
        partyQuestDatabase = new PartyQuestDatabase();
        crimeDatabase = new CrimeDatabase();
        gatheringDatabase = new GatheringDatabase();
        mainSQLDatabase = new RuinarchSQLDatabase();
    }

    #region Query
    public object GetObjectFromDatabase(System.Type type, string persistentID) {
        if (type == typeof(Character) || type.IsSubclassOf(typeof(Character))) {
            return characterDatabase.GetCharacterByPersistentID(persistentID);
        } else if (type == typeof(TileObject) || type.IsSubclassOf(typeof(TileObject))) {
            return tileObjectDatabase.GetTileObjectByPersistentIDSafe(persistentID);
        } else if (type == typeof(LocationStructure) || type.IsSubclassOf(typeof(LocationStructure))) {
            return structureDatabase.GetStructureByPersistentID(persistentID);
        } else if (type == typeof(Region)) {
            return regionDatabase.mainRegion;
        } else if (type == typeof(BaseSettlement) || type.IsSubclassOf(typeof(BaseSettlement))) {
            return settlementDatabase.GetSettlementByPersistentIDSafe(persistentID);
        } else if (type == typeof(Faction)) {
            return factionDatabase.GetFactionBasedOnPersistentID(persistentID);
        } else if (type == typeof(Party)) {
            return partyDatabase.GetPartyByPersistentIDSafe(persistentID);
        } else if (type == typeof(Area)) {
            return areaDatabase.GetAreaByPersistentID(persistentID);
        }
        return null;
    }
    #endregion

    public void ClearVolatileDatabases() {
        actionDatabase.allActions.Clear();
        interruptDatabase.allInterrupts.Clear();
        logDatabase.allLogs.Clear();
        //partyDatabase.allParties.Clear();
        partyQuestDatabase.allPartyQuests.Clear();
        crimeDatabase.allCrimes.Clear();
        locationGridTileDatabase.tileByGUID.Clear();
        locationGridTileDatabase.LocationGridTiles.Clear();
        gatheringDatabase.allGatherings.Clear();
        System.GC.Collect();
    }

    private void OnSceneUnloaded(Scene unloaded) {
#if DEBUG_LOG
        Debug.Log($"Scene {unloaded.name} was unloaded.");
#endif
        if (unloaded.name == "Game") {
            //TODO: Dispose of old databases.
            DisposeDatabases();
        }
    }

    private void DisposeDatabases() {
        mainSQLDatabase?.Dispose();
    }
    private void OnDestroy() {
        DisposeDatabases();
    }
}
