using System;
using System.Collections.Generic;
using Locations.Settlements;
namespace Databases {
    public class SettlementDatabase {
        public Dictionary<string, BaseSettlement> settlementsByGUID { get; }
        public List<BaseSettlement> allSettlements { get; }
        public List<NPCSettlement> allNonPlayerSettlements { get; }

        public SettlementDatabase() {
            settlementsByGUID = new Dictionary<string, BaseSettlement>();
            allSettlements = new List<BaseSettlement>();
            allNonPlayerSettlements = new List<NPCSettlement>();
        }
        
        public void RegisterSettlement(BaseSettlement baseSettlement) {
            settlementsByGUID.Add(baseSettlement.persistentID, baseSettlement);
            allSettlements.Add(baseSettlement);
            if (baseSettlement is NPCSettlement npcSettlement) {
                allNonPlayerSettlements.Add(npcSettlement);
            }
        }
        public BaseSettlement GetSettlementByID(int id) {
            for (int i = 0; i < allSettlements.Count; i++) {
                BaseSettlement settlement = allSettlements[i];
                if (settlement.id == id) {
                    return settlement;
                }
            }
            return null;
        }
        public BaseSettlement GetSettlementByPersistentID(string id) {
            if (settlementsByGUID.ContainsKey(id)) {
                return settlementsByGUID[id];
            }
            throw new Exception($"There is no settlement with persistent ID {id}");
        }
        public BaseSettlement GetSettlementByName(string name) {
            for (int i = 0; i < allSettlements.Count; i++) {
                BaseSettlement settlement = allSettlements[i];
                if (settlement.name.Equals(name)) {
                    return settlement;
                }
            }
            return null;
        }
    }
}