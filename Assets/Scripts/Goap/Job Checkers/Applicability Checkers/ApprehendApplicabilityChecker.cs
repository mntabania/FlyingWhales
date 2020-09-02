using UnityEngine.Assertions;
namespace Goap.Job_Checkers {
    public class ApprehendApplicabilityChecker : JobApplicabilityChecker {
        public override string key => JobManager.Apprehend_Applicability;
        public override bool IsJobStillApplicable(JobQueueItem job) {
            GoapPlanJob goapPlanJob = job as GoapPlanJob;
            Assert.IsNotNull(goapPlanJob);
            Character target = goapPlanJob.targetPOI as Character;
            Assert.IsNotNull(target);
            NPCSettlement settlement = job.originalOwner as NPCSettlement;
            Assert.IsNotNull(settlement);
            
            bool isApplicable = !target.traitContainer.HasTrait("Restrained") || target.currentStructure != settlement.prison;
            return target.gridTileLocation != null && target.gridTileLocation.IsNextToSettlementAreaOrPartOfSettlement(settlement) && isApplicable;
        }
    }
}