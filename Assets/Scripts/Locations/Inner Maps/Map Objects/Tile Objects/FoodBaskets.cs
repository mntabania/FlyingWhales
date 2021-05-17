using System.Collections.Generic;

public class FoodBaskets : TileObject{
    public FoodBaskets() {
        Initialize(TILE_OBJECT_TYPE.FOOD_BASKETS);
    }
    public FoodBaskets(SaveDataTileObject data) : base(data) {
        
    }
}
