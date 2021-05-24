namespace Inner_Maps.Location_Structures {
    public class KennelStructureObject : LocationStructureObject {
        protected override void PreplacedObjectProcessing(StructureTemplateObjectData preplacedObj, LocationGridTile tile, LocationStructure structure, TileObject newTileObject) {
            base.PreplacedObjectProcessing(preplacedObj, tile, structure, newTileObject);
            if (newTileObject.tileObjectType == TILE_OBJECT_TYPE.KENNEL_TILE_OBJECT || newTileObject.tileObjectType == TILE_OBJECT_TYPE.DEMONIC_STRUCTURE_BLOCKER_TILE_OBJECT) {
                structure.AddObjectAsDamageContributor(newTileObject);    
            }
        }
    } 
}