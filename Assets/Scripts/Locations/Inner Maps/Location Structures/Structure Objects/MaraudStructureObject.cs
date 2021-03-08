namespace Inner_Maps.Location_Structures {
    public class MaraudStructureObject : LocationStructureObject {
        protected override void PreplacedObjectProcessing(StructureTemplateObjectData preplacedObj,
            LocationGridTile tile, LocationStructure structure, TileObject newTileObject) {
            base.PreplacedObjectProcessing(preplacedObj, tile, structure, newTileObject);
            if (newTileObject.tileObjectType == TILE_OBJECT_TYPE.BLOOD_POOL ||
                newTileObject.tileObjectType == TILE_OBJECT_TYPE.RACK_WEAPONS ||
                newTileObject.tileObjectType == TILE_OBJECT_TYPE.CULT_CIRCLE ||
                newTileObject.tileObjectType == TILE_OBJECT_TYPE.CORRUPTED_SPIKE ||
                newTileObject.tileObjectType == TILE_OBJECT_TYPE.RACK_FARMING_TOOLS) {
                structure.AddObjectAsDamageContributor(newTileObject);
            }
        }
    }
}