using UnityEngine.Assertions;
namespace Goap.Job_Checkers {
    public class RestrainApplicabilityChecker : JobApplicabilityChecker {
        public override string key => JobManager.Restrain_Applicability;
        public override bool IsJobStillApplicable(JobQueueItem job) {
            GoapPlanJob goapPlanJob = job as GoapPlanJob;
            Assert.IsNotNull(goapPlanJob);
            Character target = goapPlanJob.targetPOI as Character;
            Assert.IsNotNull(target);
            NPCSettlement settlement = job.originalOwner as NPCSettlement;
            Assert.IsNotNull(settlement);
            
            bool isApplicable = !target.traitContainer.HasTrait("Restrained"); //|| target.currentStructure != _owner.prison; //Removed check for structure must be in prison because restrain job only puts restrains on the target they dont actually carry them to the prison
            if (target.gridTileLocation != null && isApplicable) {
                if (target.gridTileLocation.IsNextToSettlementAreaOrPartOfSettlement(settlement)) {
                    //if target is within settlement job is always valid
                    return true;
                } else {
                    //if target is no longer within settlement then check if job is already taken
                    if (job.assignedCharacter != null) {
                        //if job is taken, check if assigned character is in actual combat with the target (aka. is already fighting target and not just pursuing)
                        return job.assignedCharacter.combatComponent.IsInActualCombatWith(target);
                    } else {
                        //if job is not yet taken, then it is invalid.
                        return false;
                    }
                    // return job.assignedCharacter != null;
                }
                // return target.gridTileLocation != null && target.gridTileLocation.IsNextToSettlementAreaOrPartOfSettlement(_owner) && isApplicable;    
            }
            return false;
        }
    }
}