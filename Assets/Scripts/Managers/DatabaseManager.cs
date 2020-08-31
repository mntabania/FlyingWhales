using System;
using System.Collections;
using System.Collections.Generic;
using Databases;
using UnityEngine;

public class DatabaseManager : MonoBehaviour {
    public static DatabaseManager Instance;

    public HexTileDatabase hexTileDatabase { get; private set; }
    public RegionDatabase regionDatabase { get; private set; }
    public CharacterDatabase characterDatabase { get; private set; }
    public FactionDatabase factionDatabase { get; private set; }
    public TileObjectDatabase tileObjectDatabase { get; private set; }
    public LocationGridTileDatabase locationGridTileDatabase { get; private set; }
    public SettlementDatabase settlementDatabase { get; private set; }
    public LocationStructureDatabase structureDatabase { get; private set; }
    public TraitDatabase traitDatabase { get; private set; }
    public BurningSourceDatabase burningSourceDatabase { get; private set; }
    
    void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    //Use this for initialization
    public void Initialize() {
        //Called in InitializeDataBeforeWorldCreation
        hexTileDatabase = new HexTileDatabase();
        regionDatabase = new RegionDatabase();
        characterDatabase = new CharacterDatabase();
        factionDatabase = new FactionDatabase();
        tileObjectDatabase = new TileObjectDatabase();
        locationGridTileDatabase = new LocationGridTileDatabase();
        settlementDatabase = new SettlementDatabase();
        structureDatabase = new LocationStructureDatabase();
        traitDatabase = new TraitDatabase();
        burningSourceDatabase = new BurningSourceDatabase();
    }
}
