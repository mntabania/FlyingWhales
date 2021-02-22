using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inner_Maps {
    public class GridTileCorruptionComponent : LocationGridTileComponent {
        private const int corruptionDurationInTicks = 5;
        private const int buildOrDestroyWallDurationInTicks = 5;
        public bool isCorrupted => owner.groundType == LocationGridTile.Ground_Type.Corrupted;
        public bool isCurrentlyBeingCorrupted { get; private set; }
        public bool wallIsBeingBuilt { get; private set; }
        public bool wallIsBeingDestroyed { get; private set; }
        public GameDate corruptDate { get; private set; }
        public GameDate wallBuildOrDestroyDate { get; private set; }

        private GameObject _buildSmokeEffect;
        private string _corruptionScheduleID;
        //private string _wallScheduleID;

        public GridTileCorruptionComponent() {
        }
        public GridTileCorruptionComponent(SaveDataGridTileCorruptionComponent data) {
            isCurrentlyBeingCorrupted = data.isCurrentlyBeingCorrupted;
            corruptDate = data.corruptDate;
            wallIsBeingBuilt = data.wallIsBeingBuilt;
            wallIsBeingDestroyed = data.wallIsBeingDestroyed;
            wallBuildOrDestroyDate = data.wallBuildOrDestroyDate;
        }

        #region Utilities
        public void StartCorruption() {
            if (!isCurrentlyBeingCorrupted && !isCorrupted) {
                if (!_buildSmokeEffect) {
                    _buildSmokeEffect = GameManager.Instance.CreateParticleEffectAt(owner, PARTICLE_EFFECT.Build_Grid_Tile_Smoke);
                }
                isCurrentlyBeingCorrupted = true;
                corruptDate = GameManager.Instance.Today().AddTicks(corruptionDurationInTicks);
                _corruptionScheduleID = SchedulingManager.Instance.AddEntry(corruptDate, CorruptTile, null);
                owner.mouseEventsComponent.OnHoverExit();
            }
        }
        public void DisruptCorruption() {
            if (isCurrentlyBeingCorrupted && !isCorrupted) {
                if (_buildSmokeEffect) {
                    ObjectPoolManager.Instance.DestroyObject(_buildSmokeEffect);
                    _buildSmokeEffect = null;
                }
                isCurrentlyBeingCorrupted = false;
                SchedulingManager.Instance.RemoveSpecificEntry(_corruptionScheduleID);
                owner.mouseEventsComponent.OnHoverExit();
            }
        }
        public void CorruptTile() {
            isCurrentlyBeingCorrupted = false;
            if (_buildSmokeEffect) {
                ObjectPoolManager.Instance.DestroyObject(_buildSmokeEffect);
                _buildSmokeEffect = null;
            }
            if (!isCorrupted) {
                owner.SetGroundTilemapVisual(InnerMapManager.Instance.assetManager.corruptedTile);
                owner.CreateSeamlessEdgesForSelfAndNeighbours();
                if (owner.objHere != null) {
                    if (owner.objHere is TreeObject tree) {
                        (tree.mapObjectVisual as TileObjectGameObject).UpdateTileObjectVisual(tree);
                    } else if (owner.objHere is BlockWall blockWall) {
                        blockWall.SetWallType(WALL_TYPE.Demon_Stone);
                        blockWall.UpdateVisual(owner);
                    } else {
                        if (owner.objHere is TileObject tileObject) {
                            if (owner.objHere is Tombstone tombstone) {
                                tombstone.SetRespawnCorpseOnDestroy(false);
                            }
                            if (!tileObject.tileObjectType.IsTileObjectImportant() && !tileObject.traitContainer.HasTrait("Indestructible")) {
                                owner.structure.RemovePOI(owner.objHere);
                            }
                        }
                        //structure.RemovePOI(objHere);
                    }
                }
                owner.mouseEventsComponent.SetMouseEventsForAllNeighbours(true);
                owner.mouseEventsComponent.OnHoverExit();
            }
        }
        public void UncorruptTile() {
            if (isCorrupted) {
                owner.RevertTileToOriginalPerlin();
                owner.CreateSeamlessEdgesForSelfAndNeighbours();
                //if (owner.objHere != null) {
                //    if (owner.objHere is TileObject tileObject) {
                //        if (!tileObject.traitContainer.HasTrait("Indestructible")) {
                //            owner.structure.RemovePOI(owner.objHere);
                //        }
                //    } else {
                //        owner.structure.RemovePOI(owner.objHere);
                //    }
                //}
                if (!IsTileAdjacentToACorruption()) {
                    owner.mouseEventsComponent.SetHasMouseEvents(false);
                }
                for (int i = 0; i < owner.neighbourList.Count; i++) {
                    LocationGridTile neighbour = owner.neighbourList[i];
                    if (!neighbour.corruptionComponent.isCorrupted) {
                        if (!neighbour.corruptionComponent.IsTileAdjacentToACorruption()) {
                            neighbour.mouseEventsComponent.SetHasMouseEvents(false);
                        }
                    }
                }
            } else {
                DisruptCorruption();
            }
        }
        public bool IsTileAdjacentToACorruption() {
            for (int i = 0; i < owner.neighbourList.Count; i++) {
                LocationGridTile neighbour = owner.neighbourList[i];
                if (neighbour.corruptionComponent.isCorrupted) {
                    return true;
                }
            }
            return false;
        }
        public bool CanCorruptTile() {
            //Can only corrupt wilderness grid tiles
            return IsTileAdjacentToACorruption() && !owner.hasBlueprint && owner.structure.structureType == STRUCTURE_TYPE.WILDERNESS && !isCorrupted && !isCurrentlyBeingCorrupted;
        }
        public bool CanDisruptCorruptionOfTile() {
            return !isCorrupted && isCurrentlyBeingCorrupted;
        }
        #endregion

        #region Demonic Wall
        public void StartBuildDemonicWall() {
            if (CanBuildDemonicWall()) {
                if (!_buildSmokeEffect) {
                    _buildSmokeEffect = GameManager.Instance.CreateParticleEffectAt(owner, PARTICLE_EFFECT.Build_Grid_Tile_Smoke);
                }
                wallIsBeingBuilt = true;

                wallBuildOrDestroyDate = GameManager.Instance.Today().AddTicks(buildOrDestroyWallDurationInTicks);
                SchedulingManager.Instance.AddEntry(wallBuildOrDestroyDate, BuildDemonicWall, null); //_wallScheduleID = 
                owner.mouseEventsComponent.OnHoverExit();
            }
        }
        public void BuildDemonicWall() {
            wallIsBeingBuilt = false;
            if (CanBuildDemonicWall()) {
                if (_buildSmokeEffect) {
                    ObjectPoolManager.Instance.DestroyObject(_buildSmokeEffect);
                    _buildSmokeEffect = null;
                }
                BlockWall wall = InnerMapManager.Instance.CreateNewTileObject<BlockWall>(TILE_OBJECT_TYPE.BLOCK_WALL);
                wall.SetWallType(WALL_TYPE.Demon_Stone);
                owner.structure.AddPOI(wall, owner);
                owner.mouseEventsComponent.OnHoverExit();
            }
        }
        public void StartDestroyDemonicWall() {
            if (CanDestroyDemonicWall()) {
                if (!_buildSmokeEffect) {
                    _buildSmokeEffect = GameManager.Instance.CreateParticleEffectAt(owner, PARTICLE_EFFECT.Build_Grid_Tile_Smoke);
                }
                wallIsBeingDestroyed = true;
                wallBuildOrDestroyDate = GameManager.Instance.Today().AddTicks(buildOrDestroyWallDurationInTicks);
                SchedulingManager.Instance.AddEntry(wallBuildOrDestroyDate, DestroyDemonicWall, null); //_wallScheduleID = 
                owner.mouseEventsComponent.OnHoverExit();
            }
        }
        public void DestroyDemonicWall() {
            wallIsBeingDestroyed = false;
            if (CanDestroyDemonicWall()) {
                if (_buildSmokeEffect) {
                    ObjectPoolManager.Instance.DestroyObject(_buildSmokeEffect);
                    _buildSmokeEffect = null;
                }
                owner.objHere.AdjustHP(-owner.objHere.currentHP, ELEMENTAL_TYPE.Normal, true);
                owner.mouseEventsComponent.OnHoverExit();
            }
        }
        public bool CanBuildDemonicWall() {
            return isCorrupted && !wallIsBeingBuilt && !wallIsBeingDestroyed && owner.objHere == null && owner.tileState != LocationGridTile.Tile_State.Occupied;
        }
        public bool CanDestroyDemonicWall() {
            return isCorrupted && !wallIsBeingDestroyed && !wallIsBeingBuilt && owner.objHere is BlockWall wall && wall.wallType == WALL_TYPE.Demon_Stone;
        }
        #endregion

        #region Loading
        public void LoadSecondWave() {
            if (isCurrentlyBeingCorrupted) {
                if (!_buildSmokeEffect) {
                    _buildSmokeEffect = GameManager.Instance.CreateParticleEffectAt(owner, PARTICLE_EFFECT.Build_Grid_Tile_Smoke);
                }
                _corruptionScheduleID = SchedulingManager.Instance.AddEntry(corruptDate, CorruptTile, null);
            } else if (wallIsBeingBuilt) {
                if (!_buildSmokeEffect) {
                    _buildSmokeEffect = GameManager.Instance.CreateParticleEffectAt(owner, PARTICLE_EFFECT.Build_Grid_Tile_Smoke);
                }
                SchedulingManager.Instance.AddEntry(wallBuildOrDestroyDate, BuildDemonicWall, null); //_wallScheduleID = 
            } else if (wallIsBeingDestroyed) {
                if (!_buildSmokeEffect) {
                    _buildSmokeEffect = GameManager.Instance.CreateParticleEffectAt(owner, PARTICLE_EFFECT.Build_Grid_Tile_Smoke);
                }
                SchedulingManager.Instance.AddEntry(wallBuildOrDestroyDate, DestroyDemonicWall, null); //_wallScheduleID = 
            }
            if (isCorrupted) {
                owner.mouseEventsComponent.SetMouseEventsForAllNeighbours(true);
                owner.mouseEventsComponent.OnHoverExit();
            }
        }
        #endregion
    }
    public class SaveDataGridTileCorruptionComponent : SaveData<GridTileCorruptionComponent>
    {
        public bool isCurrentlyBeingCorrupted;
        public GameDate corruptDate;
        public bool wallIsBeingBuilt;
        public bool wallIsBeingDestroyed;
        public GameDate wallBuildOrDestroyDate;

        public override void Save(GridTileCorruptionComponent data) {
            base.Save(data);
            isCurrentlyBeingCorrupted = data.isCurrentlyBeingCorrupted;
            corruptDate = data.corruptDate;
            wallIsBeingBuilt = data.wallIsBeingBuilt;
            wallIsBeingDestroyed = data.wallIsBeingDestroyed;
            wallBuildOrDestroyDate = data.wallBuildOrDestroyDate;
        }
        public override GridTileCorruptionComponent Load() {
            GridTileCorruptionComponent component = new GridTileCorruptionComponent(this);
            return component;
        }
    }
}