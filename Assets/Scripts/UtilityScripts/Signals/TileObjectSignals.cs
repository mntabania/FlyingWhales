public static class TileObjectSignals {
    public static string TILE_OBJECT_TRAIT_ADDED = "OnTileObjectTraitAdded";
    public static string TILE_OBJECT_TRAIT_REMOVED = "OnTileObjectTraitRemoved"; //Parameters (Character character, Trait)
    public static string TILE_OBJECT_TRAIT_STACKED = "OnTileObjectTraitStacked";
    public static string TILE_OBJECT_TRAIT_UNSTACKED = "OnTileObjectTraitUnstacked";
    public static string CHECK_UNBUILT_OBJECT_VALIDITY = "CheckUnbuiltObjectValidity";
    public static string ADD_TILE_OBJECT_USER = "OnAddTileObjectUser";
    public static string REMOVE_TILE_OBJECT_USER = "OnAddTileObjectUser";
    /// <summary>
    /// Parameters TileObject
    /// </summary>
    public static string TILE_OBJECT_ACTIVATED = "OnTileObjectActivated";
    /// <summary>
    /// Parameters (ResourcePile resource)
    /// </summary>
    public static string RESOURCE_IN_PILE_CHANGED = "OnResourceInPileChanged";
    /// <summary>
    /// Parameters (TileObject damagedObj, int damageAmount)
    /// </summary>
    public static string TILE_OBJECT_DAMAGED = "OnTileObjectDamaged";
    /// <summary>
    /// Parameters (TileObject damagedObj, int damageAmount, Character p_responsibleCharacter)
    /// </summary>
    public static string TILE_OBJECT_DAMAGED_BY = "OnTileObjectDamagedBy";
    /// <summary>
    /// Parameters (TileObject repairedObj, int repairAmount)
    /// </summary>
    public static string TILE_OBJECT_REPAIRED = "OnTileObjectRepaired";
    /// <summary>
    /// Parameters (TileObject repairedObj)
    /// </summary>
    public static string TILE_OBJECT_FULLY_REPAIRED = "OnTileObjectFullyRepaired";
    public static string DESTROY_TILE_OBJECT = "OnDestroyTileObject";
    /// <summary>
    /// Parameters (MovingTileObject)
    /// </summary>
    public static string MOVING_TILE_OBJECT_EXPIRED = "OnMovingTileObjectExpired";
}