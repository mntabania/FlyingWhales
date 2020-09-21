using UnityEngine.Assertions;
namespace Goap.Job_Checkers {
    public class CanTakeJoinGathering : CanTakeJobChecker {
        
        public override string key => JobManager.Can_Take_Join_Gathering;
        
        public override bool CanTakeJob(Character character, JobQueueItem jobQueueItem) {
            GoapPlanJob goapPlanJob = jobQueueItem as GoapPlanJob;
            Assert.IsNotNull(goapPlanJob);
            Character targetCharacter = goapPlanJob.targetPOI as Character;
            Assert.IsNotNull(targetCharacter);
            Gathering gatheringToJoin = targetCharacter.gatheringComponent.currentGathering;
            return !character.gatheringComponent.hasGathering && gatheringToJoin != null && !gatheringToJoin.isWaitTimeOver && !gatheringToJoin.isDisbanded && gatheringToJoin.IsAllowedToJoin(character);
        }
    }
}