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