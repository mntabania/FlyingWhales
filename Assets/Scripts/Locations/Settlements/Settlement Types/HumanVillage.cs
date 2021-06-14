using Inner_Maps.Location_Structures;
namespace Locations.Settlements.Settlement_Types {
    public class HumanVillage : SettlementType {
        public HumanVillage() : base(SETTLEMENT_TYPE.Human_Village) {
            maxDwellings = 16;
            maxFacilities = 16; //6
        }
        public HumanVillage(SaveDataSettlementType saveData) : base(saveData) {
            maxDwellings = 16;
            maxFacilities = 16; //6
        }
        public override void ApplyDefaultSettings() {
            SetInitialFacilityWeightAndCap(new StructureSetting(STRUCTURE_TYPE.MINE, RESOURCE.NONE), 300, 1);
            SetInitialFacilityWeightAndCap(new StructureSetting(STRUCTURE_TYPE.TAVERN, RESOURCE.STONE), 30, 1);
            SetInitialFacilityWeightAndCap(new StructureSetting(STRUCTURE_TYPE.FARM, RESOURCE.NONE), 30, 1);
            SetInitialFacilityWeightAndCap(new StructureSetting(STRUCTURE_TYPE.FISHERY, RESOURCE.WOOD), 30, 1);
            SetInitialFacilityWeightAndCap(new StructureSetting(STRUCTURE_TYPE.HUNTER_LODGE, RESOURCE.STONE), 30, 1);
            SetInitialFacilityWeightAndCap(new StructureSetting(STRUCTURE_TYPE.CEMETERY, RESOURCE.STONE), 10, 1);
            SetInitialFacilityWeightAndCap(new StructureSetting(STRUCTURE_TYPE.PRISON, RESOURCE.STONE), 10, 1);
            SetInitialFacilityWeightAndCap(new StructureSetting(STRUCTURE_TYPE.BARRACKS, RESOURCE.STONE), 20, 1);
            SetInitialFacilityWeightAndCap(new StructureSetting(STRUCTURE_TYPE.MAGE_QUARTERS, RESOURCE.STONE), 10, 1);
            SetInitialFacilityWeightAndCap(new StructureSetting(STRUCTURE_TYPE.HOSPICE, RESOURCE.STONE), 10, 1);
            SetInitialFacilityWeightAndCap(new StructureSetting(STRUCTURE_TYPE.QUARRY, RESOURCE.STONE), 10, 1);
            SetInitialFacilityWeightAndCap(new StructureSetting(STRUCTURE_TYPE.LUMBERYARD, RESOURCE.NONE), 10, 1);
            
        }
        public override StructureSetting GetDwellingSetting(Faction faction) {
            return new StructureSetting(STRUCTURE_TYPE.DWELLING, RESOURCE.STONE, faction.factionType.usesCorruptedStructures);
        }
    }
}