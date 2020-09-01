using UnityEngine.Assertions;
namespace Goap.Job_Checkers {
    public class RepairApplicabilityChecker : JobApplicabilityChecker {
        public override string key => JobManager.Repair_Applicability;
        public override bool IsJobStillApplicable(JobQueueItem job) {
            GoapPlanJob goapPlanJob = job as GoapPlanJob;
            Assert.IsNotNull(goapPlanJob);
            NPCSettlement settlement = job.originalOwner as NPCSettlement;
            Assert.IsNotNull(settlement);
            
            return goapPlanJob.targetPOI.currentHP < goapPlanJob.targetPOI.maxHP && goapPlanJob.targetPOI.gridTileLocation != null && goapPlanJob.targetPOI.gridTileLocation.IsPartOfSettlement(settlement);
        }
    }
}