public class Orichalcum : MetalPile {
    public override CONCRETE_RESOURCES specificProvidedResource => CONCRETE_RESOURCES.Orichalcum;
    
    public Orichalcum() : base(TILE_OBJECT_TYPE.ORICHALCUM) { }
    public Orichalcum(SaveDataTileObject data) : base(data) { }
}
