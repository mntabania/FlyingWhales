using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;

public class TreasureChest : TileObject {
    
    public IPointOfInterest objectInside { get; private set; }
    private Character[] _users;
    
    public override Character[] users => _users;
    public override System.Type serializedData => typeof(SaveDataTreasureChest);
    
    private readonly TILE_OBJECT_TYPE[] _possibleItems = new[] {
        TILE_OBJECT_TYPE.HEALING_POTION,
        TILE_OBJECT_TYPE.POISON_FLASK,
        TILE_OBJECT_TYPE.ANTIDOTE,
        TILE_OBJECT_TYPE.EMBER,
        TILE_OBJECT_TYPE.ICE,
        TILE_OBJECT_TYPE.WOOD_PILE,
        TILE_OBJECT_TYPE.STONE_PILE,
        TILE_OBJECT_TYPE.METAL_PILE,
        TILE_OBJECT_TYPE.ANIMAL_MEAT,
    };
    
    public TreasureChest() {
        Initialize(TILE_OBJECT_TYPE.TREASURE_CHEST, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
        AddAdvertisedAction(INTERACTION_TYPE.OPEN);
    }
    public TreasureChest(SaveDataTreasureChest data) : base(data) { }

    #region Loading
    public override void LoadAdditionalInfo(SaveDataTileObject data) {
        base.LoadAdditionalInfo(data);
        SaveDataTreasureChest saveDataTreasureChest = data as SaveDataTreasureChest;
        Assert.IsNotNull(saveDataTreasureChest);
        if (!string.IsNullOrEmpty(saveDataTreasureChest.objectInsideID) && gridTileLocation != null) {
            if (saveDataTreasureChest.objectInsideType == OBJECT_TYPE.Character) {
                Character character = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(saveDataTreasureChest.objectInsideID);
                if (character != null) {
                    SetObjectInside(character);
                    if (character.hasMarker) {
                        character.marker.PlaceMarkerAt(gridTileLocation);
                        character.DisableMarker();
                    }    
                }
            } else if (saveDataTreasureChest.objectInsideType == OBJECT_TYPE.Tile_Object) {
                TileObject tileObject = DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentID(saveDataTreasureChest.objectInsideID);
                SetObjectInside(tileObject);
            }
        }
    }
    #endregion
    
    protected override string GenerateName() { return "Treasure Chest"; }
    public override void OnDoActionToObject(ActualGoapNode action) {
        if (action.goapType == INTERACTION_TYPE.OPEN) {
            RollForItem(gridTileLocation);
        }
    }
    public override void AdjustHP(int amount, ELEMENTAL_TYPE elementalDamageType, bool triggerDeath = false, object source = null,
        CombatManager.ElementalTraitProcessor elementalTraitProcessor = null, bool showHPBar = false) {
        LocationGridTile location = gridTileLocation;
        base.AdjustHP(amount, elementalDamageType, triggerDeath, source, elementalTraitProcessor, showHPBar);
        if (CanBeDamaged() && amount < 0) {
            //check if can awaken a mimic
            if (objectInside == null) {
                //no object inside yet, roll fot item
                RollForItem(location);
                //if it is mimic then spawn the mimic now.
                if (objectInside is Mimic summon) {
                    SpawnInitialMimic(location, summon);
                    location.structure.RemovePOI(this);
                }
            } else if (objectInside is Mimic summon) {
                //if there is an object inside the chest and it is a mimic, awaken it
                if (!summon.isDead) {
                    summon.marker.PlaceMarkerAt(location);
                    summon.SetIsTreasureChest(false);    
                }
                location.structure.RemovePOI(this);
                TraitManager.Instance.CopyStatuses(this, summon);
            }
        }
    }
    public override void OnPlacePOI() {
        base.OnPlacePOI();
        if (objectInside is Character character) {
            character.marker.PlaceMarkerAt(gridTileLocation);
            character.DisableMarker();
        }
    }
    #region Object Inside
    private void RollForItem(LocationGridTile locationGridTile) {
        if (objectInside != null) { return; } //already has object inside
        if (GameUtilities.RollChance(5)) { //5
            Summon summon = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Mimic, FactionManager.Instance.neutralFaction, homeRegion: locationGridTile.parentMap.region);
            SetObjectInside(summon);
        } else {
            TILE_OBJECT_TYPE chosenType = CollectionUtilities.GetRandomElement(_possibleItems);
            SetObjectInside(InnerMapManager.Instance.CreateNewTileObject<TileObject>(chosenType));    
        }
    }
    public void SetObjectInside(IPointOfInterest pointOfInterest) {
        objectInside = pointOfInterest;
        if (objectInside is Character character) {
            _users = new[] { character };
        }
    }
    public void SpawnInitialMimic(LocationGridTile tile, Summon summon) {
        CharacterManager.Instance.PlaceSummonInitially(summon, tile);
        summon.SetTerritory(tile.GetNearestHexTileWithinRegion());
        TraitManager.Instance.CopyStatuses(this, summon);
    }
    #endregion
}

#region Save Data
public class SaveDataTreasureChest : SaveDataTileObject {
    public OBJECT_TYPE objectInsideType;
    public string objectInsideID;
    public override void Save(TileObject data) {
        base.Save(data);
        TreasureChest treasureChest = data as TreasureChest;
        Assert.IsNotNull(treasureChest);
        if (treasureChest.objectInside != null) {
            objectInsideType = treasureChest.objectInside.objectType;
            objectInsideID = treasureChest.objectInside.persistentID;    
        }
    }
}
#endregion