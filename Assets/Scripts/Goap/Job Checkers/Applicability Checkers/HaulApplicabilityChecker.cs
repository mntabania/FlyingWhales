using UnityEngine.Assertions;
namespace Goap.Job_Checkers {
    public class HaulApplicabilityChecker : JobApplicabilityChecker {
        public override string key => JobManager.Haul_Applicability;
        public override bool IsJobStillApplicable(JobQueueItem job) {
            GoapPlanJob goapPlanJob = job as GoapPlanJob;
            Assert.IsNotNull(goapPlanJob);
            NPCSettlement settlement = job.originalOwner as NPCSettlement;
            Assert.IsNotNull(settlement);
            
            return goapPlanJob.targetPOI.isBeingCarriedBy != null || (goapPlanJob.targetPOI.gridTileLocation != null && goapPlanJob.targetPOI.gridTileLocation.structure != settlement.mainStorage);
        }
    }
}