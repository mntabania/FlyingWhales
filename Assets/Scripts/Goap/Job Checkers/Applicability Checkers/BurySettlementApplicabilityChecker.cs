using UnityEngine.Assertions;
namespace Goap.Job_Checkers {
    public class BurySettlementApplicabilityChecker : JobApplicabilityChecker {
        public override string key => JobManager.Bury_Settlement_Applicability;
        public override bool IsJobStillApplicable(JobQueueItem job) {
            GoapPlanJob goapPlanJob = job as GoapPlanJob;
            Assert.IsNotNull(goapPlanJob);
            Character target = goapPlanJob.targetPOI as Character;
            Assert.IsNotNull(target);
            NPCSettlement npcSettlement = job.originalOwner as NPCSettlement;
            Assert.IsNotNull(npcSettlement);
            
            return target.gridTileLocation != null && target.gridTileLocation.IsNextToOrPartOfSettlement(npcSettlement) && target.marker != null;
        }
    }
}