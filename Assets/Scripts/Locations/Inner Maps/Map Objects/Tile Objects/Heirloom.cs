using System.Collections.Generic;
using Inner_Maps.Location_Structures;

public class Heirloom : TileObject{

    public LocationStructure structureSpot { get; private set; }

    public Heirloom() {
        Initialize(TILE_OBJECT_TYPE.HEIRLOOM);
        AddAdvertisedAction(INTERACTION_TYPE.HUNT_HEIRLOOM);
        AddAdvertisedAction(INTERACTION_TYPE.DROP_ITEM);
        AddAdvertisedAction(INTERACTION_TYPE.PICK_UP);
        traitContainer.AddTrait(this, "Indestructible");
        traitContainer.AddTrait(this, "Fire Resistant");
    }
    public Heirloom(SaveDataTileObject data) : base(data) { }

    public void SetStructureSpot(LocationStructure structure) {
        structureSpot = structure;
    }

    public bool IsInStructureSpot() {
        return gridTileLocation != null && gridTileLocation.structure == structureSpot;
    }
}
