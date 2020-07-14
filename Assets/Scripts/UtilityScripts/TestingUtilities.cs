using Boo.Lang;
namespace UtilityScripts {
    public static class TestingUtilities {
        
        public static void ShowLocationInfo(Region region) {
            string summary = $"{region.name} Info:";
            summary += $"\nActive burning sources {region.innerMap.activeBurningSources.Count.ToString()}";

            // for (int i = 0; i < region.innerMap.activeBurningSources.Count; i++) {
            //     BurningSource source = region.innerMap.activeBurningSources[i];
            //     summary += $"\n{source}: ";
            //     for (int j = 0; j < source.objectsOnFire.Count; j++) {
            //         summary += $"\n\t{source.objectsOnFire[j]}";    
            //     }
            // }
            
            List<NPCSettlement> settlements = GetSettlementsInRegion(region);
            summary += $"\n-----------------------------";
            summary += "\nLocations Info:";
            for (int i = 0; i < settlements.Count; i++) {
                NPCSettlement npcSettlement = settlements[i];
                if (npcSettlement.owner == null) { continue; }
                summary += $"\n{npcSettlement.name}";
                // summary += $"\nDryers: {npcSettlement.settlementJobTriggerComponent.tileDryers.Count.ToString()}";
                // summary += $"\nCleansers: {npcSettlement.settlementJobTriggerComponent.poisonCleansers.Count.ToString()}";
                // summary += $"\nDousers: {npcSettlement.settlementJobTriggerComponent.dousers.Count.ToString()}";
                // for (int j = 0; j < npcSettlement.settlementJobTriggerComponent.dousers.Count; j++) {
                //     Character douser = npcSettlement.settlementJobTriggerComponent.dousers[j];
                //     summary += $"\n\t-{douser.name}";    
                // }
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
                if(npcSettlement.owner != null) {
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