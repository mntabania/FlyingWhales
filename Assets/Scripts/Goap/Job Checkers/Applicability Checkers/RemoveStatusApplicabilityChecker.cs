using UnityEngine.Assertions;
namespace Goap.Job_Checkers {
    public class RemoveStatusApplicabilityChecker : JobApplicabilityChecker {
        public override string key => JobManager.Remove_Status_Applicability;
        public override bool IsJobStillApplicable(JobQueueItem job) {
            GoapPlanJob goapPlanJob = job as GoapPlanJob;
            Assert.IsNotNull(goapPlanJob);
            Character target = goapPlanJob.targetPOI as Character;
            Assert.IsNotNull(target);
            string traitName = goapPlanJob.goal.conditionKey;
            
            if (target.gridTileLocation == null || target.isDead) {
                return false;
            }
            if (target.gridTileLocation.IsNextToSettlementAreaOrPartOfSettlement(job.originalOwner as NPCSettlement) == false) {
                return false;
            }
            if (target.traitContainer.HasTrait("Criminal")) {
                return false;
            }
            if (!target.traitContainer.HasTrait(traitName)) {
                return false; //target no longer has the given trait
            }
            return true;
        }
    }
}