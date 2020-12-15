using System;
using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Pathfinding.Util;
using UnityEngine;
using Traits;
using UnityEngine.Assertions;
using Debug = System.Diagnostics.Debug;

public class GenericTileObject : TileObject {
    private bool hasBeenInitialized { get; set; }
    public LocationStructureObject blueprintOnTile { get; private set; }
    public GameDate blueprintExpiryDate { get; private set; }
    public bool isCurrentlyBuilding { get; private set; }
    private LocationGridTile _owner;
    private string _expiryKey;

    #region getters
    public override Type serializedData => typeof(SaveDataGenericTileObject);
    public override LocationGridTile gridTileLocation => _owner;
    #endregion
    
    public GenericTileObject(LocationGridTile locationGridTile) : base() {
        SetTileOwner(locationGridTile);
    }
    public GenericTileObject(SaveDataGenericTileObject data) : base(data) { }

    #region Override
    public override void OnRemoveTileObject(Character removedBy, LocationGridTile removedFrom, bool removeTraits = true, bool destroyTileSlots = true) {
        Messenger.Broadcast(GridTileSignals.TILE_OBJECT_REMOVED, this as TileObject, removedBy, removedFrom, destroyTileSlots);
        if (hasCreatedSlots && destroyTileSlots) {
            DestroyTileSlots();
        }
    }
    public override void OnPlacePOI() {
        SetPOIState(POI_STATE.ACTIVE);
    }
    protected override string GenerateName() {
#if UNITY_EDITOR
        return $"the floor at {gridTileLocation}";
#else
        return "the floor";
#endif
        
    }
    protected override void OnPlaceTileObjectAtTile(LocationGridTile tile) { } //overridden this to reduce unnecessary processing 
    public override void OnDestroyPOI() {
        DisableGameObject();
        OnRemoveTileObject(null, previousTile);
        SetPOIState(POI_STATE.INACTIVE);
    }
    public override void RemoveTileObject(Character removedBy) {
        LocationGridTile previousTileLocation = gridTileLocation;
        DisableGameObject();
        OnRemoveTileObject(removedBy, previousTileLocation);
        SetPOIState(POI_STATE.INACTIVE);
    }
    public override bool IsValidCombatTargetFor(IPointOfInterest source) {
        return false;
    }
    public override void OnTileObjectGainedTrait(Trait trait) {
        if (trait is Status status) {
            if(status.isTangible) {
                //if status is wet, and this tile is not part of a settlement, then do not create a map visual, since
                //characters do not react to wet tiles outside their settlement.
                bool willCreateVisual = !(status is Wet && gridTileLocation.IsPartOfSettlement() == false);
                if (willCreateVisual) {
                    GetOrCreateMapVisual();
                    SubscribeListeners();    
                } else {
                    //if should not create visual, also do not vote on vision, this is for cases when a tile already has a gameobject
                    //and gained a trait that should not make the tile visible.
                    return;
                }
            }
        }
        if (trait.name != "Flammable") {
            //Whenever a generic tile object gains a trait that is NOT flammable, then set it as not Default
            //This checking is necessary because the tile's flammability is determined by it's ground type, and it being flammable
            //could be its default, so that case is handled by changes in ground type.
            _owner.SetIsDefault(false);
        }
        base.OnTileObjectGainedTrait(trait);
    }
    public override void OnTileObjectLostTrait(Trait trait) {
        base.OnTileObjectLostTrait(trait);
        if (TryDestroyMapVisual()) {
            UnsubscribeListeners();
        }
    }
    public override string ToString() {
        return $"Generic Obj at tile {gridTileLocation}";
    }
    public override void AddExistingJobTargetingThis(JobQueueItem job) {
        base.AddExistingJobTargetingThis(job);
        //Set this tile as no longer default since a job might need it when loading
        _owner.SetIsDefault(false);
    }
    public override void AdjustHP(int amount, ELEMENTAL_TYPE elementalDamageType, bool triggerDeath = false,
        object source = null, CombatManager.ElementalTraitProcessor elementalTraitProcessor = null, bool showHPBar = false) {
        if (currentHP == 0 && amount < 0) {
            return; //hp is already at minimum, do not allow any more negative adjustments
        }
        CombatManager.Instance.DamageModifierByElements(ref amount, elementalDamageType, this);
        currentHP += amount;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        _owner.SetIsDefault(false);
        if (amount < 0) {
            Character responsibleCharacter = null;
            if (source is Character character) {
                responsibleCharacter = character;
            }
            CombatManager.ElementalTraitProcessor etp = elementalTraitProcessor ?? 
                                                        CombatManager.Instance.DefaultElementalTraitProcessor;
            CombatManager.Instance.ApplyElementalDamage(amount, elementalDamageType, this, 
                responsibleCharacter, etp);
        }
        
        if (currentHP <= 0) {
            //floor has been destroyed
            gridTileLocation.DetermineNextGroundTypeAfterDestruction();
        } 
        if (amount < 0) {
            structureLocation.OnTileDamaged(gridTileLocation, amount);
        } else if (amount > 0) {
            structureLocation.OnTileRepaired(gridTileLocation, amount);
        }

        if (currentHP <= 0) {
            //reset floor hp at end of processing
            currentHP = maxHP;
        }
    }
    public override bool CanBeDamaged() {
        //only damage tiles that are part of non open space structures i.e structures with walls.
        return structureLocation.structureType.IsOpenSpace() == false;
    }
    public override bool CanBeSelected() {
        return false;
    }
    public override void OnReferencedInALog() {
        base.OnReferencedInALog();
        _owner.SetIsDefault(false);
    }
    public override void OnDoActionToObject(ActualGoapNode action) {
        base.OnDoActionToObject(action);
        if (action.goapType == INTERACTION_TYPE.BUILD_BLUEPRINT) {
            isCurrentlyBuilding = true;
        }
    }
    public override void OnCancelActionTowardsObject(ActualGoapNode action) {
        base.OnCancelActionTowardsObject(action);
        if (action.goapType == INTERACTION_TYPE.BUILD_BLUEPRINT) {
            isCurrentlyBuilding = false;
        }
    }
    #endregion

    public BaseMapObjectVisual GetOrCreateMapVisual() {
        if (ReferenceEquals(mapVisual, null)) {
            InitializeMapObject(this);
            PlaceMapObjectAt(gridTileLocation);
            OnPlaceTileObjectAtTile(gridTileLocation);
        }
        return mapVisual;
    }
    public bool TryDestroyMapVisual() {
        if (traitContainer.HasTangibleTrait() == false) {
            if (ReferenceEquals(mapVisual, null) == false) {
                DestroyMapVisualGameObject();
            }
            return true;
        }
        return false;
    }
    public void SetTileOwner(LocationGridTile owner) {
        _owner = owner;
    }

    public void ManualInitialize(LocationGridTile tile) {
        if (hasBeenInitialized) {
            return;
        }
        hasBeenInitialized = true;
        Initialize(TILE_OBJECT_TYPE.GENERIC_TILE_OBJECT, false);
        SetGridTileLocation(tile);
        AddAdvertisedAction(INTERACTION_TYPE.PLACE_FREEZING_TRAP);
        AddAdvertisedAction(INTERACTION_TYPE.GO_TO_TILE);
        AddAdvertisedAction(INTERACTION_TYPE.GO_TO_SPECIFIC_TILE);
        AddAdvertisedAction(INTERACTION_TYPE.FLEE_CRIME);
        AddAdvertisedAction(INTERACTION_TYPE.PLACE_BLUEPRINT);
        AddAdvertisedAction(INTERACTION_TYPE.BUILD_BLUEPRINT);
        AddAdvertisedAction(INTERACTION_TYPE.BUILD_VAMPIRE_CASTLE);
        AddAdvertisedAction(INTERACTION_TYPE.BUILD_NEW_VILLAGE);
    }
    public void ManualInitializeLoad(LocationGridTile tile, SaveDataTileObject saveDataTileObject) {
        if (hasBeenInitialized) {
            return;
        }
        hasBeenInitialized = true;
        Initialize(saveDataTileObject);
        SetGridTileLocation(tile);
    }

    #region Structure Blueprints
    public bool PlaceBlueprintOnTile(string prefabName) {
        GameObject structurePrefab = ObjectPoolManager.Instance.InstantiateObjectFromPool(prefabName, Vector3.zero, Quaternion.identity, gridTileLocation.parentMap.structureParent);
        LocationStructureObject structureObject = structurePrefab.GetComponent<LocationStructureObject>();
        if (structureObject.HasEnoughSpaceIfPlacedOn(gridTileLocation)) {
            structurePrefab.transform.position = gridTileLocation.centeredWorldLocation;
        
            structureObject.RefreshAllTilemaps();
            List<LocationGridTile> occupiedTiles = structureObject.GetTilesOccupiedByStructure(gridTileLocation.parentMap);
            for (int j = 0; j < occupiedTiles.Count; j++) {
                LocationGridTile tile = occupiedTiles[j];
                tile.SetHasBlueprint(true);
            }
            structureObject.SetVisualMode(LocationStructureObject.Structure_Visual_Mode.Blueprint, gridTileLocation.parentMap);
            structureObject.SetTilesInStructure(occupiedTiles.ToArray());
            blueprintOnTile = structureObject;
            gridTileLocation.SetIsDefault(false);
            ScheduleBlueprintExpiry();
            return true;
        }
        ObjectPoolManager.Instance.DestroyObject(structurePrefab); //destroy structure since it wasn't placed
        return false;
    }
    private void ScheduleBlueprintExpiry() {
        blueprintExpiryDate = GameManager.Instance.Today();
        blueprintExpiryDate = blueprintExpiryDate.AddDays(3);
        // blueprintExpiryDate = blueprintExpiryDate.AddTicks(GameManager.Instance.GetTicksBasedOnHour(1));
        _expiryKey = SchedulingManager.Instance.AddEntry(blueprintExpiryDate, ExpireBlueprint, this);
    }
    private void CancelBlueprintExpiry() {
        if (!string.IsNullOrEmpty(_expiryKey)) {
            SchedulingManager.Instance.RemoveSpecificEntry(_expiryKey);
            _expiryKey = string.Empty;
        }
    }
    private void ExpireBlueprint() {
        if (isCurrentlyBuilding) {
            //if currently building, schedule expiry again after 1 hour.
            blueprintExpiryDate = GameManager.Instance.Today();
            blueprintExpiryDate = blueprintExpiryDate.AddTicks(GameManager.ticksPerHour);
            _expiryKey = SchedulingManager.Instance.AddEntry(blueprintExpiryDate, ExpireBlueprint, this);
        } else {
            Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_JOB_TYPES_TARGETING_POI, this as IPointOfInterest, "", JOB_TYPE.BUILD_BLUEPRINT);
            ObjectPoolManager.Instance.DestroyObject(blueprintOnTile);
            blueprintOnTile = null;
            _expiryKey = string.Empty;    
        }
        
    }
    public LocationStructure BuildBlueprint(NPCSettlement npcSettlement) {
        HexTile hexTile = gridTileLocation.collectionOwner.partOfHextile.hexTileOwner;
        npcSettlement.AddTileToSettlement(hexTile);
        
        blueprintOnTile.SetVisualMode(LocationStructureObject.Structure_Visual_Mode.Built, gridTileLocation.parentMap);
        LocationStructure structure = LandmarkManager.Instance.CreateNewStructureAt(gridTileLocation.parentMap.region, blueprintOnTile.structureType, npcSettlement);
        blueprintOnTile.ClearOutUnimportantObjectsBeforePlacement();
    
        for (int j = 0; j < blueprintOnTile.tiles.Length; j++) {
            LocationGridTile tile = blueprintOnTile.tiles[j];
            tile.SetStructure(structure);
            tile.SetHasBlueprint(false);
        }
        Assert.IsTrue(structure is DemonicStructure || structure is ManMadeStructure);
        if (structure is DemonicStructure demonicStructure) {
            demonicStructure.SetStructureObject(blueprintOnTile);    
        } else if (structure is ManMadeStructure manMadeStructure) {
            manMadeStructure.SetStructureObject(blueprintOnTile);    
        }
        structure.SetOccupiedHexTile(hexTile.innerMapHexTile);
        blueprintOnTile.OnBuiltStructureObjectPlaced(gridTileLocation.parentMap, structure, out int createdWalls, out int totalWalls);
        structure.CreateRoomsBasedOnStructureObject(blueprintOnTile);
        structure.OnBuiltNewStructure();
        structure.OnBuiltNewStructureFromBlueprint();

        CancelBlueprintExpiry();
        blueprintOnTile = null;
        isCurrentlyBuilding = false;
        return structure;
        
    }
    #endregion

    #region Loading
    public override void LoadSecondWave(SaveDataTileObject data) {
        base.LoadSecondWave(data);
        SaveDataGenericTileObject saveDataGenericTileObject = data as SaveDataGenericTileObject;
        Debug.Assert(saveDataGenericTileObject != null, nameof(saveDataGenericTileObject) + " != null");
        if (!string.IsNullOrEmpty(saveDataGenericTileObject.blueprintOnTileName)) {
            LoadBlueprintOnTile(saveDataGenericTileObject.blueprintOnTileName);
            //schedule expiry
            blueprintExpiryDate = saveDataGenericTileObject.blueprintExpiryDate;
            _expiryKey = SchedulingManager.Instance.AddEntry(blueprintExpiryDate, ExpireBlueprint, this);
            isCurrentlyBuilding = saveDataGenericTileObject.isCurrentlyBuilding;
        }
    }
    private void LoadBlueprintOnTile(string prefabName) {
        GameObject structurePrefab = ObjectPoolManager.Instance.InstantiateObjectFromPool(prefabName, Vector3.zero, Quaternion.identity, gridTileLocation.parentMap.structureParent);
        LocationStructureObject structureObject = structurePrefab.GetComponent<LocationStructureObject>();
        structurePrefab.transform.position = gridTileLocation.centeredWorldLocation;
    
        structureObject.RefreshAllTilemaps();
        List<LocationGridTile> occupiedTiles = structureObject.GetTilesOccupiedByStructure(gridTileLocation.parentMap);
        for (int j = 0; j < occupiedTiles.Count; j++) {
            LocationGridTile tile = occupiedTiles[j];
            tile.SetHasBlueprint(true);
        }
        structureObject.SetVisualMode(LocationStructureObject.Structure_Visual_Mode.Blueprint, gridTileLocation.parentMap);
        structureObject.SetTilesInStructure(occupiedTiles.ToArray());
        blueprintOnTile = structureObject;
    }
    #endregion
}

#region Save Data
public class SaveDataGenericTileObject : SaveDataTileObject {

    public string blueprintOnTileName;
    public GameDate blueprintExpiryDate;
    public bool isCurrentlyBuilding;
    
    public override void Save(TileObject data) {
        base.Save(data);
        GenericTileObject genericTileObject = data as GenericTileObject;
        Debug.Assert(genericTileObject != null, nameof(genericTileObject) + " != null");
        if (genericTileObject.blueprintOnTile != null) {
            blueprintOnTileName = genericTileObject.blueprintOnTile.name.Replace("(Clone)", "");
            blueprintExpiryDate = genericTileObject.blueprintExpiryDate;
            isCurrentlyBuilding = genericTileObject.isCurrentlyBuilding;
        }
    }
    public override TileObject Load() {
        GenericTileObject genericTileObject = InnerMapManager.Instance.LoadTileObject<GenericTileObject>(this);
        return genericTileObject;
    }
}
#endregion