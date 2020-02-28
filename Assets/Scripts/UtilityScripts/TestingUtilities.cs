using Boo.Lang;
namespace UtilityScripts {
    public static class TestingUtilities {
        
        public static void ShowLocationInfo(Region region) {
            List<Settlement> settlements = GetSettlementsInRegion(region);
            string summary = "Locations Job Queue";
            for (int i = 0; i < settlements.Count; i++) {
                Settlement settlement = settlements[i];
                summary += $"\n{settlement.name} Location Job Queue: ";
                if (settlement.availableJobs.Count > 0) {
                    for (int j = 0; j < settlement.availableJobs.Count; j++) {
                        JobQueueItem jqi = settlement.availableJobs[j];
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
                if (settlement.owner != null && settlement.owner.activeQuest != null) {
                    summary += settlement.owner.activeQuest.name;
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

        private static List<Settlement> GetSettlementsInRegion(Region region) {
            List<Settlement> settlements = new List<Settlement>();
            for (int i = 0; i < LandmarkManager.Instance.allSetttlements.Count; i++) {
                Settlement settlement = LandmarkManager.Instance.allSetttlements[i];
                if (settlement.region == region) {
                    settlements.Add(settlement);
                }
            }
            return settlements;
        }
    }
}