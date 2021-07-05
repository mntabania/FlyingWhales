using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;
using UtilityScripts;
namespace Locations.Area_Features {
    public class WoodSourceFeature : AreaFeature {
        private const int MaxBigTrees = 4;
        private const int MaxSmallTrees = 8;
    
        private Area owner;
        private int currentBigTreeCount;
        private int currentSmallTreeCount;
        private bool isGeneratingBigTreePerHour;
        private bool isGeneratingSmallTreePerHour;

        public WoodSourceFeature() {
            name = "Wood Source";
            description = "Provides wood.";
        }  
    
        #region Overrides
        public override void GameStartActions(Area p_area) {
            owner = p_area;
            Messenger.AddListener<TileObject, LocationGridTile>(GridTileSignals.TILE_OBJECT_PLACED, OnTileObjectPlaced);
            Messenger.AddListener<TileObject, Character, LocationGridTile>(GridTileSignals.TILE_OBJECT_REMOVED, OnTileObjectRemoved);
        
            int bigTreesCount = p_area.tileObjectComponent.GetNumberOfTileObjectsInHexTile(TILE_OBJECT_TYPE.BIG_TREE_OBJECT);
            currentBigTreeCount = bigTreesCount;
            // if (bigTreesCount < MaxBigTrees) {
            //     int missingTrees = MaxBigTrees - bigTreesCount;
            //     for (int i = 0; i <= missingTrees; i++) {
            //         if (CreateNewBigTree() == false) {
            //             break;
            //         }
            //     }
            // }
            //
            int smallTreesCount = p_area.tileObjectComponent.GetNumberOfTileObjectsInHexTile(TILE_OBJECT_TYPE.SMALL_TREE_OBJECT);
            currentSmallTreeCount = smallTreesCount;
            // if (smallTreesCount < MaxSmallTrees) {
            //     int missingTrees = MaxSmallTrees - smallTreesCount;
            //     for (int i = 0; i <= missingTrees; i++) {
            //         if (CreateNewSmallTree() == false) {
            //             break;
            //         }
            //     }
            // }
        }
        public override void OnRemoveFeature(Area p_area) {
            base.OnRemoveFeature(p_area);
            Messenger.RemoveListener(Signals.HOUR_STARTED, TryGenerateBigTreePerHour);
            Messenger.RemoveListener(Signals.HOUR_STARTED, TryGenerateSmallTreePerHour);
        }
        #endregion
    
        private void OnTileObjectPlaced(TileObject tileObject, LocationGridTile tile) {
            if (tile.area == owner) {
                if (tileObject.tileObjectType == TILE_OBJECT_TYPE.BIG_TREE_OBJECT) {
                    AdjustBigTreeCount(1);    
                } else if (tileObject.tileObjectType == TILE_OBJECT_TYPE.SMALL_TREE_OBJECT) {
                    AdjustSmallTreeCount(1);
                }
            
            }
        }
        private void OnTileObjectRemoved(TileObject tileObject, Character character, LocationGridTile tile) {
            if (tile.area == owner) {
                if (tileObject.tileObjectType == TILE_OBJECT_TYPE.BIG_TREE_OBJECT) {
                    AdjustBigTreeCount(-1);    
                } else if (tileObject.tileObjectType == TILE_OBJECT_TYPE.SMALL_TREE_OBJECT) {
                    AdjustSmallTreeCount(-1);
                }
            }
        }

        #region Big Tree
        private void AdjustBigTreeCount(int amount) {
            currentBigTreeCount += amount;
            OnBigTreeCountChanged();
        }
        private void OnBigTreeCountChanged() {
            if (currentBigTreeCount < MaxBigTrees) {
                if (isGeneratingBigTreePerHour == false) {
                    isGeneratingBigTreePerHour = true;
                    Messenger.AddListener(Signals.HOUR_STARTED, TryGenerateBigTreePerHour);    
                }
            } else {
                isGeneratingBigTreePerHour = false;
                Messenger.RemoveListener(Signals.HOUR_STARTED, TryGenerateBigTreePerHour);
            }
        }
        private void TryGenerateBigTreePerHour() {
            if (Random.Range(0, 100) < 10) {
                CreateNewBigTree();
            }
        }
        private bool CreateNewBigTree() {
            LocationGridTile chosenTile = null;
            List<LocationGridTile> choices = RuinarchListPool<LocationGridTile>.Claim();
            for (int i = 0; i < owner.gridTileComponent.gridTiles.Count; i++) {
                LocationGridTile x = owner.gridTileComponent.gridTiles[i];
                if (x.isOccupied == false && x.structure.structureType.IsOpenSpace() && InnerMapManager.Instance.CanBigTreeBePlacedOnTile(x)) {
                    choices.Add(x);
                }
            }
            if (choices.Count > 0) {
                chosenTile = CollectionUtilities.GetRandomElement(choices);
            }
            RuinarchListPool<LocationGridTile>.Release(choices);
            if (chosenTile != null) {
                chosenTile.structure.AddPOI(InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.BIG_TREE_OBJECT), chosenTile);
                return true;
            }
            return false;
        }
        #endregion
    
        #region Small Tree
        private void AdjustSmallTreeCount(int amount) {
            currentSmallTreeCount += amount;
            OnSmallTreeCountChanged();
        }
        private void OnSmallTreeCountChanged() {
            if (currentSmallTreeCount < MaxSmallTrees) {
                if (isGeneratingSmallTreePerHour == false) {
                    isGeneratingSmallTreePerHour = true;
                    Messenger.AddListener(Signals.HOUR_STARTED, TryGenerateSmallTreePerHour);    
                }
            } else {
                isGeneratingSmallTreePerHour = false;
                Messenger.RemoveListener(Signals.HOUR_STARTED, TryGenerateSmallTreePerHour);
            }
        }
        private void TryGenerateSmallTreePerHour() {
            if (Random.Range(0, 100) < 10) {
                CreateNewSmallTree();
            }
        }
        private bool CreateNewSmallTree() {
            List<LocationGridTile> choices = owner.gridTileComponent.gridTiles.Where(x => x.isOccupied == false 
                                                                                && x.structure.structureType.IsOpenSpace()
                                                                                && x.groundType != LocationGridTile.Ground_Type.Bone 
                                                                                && x.groundType != LocationGridTile.Ground_Type.Water).ToList();
            if (choices.Count > 0) {
                LocationGridTile chosenTile = CollectionUtilities.GetRandomElement(choices);
                chosenTile.structure.AddPOI(InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.SMALL_TREE_OBJECT),
                    chosenTile);
                return true;
            }
            return false;
        }
        #endregion

    
    }
}