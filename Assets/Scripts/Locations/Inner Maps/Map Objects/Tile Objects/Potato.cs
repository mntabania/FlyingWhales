﻿public class Potato : FoodPile {
    
    public override CONCRETE_RESOURCES specificProvidedResource => CONCRETE_RESOURCES.Potato;
    
    public Potato() : base(TILE_OBJECT_TYPE.POTATO) { }
    public Potato(SaveDataTileObject saveDataTileObject) : base(saveDataTileObject) { }
    
    #region Overrides
    public override string ToString() {
        return $"Potato {id.ToString()}";
    }
    #endregion
}
