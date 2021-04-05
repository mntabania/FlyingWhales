public static class StructureSignals {
    public static string WALL_DAMAGED = "OnWallDamaged";
    public static string WALL_REPAIRED = "OnWallRepaired";
    /// <summary>
    /// parameters:
    /// LocationStructure placedStructure
    /// </summary>
    public static string STRUCTURE_OBJECT_PLACED = "OnStructureObjectPlaced";
    public static string STRUCTURE_OBJECT_REMOVED = "OnStructureObjectRemoved";
    public static string ADDED_STRUCTURE_RESIDENT = "OnAddedStructureResident";
    public static string REMOVED_STRUCTURE_RESIDENT = "OnRemoveStructureResident";
    /// <summary>
    /// Parameters (LocationGridTile)
    /// </summary>
    public static string STRUCTURE_CONNECTOR_PLACED = "OnStructureConnectorPlaced";
    /// <summary>
    /// Parameters (LocationGridTile)
    /// </summary>
    public static string STRUCTURE_CONNECTOR_REMOVED = "OnStructureConnectorRemoved";
    /// <summary>
    /// Parameters: LocationStructure
    /// </summary>
    public static string STRUCTURE_DESTROYED = "OnStructureDestroyed";
    /// <summary>
    /// Parameters (Table table)
    /// </summary>
    public static string FOOD_IN_DWELLING_CHANGED = "OnFoodInDwellingChanged";
    /// <summary>
    /// Parameters (DemonicStructure)
    /// </summary>
    public static string DEMONIC_STRUCTURE_REPAIRED = "OnDemonicStructureRepaired";
    public static string UPDATE_EYE_WARDS = "OnUpdateEyeWards";
}