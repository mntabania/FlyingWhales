using System;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UnityEngine.Assertions;
namespace Inner_Maps.Location_Structures {
    public class MineShack : ManMadeStructure {
        
        public Cave connectedCave { get; private set; }
        public override Type serializedData => typeof(SaveDataMineShack);
        public MineShack(Region location) : base(STRUCTURE_TYPE.MINE_SHACK, location) {
            SetMaxHPAndReset(4000);
        }
        public MineShack(Region location, SaveDataManMadeStructure data) : base(location, data) {
            SetMaxHP(4000);
        }

        #region Loading
        public override void LoadReferences(SaveDataLocationStructure saveDataLocationStructure) {
            base.LoadReferences(saveDataLocationStructure);
            SaveDataMineShack saveDataMineShack = saveDataLocationStructure as SaveDataMineShack;
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
            
            //Create a path inside
            HexTile hexTile = p_usedConnector.collectionOwner.partOfHextile.hexTileOwner;
            LocationGridTile centerTile = hexTile.GetCenterLocationGridTile();
            List<LocationGridTile> path = PathGenerator.Instance.GetPath(p_usedConnector, centerTile, GRID_PATHFINDING_MODE.UNCONSTRAINED, includeFirstTile: true);
            if (path != null) {
                for (int i = 0; i < path.Count; i++) {
                    LocationGridTile pathTile = path[i];
                    if (pathTile.objHere is BlockWall || pathTile.objHere is OreVein) {
                        pathTile.structure.RemovePOI(pathTile.objHere);
                    }		
                }
            }
        }
        protected override void DestroyStructure() {
            base.DestroyStructure();
            connectedCave = null;
        }
    }
}

#region Save Data
public class SaveDataMineShack : SaveDataManMadeStructure {

    public string connectedCaveID;
    
    public override void Save(LocationStructure locationStructure) {
        base.Save(locationStructure);
        MineShack mineShack = locationStructure as MineShack;
        if (mineShack.connectedCave != null) {
            connectedCaveID = mineShack.connectedCave.persistentID;
        }
    }
}
#endregion