using System.Collections.Generic;

public class PlinthBook : TileObject{
    public PlinthBook() {
        Initialize(TILE_OBJECT_TYPE.PLINTH_BOOK);
    }
    public PlinthBook(SaveDataTileObject data) {
        Initialize(data);
    }
}
