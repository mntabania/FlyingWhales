public class Mithril : MetalPile {

    public override CONCRETE_RESOURCES specificProvidedResource => CONCRETE_RESOURCES.Mithril;
    
    public Mithril() : base(TILE_OBJECT_TYPE.MITHRIL){ }

    public Mithril(SaveDataTileObject data) : base(data) { }
    
}
