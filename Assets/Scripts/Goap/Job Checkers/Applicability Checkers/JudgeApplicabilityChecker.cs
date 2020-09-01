using UnityEngine.Assertions;
namespace Goap.Job_Checkers {
    public class JudgeApplicabilityChecker : JobApplicabilityChecker {
        public override string key => JobManager.Judge_Applicability;
        public override bool IsJobStillApplicable(JobQueueItem job) {
            GoapPlanJob goapPlanJob = job as GoapPlanJob;
            Assert.IsNotNull(goapPlanJob);
            Character criminal = goapPlanJob.targetPOI as Character;
            Assert.IsNotNull(criminal);
            NPCSettlement settlement = job.originalOwner as NPCSettlement;
            Assert.IsNotNull(settlement);
            
            if (criminal.isDead) {
                //Character is dead
                return false;
            }
            if (criminal.currentSettlement is NPCSettlement npcSettlement && 
                criminal.currentStructure != npcSettlement.prison) {
                //Character is no longer in jail
                return false;
            }
            if (!criminal.traitContainer.HasTrait("Restrained")) {
                //Character is no longer restrained
                return false;
            }
            return true;
        }
    }
}