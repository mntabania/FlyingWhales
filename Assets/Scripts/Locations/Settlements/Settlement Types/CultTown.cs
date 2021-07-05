using Inner_Maps.Location_Structures;
namespace Locations.Settlements.Settlement_Types {
    public class CultTown : SettlementType {
        public CultTown() : base(SETTLEMENT_TYPE.Cult_Town) {
            maxDwellings = 10;
            maxFacilities = 5;
        }
        public CultTown(SaveDataSettlementType saveData) : base(saveData) {
            maxDwellings = 110;
            maxFacilities = 5;
        }
        public override void ApplyDefaultSettings() {
            SetInitialFacilityWeightAndCap(new StructureSetting(STRUCTURE_TYPE.LUMBERYARD, RESOURCE.NONE, true), 300, 1);
            SetInitialFacilityWeightAndCap(new StructureSetting(STRUCTURE_TYPE.CULT_TEMPLE, RESOURCE.STONE, true), 80, 1);
            SetInitialFacilityWeightAndCap(new StructureSetting(STRUCTURE_TYPE.FARM, RESOURCE.NONE, true), 50, 1);
            SetInitialFacilityWeightAndCap(new StructureSetting(STRUCTURE_TYPE.TAVERN, RESOURCE.STONE, true), 30, 1);
            SetInitialFacilityWeightAndCap(new StructureSetting(STRUCTURE_TYPE.PRISON, RESOURCE.STONE, true), 10, 1);
            SetInitialFacilityWeightAndCap(new StructureSetting(STRUCTURE_TYPE.BARRACKS, RESOURCE.STONE, true), 20, 1);
            SetInitialFacilityWeightAndCap(new StructureSetting(STRUCTURE_TYPE.HOSPICE, RESOURCE.STONE, true), 10, 1);
        }
        public override StructureSetting GetDwellingSetting(Faction faction) {
            return new StructureSetting(STRUCTURE_TYPE.DWELLING, RESOURCE.STONE, true);
        }
    }
}