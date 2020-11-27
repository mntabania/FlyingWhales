﻿using Inner_Maps.Location_Structures;
namespace Locations.Settlements.Settlement_Types {
    public class ElvenHamlet : SettlementType {
        public ElvenHamlet() : base(SETTLEMENT_TYPE.Elven_Hamlet) {
            maxDwellings = 12;
            maxFacilities = 6;
        }
        public ElvenHamlet(SaveDataSettlementType saveData) : base(saveData) { 
            maxDwellings = 12;
            maxFacilities = 6;
        }
        public override void ApplyDefaultSettings() {
            SetInitialFacilityWeightAndCap(new StructureSetting(STRUCTURE_TYPE.FARM, RESOURCE.WOOD), 50, 1);
            SetInitialFacilityWeightAndCap(new StructureSetting(STRUCTURE_TYPE.TAVERN, RESOURCE.WOOD), 30, 1);
            SetInitialFacilityWeightAndCap(new StructureSetting(STRUCTURE_TYPE.CEMETERY, RESOURCE.WOOD), 10, 1);
            SetInitialFacilityWeightAndCap(new StructureSetting(STRUCTURE_TYPE.PRISON, RESOURCE.WOOD), 10, 1);
            SetInitialFacilityWeightAndCap(new StructureSetting(STRUCTURE_TYPE.HOSPICE, RESOURCE.WOOD), 10, 1);
            SetInitialFacilityWeightAndCap(new StructureSetting(STRUCTURE_TYPE.BARRACKS, RESOURCE.WOOD), 20, 1);
            SetInitialFacilityWeightAndCap(new StructureSetting(STRUCTURE_TYPE.LUMBERYARD, RESOURCE.WOOD), 30, 1);
        }
        public override StructureSetting GetDwellingSetting(Faction faction) {
            return new StructureSetting(STRUCTURE_TYPE.DWELLING, RESOURCE.WOOD);
        }
    }
}