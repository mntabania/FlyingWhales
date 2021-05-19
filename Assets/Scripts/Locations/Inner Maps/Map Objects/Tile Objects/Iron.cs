public class Iron : MetalPile {
    public override CONCRETE_RESOURCES specificProvidedResource => CONCRETE_RESOURCES.Iron;
    
    public Iron() : base(TILE_OBJECT_TYPE.IRON){ }
    public Iron(SaveDataTileObject data) : base(data) { }
}
