using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;
using UtilityScripts;
namespace Locations.Area_Features {
    public class StoneSourceFeature : AreaFeature {

        private const int MaxRocks = 8;
    
        private Area owner;
        private int currentRockCount;
        private bool isGeneratingRockPerHour;
        
        public StoneSourceFeature() {
            name = "Stone Source";
            description = "Provides stone.";
        }

        #region Overrides
        public override void GameStartActions(Area p_area) {
            owner = p_area;
            Messenger.AddListener<TileObject, LocationGridTile>(GridTileSignals.TILE_OBJECT_PLACED, OnTileObjectPlaced);
            Messenger.AddListener<TileObject, Character, LocationGridTile>(GridTileSignals.TILE_OBJECT_REMOVED, OnTileObjectRemoved);
        
            int rocksCount = p_area.tileObjectComponent.GetNumberOfTileObjectsInHexTile(TILE_OBJECT_TYPE.ROCK);
            currentRockCount = rocksCount;
            if (rocksCount < MaxRocks) {
                int missingTrees = MaxRocks - rocksCount;
                for (int i = 0; i <= missingTrees; i++) {
                    if (!CreateNewRock()) {
                        break;
                    }
                }
            }
        }
        public override void OnRemoveFeature(Area p_area) {
            base.OnRemoveFeature(p_area);
            Messenger.RemoveListener(Signals.HOUR_STARTED, TryGenerateRockPerHour);
        }
        #endregion
    
        private void OnTileObjectPlaced(TileObject tileObject, LocationGridTile tile) {
            if (tile.area == owner) {
                if (tileObject.tileObjectType == TILE_OBJECT_TYPE.ROCK) {
                    AdjustRockCount(1);    
                }
            }
        }
        private void OnTileObjectRemoved(TileObject tileObject, Character character, LocationGridTile tile) {
            if (tile.area == owner) {
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
            List<LocationGridTile> choices = RuinarchListPool<LocationGridTile>.Claim();
            for (int i = 0; i < owner.gridTileComponent.gridTiles.Count; i++) {
                LocationGridTile x = owner.gridTileComponent.gridTiles[i];
                if (x.tileObjectComponent.objHere == null && x.structure.structureType.IsOpenSpace() && x.IsPassable()) {
                    choices.Add(x);
                }
            }
            LocationGridTile chosenTile = null;
            if (choices.Count > 0) {
                chosenTile = CollectionUtilities.GetRandomElement(choices);
            }
            if (chosenTile != null) {
                chosenTile.structure.AddPOI(InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.ROCK), chosenTile);
                return true;
            }
            return false;
        }
        #endregion
    }
}