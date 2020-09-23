using Boo.Lang;
namespace UtilityScripts {
    public static class TestingUtilities {
        
        public static void ShowLocationInfo(Region region) {
            string summary = $"{region.name} Info:";
            List<NPCSettlement> settlements = GetSettlementsInRegion(region);
            summary += $"\n-----------------------------";
            summary += "\nLocations Info:";
            for (int i = 0; i < settlements.Count; i++) {
                NPCSettlement npcSettlement = settlements[i];
                summary += $"\n<b>{npcSettlement.name}</b>";
                summary += $"\nStorage: {npcSettlement.mainStorage?.name ?? "None"}. Prison: {npcSettlement.prison?.name ?? "None"}";
                if (npcSettlement.settlementType != null) {
                    summary += $"\n<b>Facility Weights and Caps:</b>";
                    foreach (var kvp in npcSettlement.settlementType.facilityWeights.dictionary) {
                        summary += $"\n\t{kvp.Key.ToString()} - {kvp.Value.ToString()} - {npcSettlement.settlementType.facilityCaps[kvp.Key].ToString()}";
                    }
                }
                if (npcSettlement.owner == null) { continue; }
                summary += $"\n{npcSettlement.name} Location Job Queue:";
                if (npcSettlement.availableJobs.Count > 0) {
                    for (int j = 0; j < npcSettlement.availableJobs.Count; j++) {
                        JobQueueItem jqi = npcSettlement.availableJobs[j];
                        if (jqi is GoapPlanJob) {
                            GoapPlanJob gpj = jqi as GoapPlanJob;
                            summary += $"\n<b>{gpj.name} Targeting {gpj.targetPOI?.ToString() ?? "None"}</b>" ;
                        } else {
                            summary += $"\n<b>{jqi.name}</b>";
                        }
                        summary += $"\n Assigned Character: {jqi.assignedCharacter?.name}";
                    }
                } else {
                    summary += "\nNone";
                }
                summary += $"\n{npcSettlement.name} Party Quests:";
                if (npcSettlement.availablePartyQuests.Count > 0) {
                    for (int j = 0; j < npcSettlement.availablePartyQuests.Count; j++) {
                        PartyQuest quest = npcSettlement.availablePartyQuests[j];
                        summary += $"\n<b>{quest.partyQuestType.ToString()}</b>";
                        summary += $"(Assigned Party: {quest.assignedParty?.partyName})";
                    }
                } else {
                    summary += "\nNone";
                }
                if (npcSettlement.owner != null) {
                    summary += $"\n-----------------------------";
                    summary += $"\n{npcSettlement.owner.name} Faction Job Queue:";
                    if (npcSettlement.owner.availableJobs.Count > 0) {
                        for (int j = 0; j < npcSettlement.owner.availableJobs.Count; j++) {
                            JobQueueItem jqi = npcSettlement.owner.availableJobs[j];
                            if (jqi is GoapPlanJob) {
                                GoapPlanJob gpj = jqi as GoapPlanJob;
                                summary += $"\n<b>{gpj.name} Targeting {gpj.targetPOI?.ToString() ?? "None"}</b>";
                            } else {
                                summary += $"\n<b>{jqi.name}</b>";
                            }
                            summary += $"\n Assigned Character: {jqi.assignedCharacter?.name}";
                        }
                    } else {
                        summary += "\nNone";
                    }
                }
                summary += "\n";
                UIManager.Instance.ShowSmallInfo(summary);
            }
        }
        public static void HideLocationInfo() {
            UIManager.Instance.HideSmallInfo();
        }

        private static List<NPCSettlement> GetSettlementsInRegion(Region region) {
            List<NPCSettlement> settlements = new List<NPCSettlement>();
            for (int i = 0; i < LandmarkManager.Instance.allNonPlayerSettlements.Count; i++) {
                NPCSettlement npcSettlement = LandmarkManager.Instance.allNonPlayerSettlements[i];
                if (npcSettlement.HasTileInRegion(region)) {
                    settlements.Add(npcSettlement);
                }
            }
            return settlements;
        }
    }
}