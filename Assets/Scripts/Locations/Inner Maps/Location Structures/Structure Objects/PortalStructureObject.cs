namespace Inner_Maps.Location_Structures {
    public class PortalStructureObject : LocationStructureObject {
        protected override void PreplacedObjectProcessing(StructureTemplateObjectData preplacedObj,
            LocationGridTile tile, LocationStructure structure, TileObject newTileObject) {
            base.PreplacedObjectProcessing(preplacedObj, tile, structure, newTileObject);
            if (newTileObject.tileObjectType == TILE_OBJECT_TYPE.PORTAL_TILE_OBJECT) {
                structure.AddObjectAsDamageContributor(newTileObject);    
            }
        }
    }
}