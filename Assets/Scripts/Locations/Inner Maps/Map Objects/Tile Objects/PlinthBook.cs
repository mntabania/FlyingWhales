using System.Collections.Generic;

public class PlinthBook : TileObject{
    public PlinthBook() {
        Initialize(TILE_OBJECT_TYPE.PLINTH_BOOK);
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
    }
    public PlinthBook(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
        Initialize(data);
    }    
}
