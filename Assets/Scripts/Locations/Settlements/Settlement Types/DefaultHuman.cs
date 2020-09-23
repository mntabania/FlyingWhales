using Inner_Maps.Location_Structures;
namespace Locations.Settlements.Settlement_Types {
    public class DefaultHuman : SettlementType {
        public DefaultHuman() : base(SETTLEMENT_TYPE.Default_Human) {
            maxDwellings = 8;
            maxFacilities = 6;
        }
        public DefaultHuman(SaveDataSettlementType saveData) : base(saveData) { }
        public override void ApplyDefaultSettings() {
            SetInitialFacilityWeightAndCap(new StructureSetting(STRUCTURE_TYPE.FARM, RESOURCE.STONE), 50, 1);
            SetInitialFacilityWeightAndCap(new StructureSetting(STRUCTURE_TYPE.TAVERN, RESOURCE.STONE), 30, 1);
            SetInitialFacilityWeightAndCap(new StructureSetting(STRUCTURE_TYPE.CEMETERY, RESOURCE.STONE), 10, 1);
            SetInitialFacilityWeightAndCap(new StructureSetting(STRUCTURE_TYPE.PRISON, RESOURCE.STONE), 10, 1);
            SetInitialFacilityWeightAndCap(new StructureSetting(STRUCTURE_TYPE.BARRACKS, RESOURCE.STONE), 20, 1);
        }
    }
}