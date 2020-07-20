using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;

public class TreeObject : TileObject {
    public int yield { get; private set; }
    public override Character[] users => _users;
    
    private enum Occupied_State { Undecided, Occupied, Unoccupied }
    /// <summary>
    /// If this has value, then an ent is occupying this tree, and should be awakened when this tree is damaged.
    /// </summary>
    private Ent _ent;
    private Character[] _users;
    private Occupied_State _occupiedState;

    public TreeObject() {
        Initialize(TILE_OBJECT_TYPE.TREE_OBJECT, false);
        AddAdvertisedAction(INTERACTION_TYPE.CHOP_WOOD);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
        SetYield(100);
        _occupiedState = Occupied_State.Undecided;
    }
    public TreeObject(SaveDataTileObject data) {
        Initialize(data, false);
        AddAdvertisedAction(INTERACTION_TYPE.CHOP_WOOD);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
    }

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
        CombatManager.ElementalTraitProcessor elementalTraitProcessor = null, bool showHPBar = false) {
        LocationGridTile location = gridTileLocation;
        base.AdjustHP(amount, elementalDamageType, triggerDeath, source, elementalTraitProcessor, showHPBar);
        if (CanBeDamaged() && amount < 0) {
            //check if can awaken an ent
            switch (_occupiedState) {
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
        if (_ent != null) {
            _ent.marker.PlaceMarkerAt(gridTileLocation);
            _ent.marker.SetVisualState(false);
        }
    }
    #endregion

    #region Yield
    public void AdjustYield(int amount) {
        yield += amount;
        yield = Mathf.Max(0, yield);
        if (yield == 0 && gridTileLocation != null) {
            LocationGridTile loc = gridTileLocation;
            structureLocation.RemovePOI(this);
            SetGridTileLocation(loc); //so that it can still be targeted by aware characters.
        }
    }
    protected void SetYield(int amount) {
        yield = amount;
    }
    #endregion

    #region Occupant
    public void SetOccupyingEnt(Ent ent) {
        _occupiedState = Occupied_State.Occupied;
        _ent = ent;
        _users = new Character[] { ent };
    }
    private void RollForOccupant(LocationGridTile location) {
        if (GameUtilities.RollChance(3)) {
            //tree has occupant. Spawn Ent
            SUMMON_TYPE entType;
            if (location.isCorrupted) {
                entType = SUMMON_TYPE.Corrupt_Ent;
            } else {
                BIOMES biome = location.parentMap.region.coreTile.biomeType;
                if (location.collectionOwner.isPartOfParentRegionMap) {
                    biome = location.collectionOwner.partOfHextile.hexTileOwner.biomeType;
                }
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
            CharacterManager.Instance.PlaceSummon(ent, location);
            ent.AddTerritory(location.collectionOwner.GetNearestHexTileWithinRegion());
            TraitManager.Instance.CopyStatuses(this, ent);
            location.structure.RemovePOI(this);
        } else {
            //tree has no occupant, set that.
            _occupiedState = Occupied_State.Unoccupied;
        }
    }
    private void AwakenOccupant(LocationGridTile location) {
        Assert.IsNotNull(_ent);
        _ent.marker.SetVisualState(true);
        _ent.marker.PlaceMarkerAt(location);
        _ent.SetIsTree(false);
        location.structure.RemovePOI(this);
        TraitManager.Instance.CopyStatuses(this, _ent);
    }
    #endregion
}

//public class SaveDataTreeObject: SaveDataTileObject {
//    public int yield;

//    public override void Save(TileObject tileObject) {
//        base.Save(tileObject);
//        TreeObject obj = tileObject as TreeObject;
//        yield = obj.yield;
//    }

//    public override TileObject Load() {
//        TreeObject obj = base.Load() as TreeObject;
//        obj.SetYield(yield);
//        return obj;
//    }
//}