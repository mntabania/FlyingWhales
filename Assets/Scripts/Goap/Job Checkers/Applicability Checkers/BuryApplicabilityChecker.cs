using UnityEngine.Assertions;
namespace Goap.Job_Checkers {
    public class BuryApplicabilityChecker : JobApplicabilityChecker {
        public override string key => JobManager.Bury_Applicability;
        public override bool IsJobStillApplicable(JobQueueItem job) {
            GoapPlanJob goapPlanJob = job as GoapPlanJob;
            Assert.IsNotNull(goapPlanJob);
            Character target = goapPlanJob.targetPOI as Character;
            Assert.IsNotNull(target);

            return target.gridTileLocation != null && target.hasMarker;
        }
    }
}