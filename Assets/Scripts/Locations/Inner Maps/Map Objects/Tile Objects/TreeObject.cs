using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Inner_Maps.Map_Objects.Map_Object_Visuals;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;
using Locations.Settlements;

public class TreeObject : TileObject {
    public int yield { get; private set; }
    public BaseSettlement parentSettlement { get; private set; }
    public override Character[] users => _users;
    
    public enum Occupied_State { Undecided, Occupied, Unoccupied }
    /// <summary>
    /// If this has value, then an ent is occupying this tree, and should be awakened when this tree is damaged.
    /// </summary>
    private Ent _ent;
    private Character[] _users;
    private Occupied_State _occupiedState;
    public override System.Type serializedData => typeof(SaveDataTreeObject);
    public StructureConnector structureConnector {
        get {
            if (_treeGameObject != null) {
                return _treeGameObject.structureConnector;
            }
            return null;
        }
    }
    public Occupied_State occupiedState => _occupiedState;
    public Ent ent => _ent;

    private TreeGameObject _treeGameObject;

    public TreeObject() {
        Initialize(TILE_OBJECT_TYPE.TREE_OBJECT, false);
        AddAdvertisedAction(INTERACTION_TYPE.CHOP_WOOD);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
        SetYield(100);
        _occupiedState = Occupied_State.Undecided;
    }
    public TreeObject(SaveDataTreeObject data) : base(data) {
        //SaveDataTreeObject saveDataTreeObject = data as SaveDataTreeObject;
        Assert.IsNotNull(data);
        yield = data.yield;
        _occupiedState = data.occupiedState;
    }
    protected override void UpdateSettlementResourcesParent() {
        if (gridTileLocation.area.settlementOnArea != null) {
            gridTileLocation.area.settlementOnArea.SettlementResources?.AddToListBasedOnRequirement(SettlementResources.StructureRequirement.TREE, this);
        }
        gridTileLocation.area.neighbourComponent.neighbours.ForEach((eachNeighbor) => {
            if (eachNeighbor.settlementOnArea != null) {
                eachNeighbor.settlementOnArea.SettlementResources?.AddToListBasedOnRequirement(SettlementResources.StructureRequirement.TREE, this);
               parentSettlement = eachNeighbor.settlementOnArea;
            }
        });
    }
    protected override void RemoveFromSettlementResourcesParent() {
        if (parentSettlement != null && parentSettlement.SettlementResources != null) {
            if (parentSettlement.SettlementResources.trees.Remove(this)) {
                parentSettlement = null;
            }    
        }
        
    }

    #region Loading
    public override void LoadSecondWave(SaveDataTileObject data) {
        base.LoadSecondWave(data);
        SaveDataTreeObject saveDataTreeObject = data as SaveDataTreeObject;
        Assert.IsNotNull(saveDataTreeObject);
        if (!string.IsNullOrEmpty(saveDataTreeObject.occupyingEntID) && gridTileLocation != null) {
            Character character = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(saveDataTreeObject.occupyingEntID);
            if (character is Ent loadedEnt) {
                SetOccupyingEnt(loadedEnt);
            } else {
                //no ent was found, make sure to set occupied state to undecided again, NOTE: doubt that this will ever happen
                _occupiedState = Occupied_State.Undecided;
            }    
        }
    }
    public override void LoadAdditionalInfo(SaveDataTileObject data) {
        base.LoadAdditionalInfo(data);
        if (ent != null && ent.hasMarker && gridTileLocation != null) {
            ent.marker.PlaceMarkerAt(gridTileLocation);
            ent.marker.SetVisualState(false);
        }
    }
    #endregion

    #region Overrides
    public override string ToString() {
        return $"Tree {id.ToString()}";
    }
    protected override string GenerateName() { return "Tree"; }
    public override string GetAdditionalTestingData() {
        string data = base.GetAdditionalTestingData();
        data = $"{data}\n\tYield: {yield.ToString()}";
        return data;
    }
    public override void AdjustHP(int amount, ELEMENTAL_TYPE elementalDamageType, bool triggerDeath = false, object source = null,
        CombatManager.ElementalTraitProcessor elementalTraitProcessor = null, bool showHPBar = false, float piercingPower = 0f, bool isPlayerSource = false) {
        LocationGridTile location = gridTileLocation;
        base.AdjustHP(amount, elementalDamageType, triggerDeath, source, elementalTraitProcessor, showHPBar, piercingPower, isPlayerSource);
        if (CanBeDamaged() && amount < 0) {
            //check if can awaken an ent
            switch (occupiedState) {
                case Occupied_State.Occupied:
                    //there is an ent occupying this tree, awaken it.
                    AwakenOccupant(location);
                    break;
                case Occupied_State.Undecided:
                    //occupied state has not been decided, yet. Decide now.
                    RollForOccupant(location);
                    break;
                case Occupied_State.Unoccupied:
                    //there is no ent occupying this tree, 
                    break;
            }
        }
    }
    public override void OnPlacePOI() {
        base.OnPlacePOI();
        if (ent != null) {
            ent.marker.PlaceMarkerAt(gridTileLocation);
            ent.marker.SetVisualState(false);
        }
        if (structureConnector != null && gridTileLocation != null) {
            structureConnector.OnPlaceConnector(gridTileLocation.parentMap);    
        }
        UpdateSettlementResourcesParent();
    }
    public override void OnLoadPlacePOI() {
        DefaultProcessOnPlacePOI();
        if (ent != null) {
            ent.marker.PlaceMarkerAt(gridTileLocation);
            ent.marker.SetVisualState(false);
        }
        if (structureConnector != null && gridTileLocation != null) {
            structureConnector.LoadConnectorForTileObjects(gridTileLocation.parentMap);    
        }
        UpdateSettlementResourcesParent();
    }
    protected override void CreateMapObjectVisual() {
        base.CreateMapObjectVisual();
        _treeGameObject = mapVisual as TreeGameObject;
    }
    public override void DestroyMapVisualGameObject() {
        base.DestroyMapVisualGameObject();
        _treeGameObject = null;
    }
    #endregion

    #region Yield
    public void AdjustYield(int amount) {
        yield += amount;
        yield = Mathf.Max(0, yield);
        if (yield == 0 && gridTileLocation != null) {
            structureLocation.RemovePOI(this);
        }
    }
    public void SetYield(int amount) {
        yield = amount;
    }
    #endregion

    #region Occupant
    public void SetOccupyingEnt(Ent p_ent) {
        _occupiedState = Occupied_State.Occupied;
        _ent = p_ent;
        _users = new Character[] { p_ent };
        if (_ent != null) {
            _ent.SubscribeToAwakenEntEvent(this);
        }
    }
    private void RemoveOccupyingEnt() {
        if (_ent != null) {
#if DEBUG_LOG
            Debug.Log($"Removed Occupying ent of {name} {id.ToString()}");
#endif
            //if previous value is not null and will set it to null, unsubscribe from previous ent
            _ent.UnsubscribeToAwakenEntEvent(this);
            _ent = null;
        }
    }
    private void RollForOccupant(LocationGridTile location) {
        if (ChanceData.RollChance(CHANCE_TYPE.Ent_Spawn)) {//3
            //tree has occupant. Spawn Ent
            SUMMON_TYPE entType;
            if (location.corruptionComponent.isCorrupted) {
                entType = SUMMON_TYPE.Corrupt_Ent;
            } else {
                BIOMES biome = location.mainBiomeType;
                switch (biome) {
                    case BIOMES.DESERT:
                        entType = SUMMON_TYPE.Desert_Ent;
                        break;
                    case BIOMES.FOREST:
                        entType = SUMMON_TYPE.Forest_Ent;
                        break;
                    case BIOMES.GRASSLAND:
                        entType = SUMMON_TYPE.Grass_Ent;
                        break;
                    case BIOMES.SNOW:
                    case BIOMES.TUNDRA:
                        entType = SUMMON_TYPE.Snow_Ent;
                        break;
                    default:
                        entType = SUMMON_TYPE.Grass_Ent;
                        break;
                }
            }
            Ent ent = CharacterManager.Instance.CreateNewSummon(entType, FactionManager.Instance.neutralFaction, homeRegion: location.parentMap.region) as Ent;
            Assert.IsNotNull(ent);
            CharacterManager.Instance.PlaceSummonInitially(ent, location);
            ent.SetTerritory(location.GetNearestHexTileWithinRegion());
            TraitManager.Instance.CopyStatuses(this, ent);
            location.structure.RemovePOI(this);
            RemoveOccupyingEnt();
        } else {
            //tree has no occupant, set that.
            _occupiedState = Occupied_State.Unoccupied;
        }
    }
    private void AwakenOccupant(LocationGridTile location) {
        Assert.IsNotNull(ent);
        if (!ent.isDead) {
            if (!ent.hasMarker) {
                ent.CreateMarker();    
            }
            ent.marker.SetVisualState(true);
            ent.marker.PlaceMarkerAt(location);
            ent.SetIsTree(false);    
        }
        location.structure.RemovePOI(this);
        TraitManager.Instance.CopyStatuses(this, ent);
        RemoveOccupyingEnt();
    }
    public void TryAwakenEnt(Ent p_ent) {
        if (ent == p_ent && p_ent.gridTileLocation != null) {
            AwakenOccupant(p_ent.gridTileLocation);
        }
    }
#endregion
}

#region Save Data
public class SaveDataTreeObject : SaveDataTileObject {
    
    public TreeObject.Occupied_State occupiedState;
    public string occupyingEntID;
    public int yield;
    
    public override void Save(TileObject tileObject) {
        base.Save(tileObject);
        TreeObject treeObject = tileObject as TreeObject;
        Assert.IsNotNull(treeObject);
        yield = treeObject.yield;
        occupiedState = treeObject.occupiedState;
        if (treeObject.ent != null) {
            occupyingEntID = treeObject.ent.persistentID;
        }
    }
    public override TileObject Load() {
        TileObject tileObject = base.Load();
        return tileObject;
    }
}
#endregion