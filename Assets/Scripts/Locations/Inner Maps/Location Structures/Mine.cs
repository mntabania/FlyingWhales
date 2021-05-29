using System;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;
namespace Inner_Maps.Location_Structures {
    public class Mine : ManMadeStructure {

        List<TileObject> builtPilesInSideStructure = RuinarchListPool<TileObject>.Claim();
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
        
        public override string GetTestingInfo() {
            return $"{base.GetTestingInfo()}\nConnected Cave {connectedCave?.name}";
        }
        public override void OnUseStructureConnector(LocationGridTile p_usedConnector) {
            base.OnUseStructureConnector(p_usedConnector);
            Assert.IsTrue(p_usedConnector.structure is Cave, $"{name} did not connect to a tile inside a cave!");
            connectedCave = p_usedConnector.structure as Cave;
            Assert.IsNotNull(connectedCave);
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
            connectedCave = null;
        }

        List<TileObject> CreateClothAndLeatherList(TILE_OBJECT_TYPE p_type, LocationStructure p_targetStructure = null) {
            if(p_targetStructure == null) {
                p_targetStructure = this;
            }
            List<TileObject> createdList = RuinarchListPool<TileObject>.Claim();
            List < TileObject > unsortedList = p_targetStructure.GetTileObjectsOfType(p_type);
            if (unsortedList != null) {
                for (int x = 0; x < unsortedList.Count; ++x) {
                    if (unsortedList[x].mapObjectState == MAP_OBJECT_STATE.BUILT && !((unsortedList[x] as TileObject).HasJobTargetingThis(JOB_TYPE.HAUL))) {
                        createdList.Add(unsortedList[x]);
                    }
                }
            }
            return createdList;
        }

        void SetListToVariable(LocationStructure p_targetStructure = null) {
            builtPilesInSideStructure = CreateClothAndLeatherList(TILE_OBJECT_TYPE.COPPER, p_targetStructure);
            if (builtPilesInSideStructure != null && builtPilesInSideStructure.Count > 1) {
                return;
            }
            builtPilesInSideStructure = CreateClothAndLeatherList(TILE_OBJECT_TYPE.IRON, p_targetStructure);
            if (builtPilesInSideStructure != null && builtPilesInSideStructure.Count > 1) {
                return;
            }
            builtPilesInSideStructure = CreateClothAndLeatherList(TILE_OBJECT_TYPE.ORICHALCUM, p_targetStructure);
            if (builtPilesInSideStructure != null && builtPilesInSideStructure.Count > 1) {
                return;
            }
            builtPilesInSideStructure = CreateClothAndLeatherList(TILE_OBJECT_TYPE.MITHRIL, p_targetStructure);
            if (builtPilesInSideStructure != null && builtPilesInSideStructure.Count > 1) {
                return;
            }
            builtPilesInSideStructure = CreateClothAndLeatherList(TILE_OBJECT_TYPE.DIAMOND, p_targetStructure);
            if (builtPilesInSideStructure != null && builtPilesInSideStructure.Count > 1) {
                return;
            }
            builtPilesInSideStructure = CreateClothAndLeatherList(TILE_OBJECT_TYPE.GOLD, p_targetStructure);
            if (builtPilesInSideStructure != null && builtPilesInSideStructure.Count > 1) {
                return;
            }
            builtPilesInSideStructure = CreateClothAndLeatherList(TILE_OBJECT_TYPE.STONE_PILE, p_targetStructure);
            if (builtPilesInSideStructure != null && builtPilesInSideStructure.Count > 1) {
                return;
            }
        }

        protected override void ProcessWorkStructureJobsByWorker(Character p_worker, out JobQueueItem producedJob) {
            producedJob = null;
            ResourcePile pile = p_worker.currentSettlement.SettlementResources.GetRandomPileOfMetalOrStone();
            if (pile != null) {
                p_worker.jobComponent.TryCreateHaulJob(pile, out producedJob);
                if (producedJob != null) {
                    return;
                }
            } else {
                SetListToVariable(connectedCave);
                if (builtPilesInSideStructure != null && builtPilesInSideStructure.Count > 0) {
                    p_worker.jobComponent.TryCreateHaulJob(builtPilesInSideStructure[0] as ResourcePile, out producedJob);
                    if (producedJob != null) {
                        return;
                    }
                }
            }
            SetListToVariable();
            if (builtPilesInSideStructure != null && builtPilesInSideStructure.Count > 1) {
                p_worker.jobComponent.TryCreateCombineStockpile(builtPilesInSideStructure[0] as ResourcePile, builtPilesInSideStructure[1] as ResourcePile, out producedJob);
                if (producedJob != null) {
                    return;
                }
            }
            List<TileObject> piles = RuinarchListPool<TileObject>.Claim();
            if (p_worker.talentComponent.GetTalent(CHARACTER_TALENT.Resources).level >= 4) {
                piles = GetMetalTypeResourcePiles();
                if (piles != null && piles.Count > 0) {
                    p_worker.jobComponent.TriggerMineOre(piles[0], out producedJob);
                    if (producedJob != null) {
                        return;
                    }
                }
            } else {
                piles = GetStonelTypeResourcePiles();
                if (piles != null && piles.Count > 0) {
                    p_worker.jobComponent.TriggerMineStone(piles[0], out producedJob);
                    if (producedJob != null) {
                        return;
                    }
                }
            }
        }

        public List<TileObject> GetMetalTypeResourcePiles() {
            List<TileObject> metals = connectedCave.GetTileObjectsOfType(TILE_OBJECT_TYPE.ORE);
            List<TileObject> availMetals = connectedCave.GetTileObjectsOfType(TILE_OBJECT_TYPE.ORE);
            for (int x = 0; x < metals.Count; ++x) {
                if (metals[x].HasJobTargetingThis(JOB_TYPE.HAUL)) {
                    availMetals.Add(metals[x]);
                }
            }
            return availMetals;
        }

        public List<TileObject> GetStonelTypeResourcePiles() {
            List<TileObject> stones = connectedCave.GetTileObjectsOfType(TILE_OBJECT_TYPE.ORE);
            List<TileObject> availStone = connectedCave.GetTileObjectsOfType(TILE_OBJECT_TYPE.ORE);
            for (int x = 0; x < stones.Count; ++x) {
                if (stones[x].HasJobTargetingThis(JOB_TYPE.HAUL)) {
                    availStone.Add(stones[x]);
                }
            }
            return availStone;
        }
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