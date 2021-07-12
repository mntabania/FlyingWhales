using System;
using Inner_Maps.Location_Structures;
using Inner_Maps.Map_Objects.Map_Object_Visuals;
using Locations.Settlements;
using UtilityScripts;

public class FishingSpot : TileObject {
    public StructureConnector structureConnector {
        get {
            if (_fishingSpotGameObject != null) {
                return _fishingSpotGameObject.structureConnector;
            }
            return null;
        }
    }
    private FishingSpotGameObject _fishingSpotGameObject;
    
    public Fishery connectedFishingShack { get; private set; }
    //public BaseSettlement parentSettlement { get; private set; }
    public override Type serializedData => typeof(SaveDataFishingSpot);
    public FishingSpot() {
        Initialize(TILE_OBJECT_TYPE.FISHING_SPOT);
        traitContainer.RemoveTrait(this, "Flammable");
        traitContainer.AddTrait(this, "Immovable");
        traitContainer.AddTrait(this, "Frozen Immune");
        for (int i = 0; i < 10; i++) {
            traitContainer.AddTrait(this, "Wet", overrideDuration: 0);
        }
        //BaseSettlement.onSettlementBuilt += UpdateSettlementResourcesParent;
    }
    public FishingSpot(SaveDataTileObject data) : base(data) { }

    #region Loading
    public override void LoadSecondWave(SaveDataTileObject data) {
        base.LoadSecondWave(data);
        SaveDataFishingSpot saveDataFishingSpot = data as SaveDataFishingSpot;
        if (!string.IsNullOrEmpty(saveDataFishingSpot.connectedFishingShackID)) {
            connectedFishingShack = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(saveDataFishingSpot.connectedFishingShackID) as Fishery;
        }
    }
    #endregion
    
    #region Overrides
    protected override void CreateMapObjectVisual() {
        base.CreateMapObjectVisual();
        _fishingSpotGameObject = mapVisual as FishingSpotGameObject;
    }
    public override void DestroyMapVisualGameObject() {
        base.DestroyMapVisualGameObject();
        _fishingSpotGameObject = null;
    }
    //protected override void UpdateSettlementResourcesParent() {
    //    if (gridTileLocation.area.settlementOnArea != null) {
    //        gridTileLocation.area.settlementOnArea.SettlementResources?.AddToListBasedOnRequirement(SettlementResources.StructureRequirement.FISHING_SPOT, this);
    //    }
    //    gridTileLocation.area.neighbourComponent.neighbours.ForEach((eachNeighbor) => {
    //        if (eachNeighbor.settlementOnArea != null) {
    //            //eachNeighbor.settlementOnArea.SettlementResources?.AddToListBasedOnRequirement(SettlementResources.StructureRequirement.FISHING_SPOT, this);
    //            parentSettlement = eachNeighbor.settlementOnArea;
    //        }
    //    });
    //}
    //protected override void RemoveFromSettlementResourcesParent() {
    //    if (parentSettlement != null && parentSettlement.SettlementResources != null) {
    //        if (parentSettlement.SettlementResources.fishingSpots.Remove(this)) {
    //            parentSettlement = null;
    //        }    
    //    }
        
    //}
    public override void OnPlacePOI() {
        base.OnPlacePOI();
        traitContainer.AddTrait(this, "Indestructible");
        name = "a Lake";
        AddAdvertisedAction(INTERACTION_TYPE.FIND_FISH);
        Messenger.AddListener(Signals.HOUR_STARTED, HourStarted);
        if (structureConnector != null && gridTileLocation != null) {
            structureConnector.OnPlaceConnector(gridTileLocation.parentMap);    
        }
    }
    public override void OnLoadPlacePOI() {
        DefaultProcessOnPlacePOI();
        Messenger.AddListener(Signals.HOUR_STARTED, HourStarted);
        if (structureConnector != null && gridTileLocation != null) {
            structureConnector.LoadConnectorForTileObjects(gridTileLocation.parentMap);    
        }
    }
    public override void OnDestroyPOI() {
        base.OnDestroyPOI();
        Messenger.RemoveListener(Signals.HOUR_STARTED, HourStarted);
        //BaseSettlement.onSettlementBuilt -= UpdateSettlementResourcesParent;
    }
    public override bool CanBeAffectedByElementalStatus(string traitName) {
        if (traitName == "Wet") {
            return true; //allow water well to be wet.
        }
        return structureLocation.structureType != STRUCTURE_TYPE.OCEAN;
    }
    public override bool CanBeDamaged() {
        return structureLocation.structureType != STRUCTURE_TYPE.OCEAN;
    }
    public override bool CanBeSelected() {
        return structureLocation != null && structureLocation.structureType != STRUCTURE_TYPE.OCEAN;
    }
    public override string ToString() {
        return $"Fishing Spot {id.ToString()}";
    }
    #endregion

    #region Listeners
    private void HourStarted() {
        if (traitContainer.stacks.ContainsKey("Wet") && traitContainer.stacks["Wet"] < 10) {
            traitContainer.AddTrait(this, "Wet", overrideDuration: 0);    
        }
        
    }
    #endregion

    #region Structure
    public void SetConnectedFishingShack(Fishery p_fishingShack) {
        connectedFishingShack = p_fishingShack;
        if (connectedFishingShack != null) {
            Messenger.AddListener<LocationStructure>(StructureSignals.STRUCTURE_DESTROYED, OnStructureDestroyed);
        } else {
            Messenger.RemoveListener<LocationStructure>(StructureSignals.STRUCTURE_DESTROYED, OnStructureDestroyed);
        }
    }
    private void OnStructureDestroyed(LocationStructure p_structure) {
        if (p_structure == connectedFishingShack) {
            SetConnectedFishingShack(null);
        }
    }
    #endregion

    #region Reactions
    public override void GeneralReactionToTileObject(Character actor, ref string debugLog) {
        base.GeneralReactionToTileObject(actor, ref debugLog);
        if (gridTileLocation != null) {
            if (actor.race != RACE.TRITON) {
                if (GameUtilities.RollChance(0.05f)) {
                    if (actor.canBeTargetedByLandActions) {
                        if (!actor.traitContainer.HasTrait("Sturdy", "Hibernating") && !actor.HasJobTargetingThis(JOB_TYPE.TRITON_KIDNAP)) {
                            Summon summon = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Triton, FactionManager.Instance.neutralFaction, homeRegion: currentRegion, bypassIdeologyChecking: true);
                            summon.SetIsVolatile(true);
                            CharacterManager.Instance.PlaceSummonInitially(summon, gridTileLocation);
                            (summon as Triton).TriggerTritonKidnap(actor);
                        }
                    }
                }
            }
        }
    }
    #endregion
}

#region Save Data
public class SaveDataFishingSpot : SaveDataTileObject {
    public string connectedFishingShackID;
    public override void Save(TileObject data) {
        base.Save(data);
        FishingSpot fishingSpot = data as FishingSpot;
        if (fishingSpot.connectedFishingShack != null) {
            connectedFishingShackID = fishingSpot.connectedFishingShack.persistentID;
        }
    }
}
#endregion