using Inner_Maps.Location_Structures;
using Inner_Maps.Map_Objects.Map_Object_Visuals;
using Locations.Settlements;

public class FishingSpot : TileObject {
    public override StructureConnector structureConnector => _fishingSpotGameObject.structureConnector;
    private FishingSpotGameObject _fishingSpotGameObject;
    
    public FishingShack connectedStructure { get; private set; } //TODO:
    
    public FishingSpot() {
        Initialize(TILE_OBJECT_TYPE.FISHING_SPOT);
        traitContainer.RemoveTrait(this, "Flammable");
        traitContainer.AddTrait(this, "Immovable");
        traitContainer.AddTrait(this, "Frozen Immune");
        for (int i = 0; i < 10; i++) {
            traitContainer.AddTrait(this, "Wet", overrideDuration: 0);
        }
        BaseSettlement.onSettlementBuilt += UpdateSettlementResourcesParent;
    }
    public FishingSpot(SaveDataTileObject data) { }

    #region Overrides
    protected override void CreateMapObjectVisual() {
        base.CreateMapObjectVisual();
        _fishingSpotGameObject = mapVisual as FishingSpotGameObject;
    }
    public override void DestroyMapVisualGameObject() {
        base.DestroyMapVisualGameObject();
        _fishingSpotGameObject = null;
    }
    public override void UpdateSettlementResourcesParent() {
        if (gridTileLocation.collectionOwner.isPartOfParentRegionMap) {
            if (gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.settlementOnTile != null) {
                gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.settlementOnTile.SettlementResources.AddToListbaseOnRequirement(SettlementResources.StructureRequirement.FISHING_SPOT, this);
            }
            gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.AllNeighbours.ForEach((eachNeighboringHexTile) => {
                if (eachNeighboringHexTile.settlementOnTile != null) {
                    eachNeighboringHexTile.settlementOnTile.SettlementResources.AddToListbaseOnRequirement(SettlementResources.StructureRequirement.FISHING_SPOT, this);
                    parentSettlement = eachNeighboringHexTile.settlementOnTile;
                }
            });
        }
    }
    public override void RemoveFromSettlementResourcesParent() {
        parentSettlement?.SettlementResources.fishingSpots.Remove(this);
    }
    public override void OnPlacePOI() {
        base.OnPlacePOI();
        traitContainer.AddTrait(this, "Indestructible");
        name = "a Lake";
        AddAdvertisedAction(INTERACTION_TYPE.FISH);
        Messenger.AddListener(Signals.HOUR_STARTED, HourStarted);
    }
    public override void OnDestroyPOI() {
        base.OnDestroyPOI();
        Messenger.RemoveListener(Signals.HOUR_STARTED, HourStarted);
    }
    public override bool CanBeAffectedByElementalStatus(string traitName) {
        if (traitName == "Wet") {
            return true; //allow water well to be wet.
        }
        return structureLocation.structureType != STRUCTURE_TYPE.POND && structureLocation.structureType != STRUCTURE_TYPE.OCEAN;
    }
    public override bool CanBeDamaged() {
        return structureLocation.structureType != STRUCTURE_TYPE.POND && structureLocation.structureType != STRUCTURE_TYPE.OCEAN;
    }
    public override bool CanBeSelected() {
        return structureLocation != null && structureLocation.structureType != STRUCTURE_TYPE.POND && structureLocation.structureType != STRUCTURE_TYPE.OCEAN;
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
}
