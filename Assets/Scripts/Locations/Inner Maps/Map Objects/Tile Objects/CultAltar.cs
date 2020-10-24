public class CultAltar : TileObject {
    public CultAltar() {
        Initialize(TILE_OBJECT_TYPE.CULT_ALTAR);
        AddAdvertisedAction(INTERACTION_TYPE.SUMMON_BONE_GOLEM);
        traitContainer.AddTrait(this, "Indestructible");
        traitContainer.AddTrait(this, "Immovable");
    }
    public CultAltar(SaveDataTileObject data) : base(data) {
    }
    public override bool CanBeDamaged() {
        return false;
    }
}