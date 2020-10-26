using Boo.Lang;
using Locations.Settlements.Settlement_Events;
namespace UtilityScripts {
    public static class TestingUtilities {
        
        public static void ShowLocationInfo(Region region) {
            string summary = $"{region.name} Info:";
            List<NPCSettlement> settlements = GetSettlementsInRegion(region);
            summary += $"\n-----------------------------";
            summary += "\nLocations Info:";
            for (int i = 0; i < settlements.Count; i++) {
                NPCSettlement npcSettlement = settlements[i];
                if (npcSettlement.locationType != LOCATION_TYPE.VILLAGE) {
                    continue;
                }
                summary += $"\n<b>{npcSettlement.name}</b> Settlement Type: {npcSettlement.settlementType?.settlementType.ToString() ?? "None"}";
                summary += $"\nHas Peasants: {npcSettlement.hasPeasants.ToString()}, Has Workers: {npcSettlement.hasWorkers.ToString()}";
                summary += $"\nStorage: {npcSettlement.mainStorage?.name ?? "None"}. Prison: {npcSettlement.prison?.name ?? "None"}";
                // if (npcSettlement.settlementType != null) {
                //     summary += $"\n<b>Max Dwellings: {npcSettlement.settlementType.maxDwellings.ToString()}</b>, <b>Max Facilities: {npcSettlement.settlementType.maxFacilities.ToString()}</b>";
                //     summary += $"\n<b>Facility Weights and Caps:</b>";
                //     foreach (var kvp in npcSettlement.settlementType.facilityWeights.dictionary) {
                //         summary += $"\n\t{kvp.Key.ToString()} - {kvp.Value.ToString()} - {npcSettlement.settlementType.facilityCaps[kvp.Key].ToString()}";
                //     }
                // }
                summary += $"\nNeeded Items: ";
                for (int j = 0; j < npcSettlement.neededObjects.Count; j++) {
                    summary += $"|{npcSettlement.neededObjects[j].ToString()}|";
                }
                summary += $"\nActive Events: ";
                for (int j = 0; j < npcSettlement.eventManager.activeEvents.Count; j++) {
                    SettlementEvent settlementEvent = npcSettlement.eventManager.activeEvents[j];
                    summary += $"|{settlementEvent.eventType.ToString()}|";
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
                    summary += $"\n-----------------------------";
                    summary += $"\n{npcSettlement.owner.name} Party Quests:";
                    if (npcSettlement.owner.partyQuestBoard.availablePartyQuests.Count > 0) {
                        for (int j = 0; j < npcSettlement.owner.partyQuestBoard.availablePartyQuests.Count; j++) {
                            PartyQuest quest = npcSettlement.owner.partyQuestBoard.availablePartyQuests[j];
                            summary += $"\n<b>{quest.partyQuestType.ToString()}</b>";
                            summary += $"(Assigned Party: {quest.assignedParty?.partyName})";
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