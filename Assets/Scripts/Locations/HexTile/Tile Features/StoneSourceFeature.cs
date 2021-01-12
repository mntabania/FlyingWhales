using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;
using UtilityScripts;
namespace Locations.Tile_Features {
    public class StoneSourceFeature : TileFeature {

        private const int MaxRocks = 8;
    
        private HexTile owner;
        private int currentRockCount;
        private bool isGeneratingRockPerHour;
        
        public StoneSourceFeature() {
            name = "Stone Source";
            description = "Provides stone.";
        }

        #region Overrides
        public override void GameStartActions(HexTile tile) {
            owner = tile;
            Messenger.AddListener<TileObject, LocationGridTile>(GridTileSignals.TILE_OBJECT_PLACED, OnTileObjectPlaced);
            Messenger.AddListener<TileObject, Character, LocationGridTile>(GridTileSignals.TILE_OBJECT_REMOVED, OnTileObjectRemoved);
        
            List<TileObject> rocks = tile.GetTileObjectsInHexTile(TILE_OBJECT_TYPE.ROCK);
            currentRockCount = rocks.Count;
            if (rocks.Count < MaxRocks) {
                int missingTrees = MaxRocks - rocks.Count;
                for (int i = 0; i <= missingTrees; i++) {
                    if (!CreateNewRock()) {
                        break;
                    }
                }
            }
        }
        public override void OnRemoveFeature(HexTile tile) {
            base.OnRemoveFeature(tile);
            Messenger.RemoveListener(Signals.HOUR_STARTED, TryGenerateRockPerHour);
        }
        #endregion
    
        private void OnTileObjectPlaced(TileObject tileObject, LocationGridTile tile) {
            if (tile.collectionOwner.isPartOfParentRegionMap && tile.collectionOwner.partOfHextile.hexTileOwner == owner) {
                if (tileObject.tileObjectType == TILE_OBJECT_TYPE.ROCK) {
                    AdjustRockCount(1);    
                }
            }
        }
        private void OnTileObjectRemoved(TileObject tileObject, Character character, LocationGridTile tile) {
            if (tile.collectionOwner.isPartOfParentRegionMap && tile.collectionOwner.partOfHextile.hexTileOwner == owner) {
                if (tileObject.tileObjectType == TILE_OBJECT_TYPE.ROCK) {
                    AdjustRockCount(-1);    
                }
            }
        }

        #region Big Tree
        private void AdjustRockCount(int amount) {
            currentRockCount += amount;
            OnRockCountChanged();
        }
        private void OnRockCountChanged() {
            if (currentRockCount < MaxRocks) {
                if (isGeneratingRockPerHour == false) {
                    isGeneratingRockPerHour = true;
                    Messenger.AddListener(Signals.HOUR_STARTED, TryGenerateRockPerHour);    
                }
            } else {
                isGeneratingRockPerHour = false;
                Messenger.RemoveListener(Signals.HOUR_STARTED, TryGenerateRockPerHour);
            }
        }
        private void TryGenerateRockPerHour() {
            if (Random.Range(0, 100) < 10) {
                CreateNewRock();
            }
        }
        private bool CreateNewRock() {
            List<LocationGridTile> choices = owner.locationGridTiles.Where(x => x.objHere == null && x.structure.structureType.IsOpenSpace()).ToList();
            if (choices.Count > 0) {
                LocationGridTile chosenTile = CollectionUtilities.GetRandomElement(choices);
                chosenTile.structure.AddPOI(InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.ROCK), chosenTile);
                return true;
            }
            return false;
        }
        #endregion
    }
}