using System;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;
namespace Inner_Maps.Location_Structures {
    public class Mine : ManMadeStructure {
        public Cave connectedCave { get; private set; }
        public override Type serializedData => typeof(SaveDataMine);
        public Mine(Region location) : base(STRUCTURE_TYPE.MINE, location) {
            SetMaxHPAndReset(4000);
        }
        public Mine(Region location, SaveDataManMadeStructure data) : base(location, data) {
            SetMaxHP(4000);
        }

        #region Loading
        public override void LoadReferences(SaveDataLocationStructure saveDataLocationStructure) {
            base.LoadReferences(saveDataLocationStructure);
            SaveDataMine saveDataMineShack = saveDataLocationStructure as SaveDataMine;
            if (!string.IsNullOrEmpty(saveDataMineShack.connectedCaveID)) {
                connectedCave = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(saveDataMineShack.connectedCaveID) as Cave;
            }
        }
        #endregion
        
        #region Damage
        public override void OnTileDamaged(LocationGridTile tile, int amount, bool isPlayerSource) {
            //mines can be damaged  by any tile
            AdjustHP(amount, isPlayerSource: isPlayerSource);
            OnStructureDamaged();
        }
        #endregion
        
        public override string GetTestingInfo() {
            return $"{base.GetTestingInfo()}\nConnected Cave {connectedCave?.name}";
        }
        public override void OnUseStructureConnector(LocationGridTile p_usedConnector) {
            base.OnUseStructureConnector(p_usedConnector);
            Assert.IsTrue(p_usedConnector.structure is Cave, $"{name} did not connect to a tile inside a cave!");
            connectedCave = p_usedConnector.structure as Cave;
            Assert.IsNotNull(connectedCave);
            connectedCave.ConnectMine(this);
            //Create a path inside
            Area area = p_usedConnector.area;
            List<LocationGridTile> choices = RuinarchListPool<LocationGridTile>.Claim();
            List<LocationGridTile> filteredChoices = RuinarchListPool<LocationGridTile>.Claim();
            p_usedConnector.PopulateTilesInRadius(choices, 10, includeTilesInDifferentStructure: true); //.Where(t => t.IsPassable() && t.structure == connectedCave).ToList();
            for (int i = 0; i < choices.Count; i++) {
                LocationGridTile t = choices[i];
                if (t.IsPassable() && t.structure == connectedCave) {
                    filteredChoices.Add(t);
                }
            }
            RuinarchListPool<LocationGridTile>.Release(choices);
            LocationGridTile randomPassableTile = null;
            if (filteredChoices.Count > 0) {
                randomPassableTile = CollectionUtilities.GetRandomElement(filteredChoices);
            } else if (connectedCave.passableTiles.Count > 0) {
                randomPassableTile = CollectionUtilities.GetRandomElement(connectedCave.passableTiles);
            } else if (connectedCave.tiles.Count > 0) {
                randomPassableTile = CollectionUtilities.GetRandomElement(connectedCave.tiles);
            } else {
                throw new Exception("No random passable tile");
            }
            RuinarchListPool<LocationGridTile>.Release(filteredChoices);
            List<LocationGridTile> path = PathGenerator.Instance.GetPath(p_usedConnector, randomPassableTile, GRID_PATHFINDING_MODE.CAVE_INTERCONNECTION, includeFirstTile: true);
            if (path != null) {
                Debug.Log($"Connected {name} to {connectedCave.name}. Path is:\n {path.ComafyList()}");
                for (int i = 0; i < path.Count; i++) {
                    LocationGridTile pathTile = path[i];
                    if (pathTile.tileObjectComponent.objHere is BlockWall || pathTile.tileObjectComponent.objHere is OreVein) {
                        pathTile.structure.RemovePOI(pathTile.tileObjectComponent.objHere);
                        if (!GameManager.Instance.gameHasStarted && WorldConfigManager.Instance.mapGenerationData != null) {
                            //added this checking for mines that are created on randomized map. Since tile objects aren't created immediately
                            WorldConfigManager.Instance.mapGenerationData.SetGeneratedMapPerlinDetails(pathTile, TILE_OBJECT_TYPE.NONE);
                        }
                    } else if (!GameManager.Instance.gameHasStarted && WorldConfigManager.Instance.mapGenerationData != null) {
                        TILE_OBJECT_TYPE tileObjectType = WorldConfigManager.Instance.mapGenerationData.GetGeneratedObjectOnTile(pathTile);
                        if (tileObjectType == TILE_OBJECT_TYPE.BLOCK_WALL || tileObjectType == TILE_OBJECT_TYPE.ORE_VEIN) {
                            WorldConfigManager.Instance.mapGenerationData.SetGeneratedMapPerlinDetails(pathTile, TILE_OBJECT_TYPE.NONE);
                            pathTile.SetStructureTilemapVisual(null);
                        }
                    }		
                }
            }
        }
        protected override void DestroyStructure(Character p_responsibleCharacter = null, bool isPlayerSource = false) {
            base.DestroyStructure(p_responsibleCharacter, isPlayerSource);
            connectedCave.DisconnectMine(this);
            connectedCave = null;
        }

        private void PopulateList(List<TileObject> p_list, TILE_OBJECT_TYPE p_type) {
            p_list.Clear();
            List<TileObject> unsortedList = GetTileObjectsOfType(p_type);
            if (unsortedList != null) {
                for (int x = 0; x < unsortedList.Count; ++x) {
                    if (unsortedList[x].mapObjectState == MAP_OBJECT_STATE.BUILT && !unsortedList[x].HasJobTargetingThis(JOB_TYPE.HAUL, JOB_TYPE.COMBINE_STOCKPILE)) {
                        p_list.Add(unsortedList[x]);
                    }
                }
            }
        }

        void SetListToVariable(List<TileObject> builtPilesInSideStructure) {
            PopulateList(builtPilesInSideStructure, TILE_OBJECT_TYPE.COPPER);
            if (builtPilesInSideStructure.Count > 1) {
                return;
            }
            PopulateList(builtPilesInSideStructure, TILE_OBJECT_TYPE.IRON);
            if (builtPilesInSideStructure.Count > 1) {
                return;
            }
            PopulateList(builtPilesInSideStructure, TILE_OBJECT_TYPE.ORICHALCUM);
            if (builtPilesInSideStructure.Count > 1) {
                return;
            }
            PopulateList(builtPilesInSideStructure, TILE_OBJECT_TYPE.MITHRIL);
            if (builtPilesInSideStructure.Count > 1) {
                return;
            }
            PopulateList(builtPilesInSideStructure, TILE_OBJECT_TYPE.STONE_PILE);
            if (builtPilesInSideStructure.Count > 1) {
                return;
            }
        }

        protected override void ProcessWorkStructureJobsByWorker(Character p_worker, out JobQueueItem producedJob) {
            producedJob = null;
            ResourcePile pile = p_worker.homeSettlement.SettlementResources.GetRandomPileOfMetalOrStoneForMineHaul(p_worker.homeSettlement);
            if (pile == null && connectedCave != null) {
                pile = connectedCave.GetRandomTileObjectOfTypeThatHasTileLocationAndIsBuiltAndIsNotTargetedByHaulOrCombine<StonePile>();
                if (pile == null) {
                    pile = connectedCave.GetRandomTileObjectOfTypeThatHasTileLocationAndIsBuiltAndIsNotTargetedByHaulOrCombine<MetalPile>();
                }
            }
            if (pile != null) {
                if (p_worker.structureComponent.workPlaceStructure.unoccupiedTiles.Count > 0) {
                    p_worker.jobComponent.TryCreateHaulJob(pile, out producedJob);
                    if (producedJob != null) {
                        return;
                    }
                }
            }
            //else {
            //    SetListToVariable(connectedCave);
            //    if (builtPilesInSideStructure != null && builtPilesInSideStructure.Count > 0) {
            //        p_worker.jobComponent.TryCreateHaulJob(builtPilesInSideStructure[0] as ResourcePile, out producedJob);
            //        if (producedJob != null) {
            //            return;
            //        }
            //    }
            //}
            List<TileObject> builtPilesInSideStructure = RuinarchListPool<TileObject>.Claim();
            SetListToVariable(builtPilesInSideStructure);
            if (builtPilesInSideStructure.Count > 1) {
                //always ensure that the first pile is the pile that all other piles will be dropped to, this is to prevent complications
                //when multiple workers are combining piles, causing targets of other jobs to mess up since their target pile was carried.
                p_worker.jobComponent.TryCreateCombineStockpile(builtPilesInSideStructure[1] as ResourcePile, builtPilesInSideStructure[0] as ResourcePile, out producedJob);
                if (producedJob != null) {
                    RuinarchListPool<TileObject>.Release(builtPilesInSideStructure);
                    return;
                }
            }
            RuinarchListPool<TileObject>.Release(builtPilesInSideStructure);

            List<TileObject> piles = RuinarchListPool<TileObject>.Claim();
            if (p_worker.talentComponent.GetTalent(CHARACTER_TALENT.Resources).level >= 4) {
                PopulateMetalsIncave(piles);
                PopulateStonesInCave(piles);
                if (piles.Count > 0) {
                    TileObject chosenPile = piles[GameUtilities.RandomBetweenTwoNumbers(0, piles.Count - 1)];
                    if (chosenPile.tileObjectType == TILE_OBJECT_TYPE.ORE) {
                        p_worker.jobComponent.TriggerMineOre(chosenPile, out producedJob);
                    } else {
                        p_worker.jobComponent.TriggerMineStone(chosenPile, out producedJob);
                    }
                    
                    if (producedJob != null) {
                        RuinarchListPool<TileObject>.Release(piles);
                        return;
                    }
                }
            }

            piles.Clear();
            PopulateStonesInCave(piles);
            if (piles.Count > 0) {
                TileObject chosenPile = piles[GameUtilities.RandomBetweenTwoNumbers(0, piles.Count - 1)];
                p_worker.jobComponent.TriggerMineStone(chosenPile, out producedJob);
                if (producedJob != null) {
                    RuinarchListPool<TileObject>.Release(piles);
                    return;
                }
            }
            
            RuinarchListPool<TileObject>.Release(piles);
            
            if(TryCreateCleanJob(p_worker, out producedJob)) { return; }
        }

        public void PopulateMetalsIncave(List<TileObject> availMetals) {
            List<TileObject> metals = RuinarchListPool<TileObject>.Claim();
            connectedCave.PopulateTileObjectsOfType<Ore>(metals);
            for (int x = 0; x < metals.Count; ++x) {
                if (metals[x].mapObjectState == MAP_OBJECT_STATE.BUILT) {
                    availMetals.Add(metals[x]);
                }
            }
            RuinarchListPool<TileObject>.Release(metals);
        }

        public void PopulateStonesInCave(List<TileObject> availStones) {
            List<TileObject> stones = RuinarchListPool<TileObject>.Claim();
            connectedCave.PopulateTileObjectsOfType<Rock>(stones);
            for (int x = 0; x < stones.Count; ++x) {
                TileObject stone = stones[x];
                if (stone.mapObjectState == MAP_OBJECT_STATE.BUILT) {
                    availStones.Add(stone);
                }
            }
            RuinarchListPool<TileObject>.Release(stones);
        }
        
        #region Worker
        public override bool CanHireAWorker() {
            return true;
        }
        #endregion
        
        #region Purchasing
        public override bool CanPurchaseFromHere(Character p_buyer, out bool needsToPay, out int buyerOpinionOfWorker) {
            needsToPay = true;
            buyerOpinionOfWorker = 0;
            return true; //anyone can buy from basic resource producing structures, but everyone also needs to pay. NOTE: It is intended that villagers can buy from unassigned structures
        }
        #endregion
    }
}

#region Save Data
public class SaveDataMine : SaveDataManMadeStructure {

    public string connectedCaveID;
    
    public override void Save(LocationStructure locationStructure) {
        base.Save(locationStructure);
        Inner_Maps.Location_Structures.Mine mineShack = locationStructure as Inner_Maps.Location_Structures.Mine;
        if (mineShack.connectedCave != null) {
            connectedCaveID = mineShack.connectedCave.persistentID;
        }
    }
}
#endregion