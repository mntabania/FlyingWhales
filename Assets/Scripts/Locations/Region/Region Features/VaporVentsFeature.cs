using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;
using UtilityScripts;
namespace Locations.Region_Features {
    public class VaporVentsFeature : RegionFeature {
        public override void ActivateFeatureInWorldGen(Region region) {
            base.ActivateFeatureInWorldGen(region);
            int randomVentAmount = Random.Range(20, 31);
            List<LocationGridTile> validWildernessTiles = region.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS)
                .unoccupiedTiles.Where(t => t.IsPartOfSettlement() == false).ToList();
            for (int i = 0; i < randomVentAmount; i++) {
                if (validWildernessTiles.Count == 0) { break; }
                LocationGridTile tile = CollectionUtilities.GetRandomElement(validWildernessTiles);
                TileObject tileObject =
                    InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.VAPOR_VENT);
                tile.structure.AddPOI(tileObject, tile);
                validWildernessTiles.Remove(tile);
            }
        }
    }
}