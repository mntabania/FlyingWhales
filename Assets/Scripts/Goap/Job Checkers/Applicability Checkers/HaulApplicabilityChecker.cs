using UnityEngine.Assertions;
using Locations.Settlements;
namespace Goap.Job_Checkers {
    public class HaulApplicabilityChecker : JobApplicabilityChecker {
        public override string key => JobManager.Haul_Applicability;
        public override bool IsJobStillApplicable(JobQueueItem job) {
            GoapPlanJob goapPlanJob = job as GoapPlanJob;
            Assert.IsNotNull(goapPlanJob);
            NPCSettlement settlement = job.originalOwner as NPCSettlement;
            Assert.IsNotNull(settlement);

            IPointOfInterest target = goapPlanJob.targetPOI;
            BaseSettlement settlementWhereTargetIsPlaced = null;

            bool cannotBeHauled = target.gridTileLocation == null || (target.gridTileLocation.IsPartOfSettlement(out settlementWhereTargetIsPlaced) && settlementWhereTargetIsPlaced != settlement && settlementWhereTargetIsPlaced.owner != null &&
               (settlementWhereTargetIsPlaced.owner.isMajorNonPlayer || settlementWhereTargetIsPlaced.owner.factionType.type == FACTION_TYPE.Ratmen));


            return !cannotBeHauled && (goapPlanJob.targetPOI.isBeingCarriedBy != null || (goapPlanJob.targetPOI.gridTileLocation != null && goapPlanJob.targetPOI.gridTileLocation.structure != settlement.mainStorage));
        }
    }
}