using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Locations.Settlements.Settlement_Types;
namespace Locations.Settlements.Settlement_Types {
    public abstract class SettlementType {
        public WeightedDictionary<StructureSetting> facilityWeights { get; }
        public Dictionary<StructureSetting, int> facilityCaps { get; }
        public SETTLEMENT_TYPE settlementType { get; }
        public int maxDwellings { get; protected set; }
        public int maxFacilities { get; protected set; }

        public SettlementType(SETTLEMENT_TYPE settlementType) {
            this.settlementType = settlementType;
            facilityWeights = new WeightedDictionary<StructureSetting>();
            facilityCaps = new Dictionary<StructureSetting, int>();
        }
        public SettlementType(SaveDataSettlementType data) {
            settlementType = data.settlementType;
            facilityWeights = new WeightedDictionary<StructureSetting>(data.facilityWeights);
            facilityCaps = new Dictionary<StructureSetting, int>(data.facilityCaps);
        }

        public abstract void ApplyDefaultSettings();

        #region Facilities
        public void SetInitialFacilityWeightAndCap(StructureSetting setting, int weight, int cap) {
            SetFacilityWeight(setting, weight);
            SetFacilityCap(setting, cap);
        }
        public void SetFacilityWeight(StructureSetting setting, int weight) {
            facilityWeights.SetElementWeight(setting, weight);
        }
        public void AddFacilityWeight(StructureSetting setting, int weight) {
            facilityWeights.AddWeightToElement(setting, weight);
        }
        public void SetFacilityCap(StructureSetting setting, int cap) {
            if (!facilityCaps.ContainsKey(setting)) {
                facilityCaps.Add(setting, 0);
            }
            facilityCaps[setting] = cap;
        }
        public int GetFacilityCap(StructureSetting structureSetting) {
            if (facilityCaps.ContainsKey(structureSetting)) {
                return facilityCaps[structureSetting];
            }
            return 0;
        }
        #endregion
    }
}

public class SaveDataSettlementType : SaveData<SettlementType> {
    public Dictionary<StructureSetting, int> facilityWeights;
    public Dictionary<StructureSetting, int> facilityCaps;
    public SETTLEMENT_TYPE settlementType;
    public override void Save(SettlementType data) {
        base.Save(data);
        facilityWeights = data.facilityWeights.dictionary;
        facilityCaps = data.facilityCaps;
        settlementType = data.settlementType;
    }
    public override SettlementType Load() {
        return LandmarkManager.Instance.CreateSettlementType(this);
    }
}