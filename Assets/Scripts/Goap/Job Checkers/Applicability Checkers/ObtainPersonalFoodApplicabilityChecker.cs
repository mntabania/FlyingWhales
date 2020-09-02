using UnityEngine.Assertions;
namespace Goap.Job_Checkers {
    public class ObtainPersonalFoodApplicabilityChecker : JobApplicabilityChecker {
        public override string key => JobManager.Obtain_Personal_Food_Applicability;
        public override bool IsJobStillApplicable(JobQueueItem job) {
            GoapPlanJob goapPlanJob = job as GoapPlanJob;
            Assert.IsNotNull(goapPlanJob);

            return goapPlanJob.targetPOI.gridTileLocation != null;
        }
    }
}