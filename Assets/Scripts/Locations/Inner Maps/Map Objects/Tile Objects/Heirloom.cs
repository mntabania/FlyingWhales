using System.Collections.Generic;
using Inner_Maps.Location_Structures;

public class Heirloom : TileObject{

    public LocationStructure structureSpot { get; private set; }

    public Heirloom() {
        Initialize(TILE_OBJECT_TYPE.HEIRLOOM);
        traitContainer.AddTrait(this, "Indestructible");
        traitContainer.AddTrait(this, "Fireproof");
    }
    public Heirloom(SaveDataTileObject data) {
        Initialize(data);
        traitContainer.AddTrait(this, "Indestructible");
        traitContainer.AddTrait(this, "Fireproof");
    }

    public void SetStructureSpot(LocationStructure structure) {
        structureSpot = structure;
    }

    public bool IsInStructureSpot() {
        return gridTileLocation != null && gridTileLocation.structure == structureSpot;
    }
}
