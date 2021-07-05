using System.Collections;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;
namespace Inner_Maps {
    public class GridTileCorruptionComponent : LocationGridTileComponent {
        private const int corruptionDurationInTicks = 5;
        private const int buildOrDestroyWallDurationInTicks = 5;
        public bool isCorrupted => owner.groundType == LocationGridTile.Ground_Type.Corrupted;
        public bool isCurrentlyBeingCorrupted { get; private set; }
        public bool willGenerateDemonicDecorOnCorruptionFinish { get; private set; }
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
            willGenerateDemonicDecorOnCorruptionFinish = data.willGenerateDemonicDecorOnCorruptionFinish;
        }

        #region Utilities
        public void StartCorruption(bool randomGenerateDemonicDecor = false) {
            if (!isCurrentlyBeingCorrupted && !isCorrupted) {
                if (!_buildSmokeEffect) {
                    _buildSmokeEffect = GameManager.Instance.CreateParticleEffectAt(owner, PARTICLE_EFFECT.Build_Grid_Tile_Smoke);
                }
                isCurrentlyBeingCorrupted = true;
                willGenerateDemonicDecorOnCorruptionFinish = randomGenerateDemonicDecor;
                corruptDate = GameManager.Instance.Today().AddTicks(corruptionDurationInTicks);
                _corruptionScheduleID = randomGenerateDemonicDecor ? SchedulingManager.Instance.AddEntry(corruptDate, CorruptTileAndRandomlyGenerateDemonicObject, null) : 
                    SchedulingManager.Instance.AddEntry(corruptDate, CorruptTile, null);
                owner.mouseEventsComponent.OnHoverExit();
                owner.SetIsDefault(false);
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
                TileObject tileObject = owner.tileObjectComponent.objHere;
                if (tileObject != null) {
                    if (tileObject is TreeObject tree && tree.mapObjectVisual is TileObjectGameObject tileObjectGameObject) {
                        if (tree is BigTreeObject) {
                            owner.structure.RemovePOI(tileObject);
                        } else {
                            tileObjectGameObject.UpdateTileObjectVisual(tree);    
                        }
                    } else if (tileObject is BlockWall blockWall) {
                        blockWall.SetWallType(WALL_TYPE.Demon_Stone);
                        blockWall.UpdateVisual(owner);
                    } else {
                        if (tileObject is Tombstone tombstone) {
                            tombstone.SetRespawnCorpseOnDestroy(false);
                        }
                        if (!tileObject.tileObjectType.IsTileObjectImportant() && !tileObject.traitContainer.HasTrait("Indestructible")) {
                            owner.structure.RemovePOI(tileObject);
                            
                        }
                    }
                }
                // owner.mouseEventsComponent.SetMouseEventsForAllNeighbours(true);
                owner.mouseEventsComponent.UpdateHasMouseEventsForAllNeighbours();
                owner.mouseEventsComponent.OnHoverExit();
            }
        }
        public void CorruptTileAndRandomlyGenerateDemonicObject() {
            isCurrentlyBeingCorrupted = false;
            if (_buildSmokeEffect) {
                ObjectPoolManager.Instance.DestroyObject(_buildSmokeEffect);
                _buildSmokeEffect = null;
            }
            if (!isCorrupted) {
                owner.SetGroundTilemapVisual(InnerMapManager.Instance.assetManager.corruptedTile);
                owner.CreateSeamlessEdgesForSelfAndNeighbours();
                TileObject tileObject = owner.tileObjectComponent.objHere;
                if (tileObject != null) {
                    if (tileObject is TreeObject tree && tree.mapObjectVisual is TileObjectGameObject tileObjectGameObject) {
                        if (tree is BigTreeObject) {
                            owner.structure.RemovePOI(tileObject);
                        } else {
                            tileObjectGameObject.UpdateTileObjectVisual(tree);    
                        }
                    } else if (tileObject is BlockWall blockWall) {
                        blockWall.SetWallType(WALL_TYPE.Demon_Stone);
                        blockWall.UpdateVisual(owner);
                    } else {
                        if (tileObject is Tombstone tombstone) {
                            tombstone.SetRespawnCorpseOnDestroy(false);
                        }
                        if (!tileObject.tileObjectType.IsTileObjectImportant() && !tileObject.traitContainer.HasTrait("Indestructible")) {
                            owner.structure.RemovePOI(tileObject);
                        }
                    }
                }
                if (owner.tileObjectComponent.objHere == null && ChanceData.RollChance(CHANCE_TYPE.Demonic_Decor_On_Corrupt)) {
                    TILE_OBJECT_TYPE tileObjectType = CollectionUtilities.GetRandomElement(GameUtilities.corruptionTileObjectChoices);
                    TileObject createdDecor = InnerMapManager.Instance.CreateNewTileObject<TileObject>(tileObjectType);
                    owner.structure.AddPOI(createdDecor, owner);
#if DEBUG_LOG
                    Debug.Log($"Placed random demonic decor {createdDecor.name} at {owner.ToString()}");
#endif
                }
                // owner.mouseEventsComponent.SetMouseEventsForAllNeighbours(true);
                owner.mouseEventsComponent.UpdateHasMouseEventsForAllNeighbours();
                owner.mouseEventsComponent.OnHoverExit();
            }
        }
        public void UncorruptTile() {
            if (isCorrupted) {
                owner.RevertTileToOriginalPerlin();
                owner.CreateSeamlessEdgesForSelfAndNeighbours();
                //if (owner.tileObjectComponent.objHere != null) {
                //    if (owner.tileObjectComponent.objHere is TileObject tileObject) {
                //        if (!tileObject.traitContainer.HasTrait("Indestructible")) {
                //            owner.structure.RemovePOI(owner.tileObjectComponent.objHere);
                //        }
                //    } else {
                //        owner.structure.RemovePOI(owner.tileObjectComponent.objHere);
                //    }
                //}
                owner.mouseEventsComponent.UpdateHasMouseEvents();
                // if (!IsTileAdjacentToACorruption()) {
                //     owner.mouseEventsComponent.SetHasMouseEvents(false);
                // }
                for (int i = 0; i < owner.neighbourList.Count; i++) {
                    LocationGridTile neighbour = owner.neighbourList[i];
                    neighbour.mouseEventsComponent.UpdateHasMouseEvents();
                    // if (!neighbour.corruptionComponent.isCorrupted) {
                    //     if (!neighbour.corruptionComponent.IsTileAdjacentToACorruption()) {
                    //         neighbour.mouseEventsComponent.SetHasMouseEvents(false);
                    //     }
                    // }
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
                if (owner.tileObjectComponent.objHere != null) {
                    owner.structure.RemovePOI(owner.tileObjectComponent.objHere);
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
                owner.tileObjectComponent.objHere.AdjustHP(-owner.tileObjectComponent.objHere.currentHP, ELEMENTAL_TYPE.Normal, true);
                owner.mouseEventsComponent.OnHoverExit();
            }
        }
        public bool CanBuildDemonicWall() {
            return isCorrupted && !wallIsBeingBuilt && !wallIsBeingDestroyed && owner.structure is Wilderness && 
                   (owner.tileObjectComponent.objHere == null || (!owner.tileObjectComponent.objHere.IsUnpassable() && !owner.tileObjectComponent.objHere.traitContainer.HasTrait("Indestructible"))) /*&& owner.tileObjectComponent.objHere == null&& owner.tileState != LocationGridTile.Tile_State.Occupied*/;
        }
        public bool CanDestroyDemonicWall() {
            return isCorrupted && !wallIsBeingDestroyed && !wallIsBeingBuilt && owner.tileObjectComponent.objHere is BlockWall wall && wall.wallType == WALL_TYPE.Demon_Stone;
        }
#endregion

#region Loading
        public void LoadSecondWave() {
            if (isCurrentlyBeingCorrupted) {
                if (!_buildSmokeEffect) {
                    _buildSmokeEffect = GameManager.Instance.CreateParticleEffectAt(owner, PARTICLE_EFFECT.Build_Grid_Tile_Smoke);
                }
                _corruptionScheduleID = willGenerateDemonicDecorOnCorruptionFinish ? SchedulingManager.Instance.AddEntry(corruptDate, CorruptTileAndRandomlyGenerateDemonicObject, null) : SchedulingManager.Instance.AddEntry(corruptDate, CorruptTile, null);
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
                owner.mouseEventsComponent.UpdateHasMouseEventsForSelfAndAllNeighbours();
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
        public bool willGenerateDemonicDecorOnCorruptionFinish;

        public override void Save(GridTileCorruptionComponent data) {
            base.Save(data);
            isCurrentlyBeingCorrupted = data.isCurrentlyBeingCorrupted;
            corruptDate = data.corruptDate;
            wallIsBeingBuilt = data.wallIsBeingBuilt;
            wallIsBeingDestroyed = data.wallIsBeingDestroyed;
            wallBuildOrDestroyDate = data.wallBuildOrDestroyDate;
            willGenerateDemonicDecorOnCorruptionFinish = data.willGenerateDemonicDecorOnCorruptionFinish;
        }
        public override GridTileCorruptionComponent Load() {
            GridTileCorruptionComponent component = new GridTileCorruptionComponent(this);
            return component;
        }
    }
}