using Inner_Maps.Location_Structures;
namespace Locations.Settlements.Settlement_Types {
    public class CultTown : SettlementType {
        public CultTown() : base(SETTLEMENT_TYPE.Cult_Town) {
            maxDwellings = 8;
            maxFacilities = 5;
        }
        public CultTown(SaveDataSettlementType saveData) : base(saveData) {
            maxDwellings = 8;
            maxFacilities = 5;
        }
        public override void ApplyDefaultSettings() {
            SetInitialFacilityWeightAndCap(new StructureSetting(STRUCTURE_TYPE.FARM, RESOURCE.STONE), 50, 1);
            SetInitialFacilityWeightAndCap(new StructureSetting(STRUCTURE_TYPE.TAVERN, RESOURCE.STONE), 30, 1);
            SetInitialFacilityWeightAndCap(new StructureSetting(STRUCTURE_TYPE.PRISON, RESOURCE.STONE), 10, 1);
            SetInitialFacilityWeightAndCap(new StructureSetting(STRUCTURE_TYPE.BARRACKS, RESOURCE.STONE), 20, 1);
            SetInitialFacilityWeightAndCap(new StructureSetting(STRUCTURE_TYPE.LUMBERYARD, RESOURCE.STONE), 30, 1);
            SetInitialFacilityWeightAndCap(new StructureSetting(STRUCTURE_TYPE.TEMPLE, RESOURCE.STONE), 40, 1);
        }
    }
}