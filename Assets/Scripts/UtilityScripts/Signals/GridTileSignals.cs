public static class GridTileSignals {
    public static string OBJECT_PLACED_ON_TILE = "OnObjectPlacedOnTile"; //Parameters (LocationGridTile, IPointOfInterest)
    public static string TILE_OBJECT_REMOVED = "OnTileObjectDestroyed"; //Parameters (TileObject, Character removedBy)
    public static string TILE_OBJECT_PLACED = "OnTileObjectPlaced"; //Parameters (TileObject, LocationGridTile)
    public static string ACTION_PERFORMED_ON_TILE_TRAITABLES = "OnActionPerformedOnTileTraitables";
}