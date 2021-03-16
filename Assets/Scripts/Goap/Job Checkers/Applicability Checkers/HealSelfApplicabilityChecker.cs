using UnityEngine.Assertions;
namespace Goap.Job_Checkers {
    public class HealSelfApplicabilityChecker : JobApplicabilityChecker {
        public override string key => JobManager.Heal_Self_Applicability;
        public override bool IsJobStillApplicable(JobQueueItem job) {
            GoapPlanJob goapPlanJob = job as GoapPlanJob;
            Assert.IsNotNull(goapPlanJob);
            Character target = goapPlanJob.targetPOI as Character;
            Assert.IsNotNull(target);
            return !target.IsHealthFull();
        }
    }
}