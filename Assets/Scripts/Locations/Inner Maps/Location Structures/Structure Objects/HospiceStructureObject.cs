using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Assertions;
namespace Inner_Maps.Location_Structures {
    public class HospiceStructureObject : LocationStructureObject {

        protected override void PreplacedObjectProcessing(StructureTemplateObjectData preplacedObj,
            LocationGridTile tile, LocationStructure structure, TileObject newTileObject) {
            tile.structure.AddPOI(newTileObject, tile);
            newTileObject.mapVisual.SetVisual(preplacedObj.spriteRenderer.sprite);
            newTileObject.mapVisual.SetRotation(preplacedObj.transform.localEulerAngles.z);
            newTileObject.RevalidateTileObjectSlots();
            Hospice hospice = structure as Hospice;
            if (structureType == STRUCTURE_TYPE.HOSPICE && newTileObject is BedClinic bedClinic) {
                hospice.AddBed(bedClinic);
            }
        }
    }
}