using System.Collections.Generic;

public class FoodBaskets : TileObject{
    public FoodBaskets() {
        Initialize(TILE_OBJECT_TYPE.FOOD_BASKETS);
        //advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
    }
    public FoodBaskets(SaveDataTileObject data) {
        //advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
        Initialize(data);
    }
}
