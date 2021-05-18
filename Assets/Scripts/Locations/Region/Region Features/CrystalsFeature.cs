using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;
using UtilityScripts;
namespace Locations.Region_Features {
    public class CrystalsFeature : RegionFeature {

        private readonly TILE_OBJECT_TYPE[] crystalChoices = new[] {
            TILE_OBJECT_TYPE.ELECTRIC_CRYSTAL, TILE_OBJECT_TYPE.FIRE_CRYSTAL, TILE_OBJECT_TYPE.ICE_CRYSTAL,
            TILE_OBJECT_TYPE.POISON_CRYSTAL, TILE_OBJECT_TYPE.WATER_CRYSTAL
        };
        
        public override void ActivateFeatureInWorldGen(Region region) {
            base.ActivateFeatureInWorldGen(region);
            int amount = Random.Range(40, 61);
            List<LocationGridTile> validWildernessTiles = region.wilderness.unoccupiedTiles.Where(t => t.IsPartOfSettlement() == false).ToList();
            for (int i = 0; i < amount; i++) {
                if (validWildernessTiles.Count == 0) { break; }
                LocationGridTile tile = CollectionUtilities.GetRandomElement(validWildernessTiles);
                TILE_OBJECT_TYPE randomCrystal = CollectionUtilities.GetRandomElement(crystalChoices);
                TileObject tileObject =
                    InnerMapManager.Instance.CreateNewTileObject<TileObject>(randomCrystal);
                tile.structure.AddPOI(tileObject, tile);
                validWildernessTiles.Remove(tile);
            }
        }
    }
}