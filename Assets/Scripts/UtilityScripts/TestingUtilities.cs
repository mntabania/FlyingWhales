using Boo.Lang;
namespace UtilityScripts {
    public static class TestingUtilities {
        
        public static void ShowLocationInfo(Region region) {
            List<NPCSettlement> settlements = GetSettlementsInRegion(region);
            string summary = "Locations Job Queue";
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
                if (npcSettlement.owner != null && npcSettlement.owner.activeQuest != null) {
                    summary += npcSettlement.owner.activeQuest.name;
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