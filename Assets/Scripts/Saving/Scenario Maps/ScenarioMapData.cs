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
        public ScenarioWorldMapSave worldMapSave;
        public SettlementTemplate[] villageSettlementTemplates;

        public void SaveVillageSettlements(List<NPCSettlement> villageSettlements) {
            villageSettlementTemplates = new SettlementTemplate[villageSettlements.Count];
            for (int i = 0; i < villageSettlements.Count; i++) {
                NPCSettlement settlement = villageSettlements[i];
                
                //save hex tiles
                Point[] tileCoordinates = new Point[settlement.areas.Count];
                for (int j = 0; j < tileCoordinates.Length; j++) {
                    Area area = settlement.areas[j];
                    tileCoordinates[j] = new Point(area.areaData.xCoordinate, area.areaData.yCoordinate);
                }
                
                //save structure settings
                List<StructureSetting> structureSettings = new List<StructureSetting>();
                for (int j = 0; j < settlement.allStructures.Count; j++) {
                    LocationStructure structure = settlement.allStructures[j];
                    if (structure.structureType.IsVillageStructure() && structure.structureType != STRUCTURE_TYPE.DWELLING && structure is ManMadeStructure manMadeStructure) { 
                        //Ignored dwellings because in the templates, dwellings are only part of the more important structures i.e. (Farms, City Center, etc.)
                        StructureSetting structureSetting = new StructureSetting(structure.structureType, manMadeStructure.wallsAreMadeOf);
                        structureSettings.Add(structureSetting);
                    }
                }
                villageSettlementTemplates[i] = new SettlementTemplate(tileCoordinates, structureSettings.ToArray(), 0, settlement.owner.race, settlement.settlementType.settlementType);
            }
        }
        
    }

    [System.Serializable]
    public struct SettlementTemplate {
        public Point[] areas;
        public StructureSetting[] structureSettings;
        public int minimumVillagerCount;
        public RACE factionRace;
        public SETTLEMENT_TYPE settlementType;
        
        public SettlementTemplate(Point[] areas, StructureSetting[] structureSettings, int minimumVillagerCount, RACE factionRace, SETTLEMENT_TYPE settlementType) {
            this.areas = areas;
            this.structureSettings = structureSettings;
            this.minimumVillagerCount = minimumVillagerCount;
            this.factionRace = factionRace;
            this.settlementType = settlementType;
        }
        
        public Area[] GetTilesInTemplate(Area[,] map) {
            Area[] tiles = new Area[areas.Length];
            for (int i = 0; i < tiles.Length; i++) {
                Point point = areas[i];
                tiles[i] = map[point.X, point.Y];
            }
            return tiles;
        }
    }
}