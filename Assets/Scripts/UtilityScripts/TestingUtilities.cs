using Boo.Lang;
namespace UtilityScripts {
    public static class TestingUtilities {
        
        public static void ShowLocationInfo(Region region) {
            string summary = $"{region.name} Info:";
            summary += $"\nActive burning sources {region.innerMap.activeBurningSources.Count.ToString()}";

            for (int i = 0; i < region.innerMap.activeBurningSources.Count; i++) {
                BurningSource source = region.innerMap.activeBurningSources[i];
                summary += $"\n{source}: ";
                for (int j = 0; j < source.objectsOnFire.Count; j++) {
                    summary += $"\n\t{source.objectsOnFire[j]}";    
                }
            }
            
            List<NPCSettlement> settlements = GetSettlementsInRegion(region);
            summary += $"\n-----------------------------";
            summary += "\nLocations Job Queue";
            for (int i = 0; i < settlements.Count; i++) {
                NPCSettlement npcSettlement = settlements[i];
                summary += $"\n{npcSettlement.name} Location Job Queue: ";
                if (npcSettlement.availableJobs.Count > 0) {
                    for (int j = 0; j < npcSettlement.availableJobs.Count; j++) {
                        JobQueueItem jqi = npcSettlement.availableJobs[j];
                        if (jqi is GoapPlanJob) {
                            GoapPlanJob gpj = jqi as GoapPlanJob;
                            summary += $"\n{gpj.name} Targeting {gpj.targetPOI}" ?? "None";
                        } else {
                            summary += $"\n{jqi.name}";
                        }
                        summary += $"\nAssigned Character: {jqi.assignedCharacter?.name}" ?? "None";
                        if (UIManager.Instance.characterInfoUI.isShowing) {
                            summary +=
                                $"\nCan character take job? {jqi.CanCharacterDoJob(UIManager.Instance.characterInfoUI.activeCharacter)}";
                        }
            
                    }
                } else {
                    summary += "\nNone";
                }
                summary += "\nActive Quest: ";
                if (npcSettlement.owner != null && npcSettlement.owner.activeFactionQuest != null) {
                    summary += npcSettlement.owner.activeFactionQuest.name;
                } else {
                    summary += "None";
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