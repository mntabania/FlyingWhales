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
            
            if (target.race.IsSkinnable() ) {
                //if race is skinnable and settlement has an assigned skinners lodge, do not bury this character
                //Reference: https://trello.com/c/VEBnv6Aw/4771-bury-updates
                if (npcSettlement.HasStructureOfTypeThatIsAssigned(STRUCTURE_TYPE.HUNTER_LODGE)) {
                    return false;
                }
            }
            
            if (!npcSettlement.HasStructure(STRUCTURE_TYPE.CEMETERY) && target.previousCharacterDataComponent.homeSettlementOnDeath != npcSettlement) {
                //if settlement doesn't have a cemetery and character to be buried was not a resident of the settlement, do not bury it.
                //Reference: https://trello.com/c/VEBnv6Aw/4771-bury-updates
                return false;
            }
            
            
            return target.gridTileLocation != null && target.gridTileLocation.IsNextToOrPartOfSettlement(npcSettlement) && target.hasMarker;
        }
    }
}