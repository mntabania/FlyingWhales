using System.Collections.Generic;
using Inner_Maps;
using UtilityScripts;
namespace Locations.Area_Features {
    public class PoisonVentsFeature : AreaFeature {
        public PoisonVentsFeature() {
            name = "Poison Vents";
            description = "This location is naturally emitting poison clouds.";
        }
        public override void OnAddFeature(Area p_area) {
            base.OnAddFeature(p_area);
            int randomVentAmount = UnityEngine.Random.Range(10, 21);

            List<LocationGridTile> validTiles = RuinarchListPool<LocationGridTile>.Claim();
            PopulateValidTilesForPoisonVent(validTiles, p_area);
            
            for (int i = 0; i < randomVentAmount; i++) {
                if (validTiles.Count == 0) { break; }
                LocationGridTile tile = CollectionUtilities.GetRandomElement(validTiles);
                TileObject tileObject = InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.POISON_VENT);
                tile.structure.AddPOI(tileObject, tile);
                validTiles.Remove(tile);
            }
            RuinarchListPool<LocationGridTile>.Release(validTiles);
        }

        private void PopulateValidTilesForPoisonVent(List<LocationGridTile> p_tiles, Area p_area) {
            for (int i = 0; i < p_area.gridTileComponent.passableTiles.Count; i++) {
                LocationGridTile tile = p_area.gridTileComponent.passableTiles[i];
                if (tile.tileObjectComponent.objHere == null && !tile.isOccupied && tile.structure.structureType == STRUCTURE_TYPE.WILDERNESS) {
                    p_tiles.Add(tile);
                }
            }
        }
    }
}