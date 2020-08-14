using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
namespace Scenario_Maps {
    /// <summary>
    /// Almost the same as <see cref="SaveDataCurrentProgress"/>. But only contains
    /// necessary data for scenario maps such as Geography.
    /// </summary>
    [System.Serializable]
    public class ScenarioMapData {
        public WorldMapSave worldMapSave;
        public SettlementTemplate[] villageSettlementTemplates;

        public void SaveVillageSettlements(List<NPCSettlement> villageSettlements) {
            villageSettlementTemplates = new SettlementTemplate[villageSettlements.Count];
            for (int i = 0; i < villageSettlements.Count; i++) {
                NPCSettlement settlement = villageSettlements[i];
                
                //save hex tiles
                Point[] tileCoordinates = new Point[settlement.tiles.Count];
                for (int j = 0; j < tileCoordinates.Length; j++) {
                    HexTile hexTile = settlement.tiles[j];
                    tileCoordinates[j] = new Point(hexTile.xCoordinate, hexTile.yCoordinate);
                }
                
                //save structure settings
                List<StructureSetting> structureSettings = new List<StructureSetting>();
                for (int j = 0; j < settlement.allStructures.Count; j++) {
                    LocationStructure structure = settlement.allStructures[j];
                    if (structure.structureType.IsSettlementStructure() && structure.structureType != STRUCTURE_TYPE.DWELLING) { 
                        //Ignored dwellings because in the templates, dwellings are only part of the more important structures i.e. (Farms, City Center, etc.)
                        StructureSetting structureSetting = new StructureSetting(structure.structureType, structure.structureObj.thinWallResource);
                        structureSettings.Add(structureSetting);
                    }
                }
                villageSettlementTemplates[i] = new SettlementTemplate(tileCoordinates, structureSettings.ToArray(), 0, settlement.owner.race);
            }
        }
        
    }

    [System.Serializable]
    public struct SettlementTemplate {
        public Point[] hexTiles;
        public StructureSetting[] structureSettings;
        public int minimumVillagerCount;
        public RACE factionRace;
        
        public SettlementTemplate(Point[] hexTiles, StructureSetting[] structureSettings, int minimumVillagerCount, RACE factionRace) {
            this.hexTiles = hexTiles;
            this.structureSettings = structureSettings;
            this.minimumVillagerCount = minimumVillagerCount;
            this.factionRace = factionRace;
        }
        
        public HexTile[] GetTilesInTemplate(HexTile[,] map) {
            HexTile[] tiles = new HexTile[hexTiles.Length];
            for (int i = 0; i < tiles.Length; i++) {
                Point point = hexTiles[i];
                tiles[i] = map[point.X, point.Y];
            }
            return tiles;
        }
    }
}