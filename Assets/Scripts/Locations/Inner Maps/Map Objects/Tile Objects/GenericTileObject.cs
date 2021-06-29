using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Pathfinding.Util;
using UnityEngine;
using Traits;
using UnityEngine.Assertions;
using UnityEngine.Profiling;
using Debug = System.Diagnostics.Debug;

public class GenericTileObject : TileObject {
    private bool hasBeenInitialized { get; set; }
    /// <summary>
    /// The blueprint placed on this tile.
    /// NOTE: Only the center tile of the structure will have value.
    /// </summary>
    public LocationStructureObject blueprintOnTile { get; private set; }
    public string blueprintTemplateName { get; private set; } //Do not save this since this will be filled up automatically upon loading in SetStructureObject

    /// <summary>
    /// If blueprint that was placed here can expire,
    /// this is the date that it will expire.
    /// </summary>
    public GameDate blueprintExpiryDate { get; private set; }
    /// <summary>
    /// Is a villager currently building the blueprint on this tile.
    /// </summary>
    public bool isCurrentlyBuilding { get; private set; }
    /// <summary>
    /// If blueprint that was placed here is self building,
    /// this is the date that it will be finished
    /// </summary>
    public GameDate selfBuildingStructureDueDate { get; private set; }
    public StructureConnector structureConnector { get; private set; }
    
    public BaseSettlement selfBuildingStructureSettlement { get; private set; }
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
        if (trait.name != "Flammable") {
            //Whenever a generic tile object gains a trait that is NOT flammable, then set it as not Default
            //This checking is necessary because the tile's flammability is determined by it's ground type, and it being flammable
            //could be its default, so that case is handled by changes in ground type.
            _owner.SetIsDefault(false);
        }
        if (trait is Status status) {
            if(status.isTangible) {
                //if status is wet, and this tile is part of an Ocean, then do not create a map visual, since
                //characters do not react to Ocean Tiles
                bool willCreateVisual = !(status is Wet && gridTileLocation.structure is Ocean);
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
        object source = null, CombatManager.ElementalTraitProcessor elementalTraitProcessor = null, bool showHPBar = false, float piercingPower = 0f, bool isPlayerSource = false) {
        // if (currentHP == 0 && amount < 0) {
        //     return; //hp is already at minimum, do not allow any more negative adjustments
        // }
#if DEBUG_PROFILER
        Profiler.BeginSample($"GTO - Adjust HP - DamageModifierByElementsAndTraits");
#endif
        CombatManager.Instance.ModifyDamage(ref amount, elementalDamageType, piercingPower, this);
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif
        
        currentHP += amount;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        _owner.SetIsDefault(false);
        if (amount < 0) {
            Character responsibleCharacter = null;
            if (source is Character character) {
                responsibleCharacter = character;
            }
#if DEBUG_PROFILER
            Profiler.BeginSample($"GTO - Adjust HP - ApplyElementalDamage - {elementalDamageType.ToString()}");
#endif
            CombatManager.Instance.ApplyElementalDamage(amount, elementalDamageType, this, responsibleCharacter, elementalTraitProcessor, createHitEffect: false, setAsPlayerSource: isPlayerSource);
#if DEBUG_PROFILER
            Profiler.EndSample();
#endif
        }
        
        if (amount < 0) {
#if DEBUG_PROFILER
            Profiler.BeginSample($"GTO - Adjust HP - OnTileDamaged");
#endif
            structureLocation.OnTileDamaged(gridTileLocation, amount, isPlayerSource);
#if DEBUG_PROFILER
            Profiler.EndSample();
#endif
        } else if (amount > 0) {
#if DEBUG_PROFILER
            Profiler.BeginSample($"GTO - Adjust HP - OnTileRepaired");
#endif
            structureLocation.OnTileRepaired(gridTileLocation, amount);
#if DEBUG_PROFILER
            Profiler.EndSample();
#endif
        }

        if (currentHP <= 0) {
#if DEBUG_PROFILER
            Profiler.BeginSample($"GTO - Adjust HP - DetermineNextGroundTypeAfterDestruction");
#endif
            if (gridTileLocation.structure.structureType.IsPlayerStructure()) {
                //once tile in demonic structure is destroyed, revert tile to corrupted.
                gridTileLocation.SetGroundTilemapVisual(InnerMapManager.Instance.assetManager.corruptedTile);
            } else {
                //floor has been destroyed
                gridTileLocation.DetermineNextGroundTypeAfterDestruction();    
            }

#if DEBUG_PROFILER
            Profiler.EndSample();
#endif
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
        if (!traitContainer.HasTangibleTrait() && structureConnector == null) {
            if (!ReferenceEquals(mapVisual, null)) {
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
        // SetGridTileLocation(tile);
    }

    #region Structure Blueprints
    public bool PlaceExpiringBlueprintOnTile(string prefabName) {
        if (PlaceBlueprintOnTile(prefabName, out var blueprint)) {
            ScheduleBlueprintExpiry();
            return true;
        }
        return false;
    }
    private bool PlaceBlueprintOnTile(string p_prefabName, out LocationStructureObject o_placedBlueprint) {
        GameObject structurePrefab = ObjectPoolManager.Instance.InstantiateObjectFromPool(p_prefabName, Vector3.zero, Quaternion.identity, gridTileLocation.parentMap.structureParent);
        LocationStructureObject structureObject = structurePrefab.GetComponent<LocationStructureObject>();
        if (structureObject.HasEnoughSpaceIfPlacedOn(gridTileLocation)) {
            structurePrefab.transform.position = gridTileLocation.centeredWorldLocation;
        
            structureObject.RefreshAllTilemaps();
            List<LocationGridTile> occupiedTiles = structureObject.GetTilesOccupiedByStructure(gridTileLocation.parentMap);
            for (int j = 0; j < occupiedTiles.Count; j++) {
                LocationGridTile tile = occupiedTiles[j];
                tile.SetHasBlueprint(true);
            }
            var structureVisualMode = LocationStructureObject.Structure_Visual_Mode.Blueprint;
            if (structureObject.structureType.IsPlayerStructure()) {
                structureVisualMode = LocationStructureObject.Structure_Visual_Mode.Demonic_Structure_Blueprint;
            }
            structureObject.SetVisualMode(structureVisualMode, gridTileLocation.parentMap);
            structureObject.SetTilesInStructure(occupiedTiles.ToArray());
            blueprintOnTile = structureObject;
            blueprintTemplateName = structureObject.name;
            gridTileLocation.SetIsDefault(false);
            o_placedBlueprint = structureObject;
            return true;
        }
        o_placedBlueprint = null;
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
            blueprintTemplateName = string.Empty;
            _expiryKey = string.Empty;    
        }
    }
    public void BuildBlueprintOnTile(BaseSettlement p_settlement, LocationGridTile p_usedConnector) {
        BuildBlueprint(blueprintOnTile, p_settlement, p_usedConnector);
        CancelBlueprintExpiry();
        
        isCurrentlyBuilding = false;
    }
    public void InstantPlaceStructure(string p_structurePrefabName, BaseSettlement p_settlement) {
        GameObject structurePrefab = ObjectPoolManager.Instance.InstantiateObjectFromPool(p_structurePrefabName, Vector3.zero, Quaternion.identity, gridTileLocation.parentMap.structureParent);
        LocationStructureObject blueprint = structurePrefab.GetComponent<LocationStructureObject>();
        if (blueprint.HasEnoughSpaceIfPlacedOn(gridTileLocation)) {
            structurePrefab.transform.position = gridTileLocation.centeredWorldLocation;
            blueprint.RefreshAllTilemaps();
            List<LocationGridTile> occupiedTiles = blueprint.GetTilesOccupiedByStructure(gridTileLocation.parentMap);
            blueprint.SetTilesInStructure(occupiedTiles.ToArray());
            gridTileLocation.SetIsDefault(false);
            BuildBlueprint(blueprint, p_settlement, null);
        } else {
            throw new Exception($"Could not place {p_structurePrefabName} at {gridTileLocation}");
        }
    }
    private void BuildBlueprint(LocationStructureObject p_blueprint, BaseSettlement npcSettlement, LocationGridTile p_usedConnector) {
        Area hexTile = gridTileLocation.area;
        npcSettlement.AddAreaToSettlement(hexTile);
        p_blueprint.SetVisualMode(LocationStructureObject.Structure_Visual_Mode.Built, gridTileLocation.parentMap);
        LocationStructure structure = LandmarkManager.Instance.CreateNewStructureAt(gridTileLocation.parentMap.region, p_blueprint.structureType, npcSettlement);
        p_blueprint.ClearOutUnimportantObjectsBeforePlacement();
        
        for (int j = 0; j < p_blueprint.tiles.Length; j++) {
            LocationGridTile tile = p_blueprint.tiles[j];
            tile.SetStructure(structure);
            tile.SetHasBlueprint(false);
            if (structure is DemonicStructure) {
                tile.corruptionComponent.CorruptTile();
            } else {
                tile.corruptionComponent.UncorruptTile();
            }
        }

        if (structure is DemonicStructure dStructure) {
            //corrupt border tiles
            for (int j = 0; j < structure.tiles.Count; j++) {
                LocationGridTile tile = structure.tiles.ElementAt(j);
                for (int k = 0; k < tile.neighbourList.Count; k++) {
                    LocationGridTile neighbour = tile.neighbourList[k];
                    if (neighbour.structure is Cave || neighbour.structure is Ocean) { continue; } //do not corrupt cave tiles.
                    if (neighbour.structure != dStructure) {
                        neighbour.corruptionComponent.CorruptTileAndRandomlyGenerateDemonicObject();
                        if (structure is TortureChambers tortureChambers) {
                            tortureChambers.AddBorderTile(neighbour);
                        } else if (structure is Kennel kennel) {
                            kennel.AddBorderTile(neighbour);
                        }
                    }
                }
            }
        }
        
        Assert.IsTrue(structure is DemonicStructure || structure is ManMadeStructure || structure is AnimalDen);
        if (structure is DemonicStructure demonicStructure) {
            demonicStructure.SetStructureObject(p_blueprint);    
        } else if (structure is ManMadeStructure manMadeStructure) {
            manMadeStructure.SetStructureObject(p_blueprint);    
        } else if (structure is AnimalDen animalDen) {
            animalDen.SetStructureObject(p_blueprint);    
        }
        
        structure.SetOccupiedArea(hexTile);
        
        p_blueprint.OnBuiltStructureObjectPlaced(gridTileLocation.parentMap, structure, out int createdWalls, out int totalWalls, structure.preplacedObjectsToIgnoreWhenBuilding);
        structure.CreateRoomsBasedOnStructureObject(p_blueprint);
        structure.OnBuiltNewStructure();
        structure.OnBuiltNewStructureFromBlueprint();

        if (p_usedConnector != null) {
            if (structure is ManMadeStructure mmStructure) {
                mmStructure.OnUseStructureConnector(p_usedConnector);    
            }    
        }
        blueprintOnTile = null;
        blueprintTemplateName = string.Empty;
    }
    private BuildStructureParticleEffect _buildStructureParticles;
    public void PlaceSelfBuildingStructure(string p_structurePrefabName, BaseSettlement p_settlement, int p_buildingTimeInTicks) {
        Assert.IsTrue(p_buildingTimeInTicks > 0);
        if (PlaceBlueprintOnTile(p_structurePrefabName, out var blueprint)) {
            AudioManager.Instance.CreatePlaceDemonicStructureSound(gridTileLocation);
            BaseParticleEffect placeEffect = GameManager.Instance.CreateParticleEffectAt(gridTileLocation, PARTICLE_EFFECT.Place_Demonic_Structure).GetComponent<BaseParticleEffect>();
            placeEffect.SetSize(blueprint.size);


            selfBuildingStructureSettlement = p_settlement;
            GameDate completionDate = GameManager.Instance.Today();
            completionDate.AddTicks(p_buildingTimeInTicks);
            CreateBuildParticlesAndScheduleBuildingCompletion(blueprint, p_settlement, completionDate);
        } else {
            throw new Exception($"Could not place self building structure {p_structurePrefabName} on {gridTileLocation}!");
        }
    }
    private void CreateBuildParticlesAndScheduleBuildingCompletion(LocationStructureObject p_blueprint, BaseSettlement p_settlement, GameDate p_completionDate) {
        _buildStructureParticles = GameManager.Instance.CreateParticleEffectAt(gridTileLocation, PARTICLE_EFFECT.Build_Demonic_Structure).GetComponent<BuildStructureParticleEffect>();
        _buildStructureParticles.SetSize(p_blueprint.size);
        selfBuildingStructureDueDate = p_completionDate;
        SchedulingManager.Instance.AddEntry(p_completionDate, () => DoneSelfBuildingStructure(p_settlement), this);
    }
    private void DoneSelfBuildingStructure(BaseSettlement p_settlement) {
        if (_buildStructureParticles != null) {
            ObjectPoolManager.Instance.DestroyObject(_buildStructureParticles);
        }
        BuildBlueprint(blueprintOnTile, p_settlement, null);
        selfBuildingStructureSettlement = null;
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
            if (saveDataGenericTileObject.blueprintExpiryDate.hasValue) {
                blueprintExpiryDate = saveDataGenericTileObject.blueprintExpiryDate;
                _expiryKey = SchedulingManager.Instance.AddEntry(blueprintExpiryDate, ExpireBlueprint, this);
                isCurrentlyBuilding = saveDataGenericTileObject.isCurrentlyBuilding;    
            } else if (saveDataGenericTileObject.blueprintAutoBuildDate.hasValue) {
                if (!string.IsNullOrEmpty(saveDataGenericTileObject.selfBuildingStructureSettlement)) {
                    selfBuildingStructureSettlement = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentID(saveDataGenericTileObject.selfBuildingStructureSettlement);
                    CreateBuildParticlesAndScheduleBuildingCompletion(blueprintOnTile, selfBuildingStructureSettlement, saveDataGenericTileObject.blueprintAutoBuildDate);
                }
            }
            
        }
        if (saveDataGenericTileObject.hasStructureConnector) {
            LoadMineShackSpotStructureConnector();
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
        var structureVisualMode = LocationStructureObject.Structure_Visual_Mode.Blueprint;
        if (structureObject.structureType.IsPlayerStructure()) {
            structureVisualMode = LocationStructureObject.Structure_Visual_Mode.Demonic_Structure_Blueprint;
        }
        structureObject.SetVisualMode(structureVisualMode, gridTileLocation.parentMap);
        structureObject.SetTilesInStructure(occupiedTiles.ToArray());
        blueprintOnTile = structureObject;
        blueprintTemplateName = structureObject.name;
    }
    #endregion

    #region Structure Connectors
    public void CreateMineShackSpotStructureConnector() {
        BaseMapObjectVisual objectVisual = GetOrCreateMapVisual();
        StructureConnector connector = objectVisual.gameObject.AddComponent<StructureConnector>();
        connector.OnPlaceConnector(gridTileLocation.parentMap);
        structureConnector = connector;
        //if (gridTileLocation.area.settlementOnArea != null) {
        //    gridTileLocation.area.settlementOnArea.SettlementResources?.AddToListBasedOnRequirement(SettlementResources.StructureRequirement.MINE_SHACK_SPOT, this);
        //}
        if (gridTileLocation.area != null) {
            gridTileLocation.area.tileObjectComponent.AddMineShackSpot(gridTileLocation);
        }
        //Messenger.AddListener<Area, BaseSettlement>(SettlementSignals.SETTLEMENT_ADDED_AREA, OnSettlementAddedArea);
        //Messenger.AddListener<Area, BaseSettlement>(SettlementSignals.SETTLEMENT_REMOVED_AREA, OnSettlementRemovedArea);
        gridTileLocation.SetIsDefault(false);
    }
    private void LoadMineShackSpotStructureConnector() {
        BaseMapObjectVisual objectVisual = GetOrCreateMapVisual();
        StructureConnector connector = objectVisual.gameObject.AddComponent<StructureConnector>();
        structureConnector = connector;
        //if (gridTileLocation.area.settlementOnArea != null) {
        //    gridTileLocation.area.settlementOnArea.SettlementResources?.AddToListBasedOnRequirement(SettlementResources.StructureRequirement.MINE_SHACK_SPOT, this);
        //}
        if (gridTileLocation.area != null) {
            gridTileLocation.area.tileObjectComponent.AddMineShackSpot(gridTileLocation);
        }
        //Messenger.AddListener<Area, BaseSettlement>(SettlementSignals.SETTLEMENT_ADDED_AREA, OnSettlementAddedArea);
        //Messenger.AddListener<Area, BaseSettlement>(SettlementSignals.SETTLEMENT_REMOVED_AREA, OnSettlementRemovedArea);
        if (structureConnector != null && gridTileLocation != null) {
            structureConnector.LoadConnectorForTileObjects(gridTileLocation.parentMap);
        }
    }

    //Note: No longer needed to check if area is added or removed from settlement to add or remove mine shack spot since it is now connected to the area
    //private void OnSettlementAddedArea(Area p_area, BaseSettlement p_settlement) {
    //    if (gridTileLocation.area == p_area && p_settlement is NPCSettlement npcSettlement) {
    //        npcSettlement.SettlementResources?.AddToListBasedOnRequirement(SettlementResources.StructureRequirement.MINE_SHACK_SPOT, this);
    //    }
    //}
    //private void OnSettlementRemovedArea(Area p_area, BaseSettlement p_settlement) {
    //    if (gridTileLocation.area == p_area && p_settlement is NPCSettlement npcSettlement) {
    //        npcSettlement.SettlementResources?.mineShackSpots.Remove(gridTileLocation);
    //    }
    //}
    #endregion
}

#region Save Data
public class SaveDataGenericTileObject : SaveDataTileObject {

    public string blueprintOnTileName;
    public GameDate blueprintExpiryDate;
    public GameDate blueprintAutoBuildDate;
    public bool isCurrentlyBuilding;
    public string selfBuildingStructureSettlement;
    public bool hasStructureConnector;
    public override void Save(TileObject data) {
        base.Save(data);
        GenericTileObject genericTileObject = data as GenericTileObject;
        Debug.Assert(genericTileObject != null, nameof(genericTileObject) + " != null");
        if (!string.IsNullOrEmpty(genericTileObject.blueprintTemplateName)) {
            blueprintOnTileName = genericTileObject.blueprintTemplateName.Replace("(Clone)", "");
            blueprintExpiryDate = genericTileObject.blueprintExpiryDate;
            isCurrentlyBuilding = genericTileObject.isCurrentlyBuilding;
            blueprintAutoBuildDate = genericTileObject.selfBuildingStructureDueDate;
            if (genericTileObject.selfBuildingStructureSettlement != null) {
                selfBuildingStructureSettlement = genericTileObject.selfBuildingStructureSettlement.persistentID;
            }
        }
        hasStructureConnector = genericTileObject.structureConnector != null;
    }
    public override TileObject Load() {
        GenericTileObject genericTileObject = InnerMapManager.Instance.LoadTileObject<GenericTileObject>(this);
        return genericTileObject;
    }
}
#endregion