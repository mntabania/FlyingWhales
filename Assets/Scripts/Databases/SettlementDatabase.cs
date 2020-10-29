using System;
using System.Collections.Generic;
using Locations.Settlements;
namespace Databases {
    public class SettlementDatabase {
        public Dictionary<string, BaseSettlement> settlementsByGUID { get; }
        /// <summary>
        /// NOTE: This can contain settlements that no longer have areas. Since we cannot remove them since some classes might still need them,
        /// like raid parties and other party quests
        /// </summary>
        public List<BaseSettlement> allSettlements { get; }
        /// <summary>
        /// NOTE: This can contain settlements that no longer have areas. Since we cannot remove them since some classes might still need them,
        /// like raid parties and other party quests
        /// </summary>
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
        public void UnRegisterSettlement(BaseSettlement baseSettlement) {
            settlementsByGUID.Remove(baseSettlement.persistentID);
            allSettlements.Remove(baseSettlement);
            if (baseSettlement is NPCSettlement npcSettlement) {
                allNonPlayerSettlements.Remove(npcSettlement);
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
        public BaseSettlement GetSettlementByPersistentIDSafe(string id) {
            if (settlementsByGUID.ContainsKey(id)) {
                return settlementsByGUID[id];
            }
            return null;
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