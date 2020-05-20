using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;
using UtilityScripts;
namespace Locations.Features {
    public class VaporVentsFeature : TileFeature {

        private readonly int _maxVentCount;
        private HexTile _owner;
        
        public VaporVentsFeature() {
            name = "Vapor Vents";
            description = "This region has 3 to 4 Vapor Vents.";
            _maxVentCount = Random.Range(3, 5);
        }  
        
        #region Overrides
        public override void GameStartActions(HexTile tile) {
            base.GameStartActions(tile);
            //spawn Poison Vents
            for (int i = 0; i < _maxVentCount; i++) {
                CreateNewVent();
            }
        }
        public override void OnAddFeature(HexTile tile) {
            base.OnAddFeature(tile);
            _owner = tile;
        }
        public override void OnRemoveFeature(HexTile tile) {
            base.OnRemoveFeature(tile);
            _owner = null;
        }
        #endregion

        private void CreateNewVent() {
            List<LocationGridTile> choices = _owner.locationGridTiles.Where(x => x.isOccupied == false).ToList();
            if (choices.Count > 0) {
                TileObject tileObject =
                    InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.VAPOR_VENT);
                LocationGridTile tile = CollectionUtilities.GetRandomElement(choices);
                tile.structure.AddPOI(tileObject, tile);
            }
            else {
                Debug.LogWarning($"{GameManager.Instance.TodayLogString()}Could not place new poison vent at {_owner}");
            }
        }
    }
}