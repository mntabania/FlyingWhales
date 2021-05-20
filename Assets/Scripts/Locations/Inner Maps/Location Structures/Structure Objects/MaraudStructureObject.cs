﻿namespace Inner_Maps.Location_Structures {
    public class MaraudStructureObject : LocationStructureObject {
        protected override void PreplacedObjectProcessing(StructureTemplateObjectData preplacedObj,
            LocationGridTile tile, LocationStructure structure, TileObject newTileObject) {
            base.PreplacedObjectProcessing(preplacedObj, tile, structure, newTileObject);
            if (newTileObject.tileObjectType == TILE_OBJECT_TYPE.MARAUD_TILE_OBJECT) {
                structure.AddObjectAsDamageContributor(newTileObject);
            }
        }
    }
}