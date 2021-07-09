using Inner_Maps.Location_Structures;
using UnityEngine.Assertions;
namespace Goap.Job_Checkers {
    public class ApprehendApplicabilityChecker : JobApplicabilityChecker {
        public override string key => JobManager.Apprehend_Applicability;
        public override bool IsJobStillApplicable(JobQueueItem job) {
            GoapPlanJob goapPlanJob = job as GoapPlanJob;
            Assert.IsNotNull(goapPlanJob);
            Character target = goapPlanJob.targetPOI as Character;
            Assert.IsNotNull(target);
            Character actor = goapPlanJob.assignedCharacter;
            Assert.IsNotNull(actor);
            bool isApplicable = !target.traitContainer.HasTrait("Restrained") || !target.IsInPrison();
            return actor != null && actor.homeSettlement != null && target.gridTileLocation != null && target.gridTileLocation.IsNextToSettlementAreaOrPartOfSettlement(actor.homeSettlement) && isApplicable;
        }
    }
}