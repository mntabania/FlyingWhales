public class Copper : MetalPile {
    public override CONCRETE_RESOURCES specificProvidedResource => CONCRETE_RESOURCES.Copper;
    
    public Copper() : base(TILE_OBJECT_TYPE.COPPER) { }
    public Copper(SaveDataTileObject data) : base(data) { }
}
