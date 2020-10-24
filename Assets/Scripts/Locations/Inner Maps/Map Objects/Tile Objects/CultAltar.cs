public class CultAltar : TileObject {
    public CultAltar() {
        Initialize(TILE_OBJECT_TYPE.CULT_ALTAR);
        traitContainer.AddTrait(this, "Indestructible");
        traitContainer.AddTrait(this, "Immovable");
    }
    public CultAltar(SaveDataTileObject data) : base(data) {
        
    }
    public override bool CanBeDamaged() {
        return false;
    }
}