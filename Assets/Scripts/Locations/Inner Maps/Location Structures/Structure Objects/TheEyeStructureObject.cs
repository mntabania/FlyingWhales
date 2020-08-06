namespace Inner_Maps.Location_Structures {
    public class TheEyeStructureObject : LocationStructureObject {
        protected override void PreplacedObjectProcessing(StructureTemplateObjectData preplacedObj,
            LocationGridTile tile, LocationStructure structure, TileObject newTileObject) {
            base.PreplacedObjectProcessing(preplacedObj, tile, structure, newTileObject);
            if (newTileObject.tileObjectType == TILE_OBJECT_TYPE.EYEBALL) {
                structure.AddObjectAsDamageContributor(newTileObject);    
            }
        }
    }
}