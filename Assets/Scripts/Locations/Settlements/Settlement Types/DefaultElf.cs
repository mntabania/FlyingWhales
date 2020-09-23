using Inner_Maps.Location_Structures;
namespace Locations.Settlements.Settlement_Types {
    public class DefaultElf : SettlementType {
        public DefaultElf() : base(SETTLEMENT_TYPE.Default_Elf) {
            maxDwellings = 8;
            maxFacilities = 6;
        }
        public DefaultElf(SaveDataSettlementType saveData) : base(saveData) { }
        public override void ApplyDefaultSettings() {
            SetInitialFacilityWeightAndCap(new StructureSetting(STRUCTURE_TYPE.FARM, RESOURCE.WOOD), 50, 1);
            SetInitialFacilityWeightAndCap(new StructureSetting(STRUCTURE_TYPE.TAVERN, RESOURCE.WOOD), 30, 1);
            SetInitialFacilityWeightAndCap(new StructureSetting(STRUCTURE_TYPE.CEMETERY, RESOURCE.WOOD), 10, 1);
            SetInitialFacilityWeightAndCap(new StructureSetting(STRUCTURE_TYPE.PRISON, RESOURCE.WOOD), 10, 1);
            SetInitialFacilityWeightAndCap(new StructureSetting(STRUCTURE_TYPE.BARRACKS, RESOURCE.WOOD), 20, 1);
        }
    }
}