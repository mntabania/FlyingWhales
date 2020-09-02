using UnityEngine.Assertions;
namespace Goap.Job_Checkers {
    public class CanTakeJoinPartyJob : CanTakeJobChecker {
        
        public override string key => JobManager.Can_Take_Join_Party;
        
        public override bool CanTakeJob(Character character, JobQueueItem jobQueueItem) {
            GoapPlanJob goapPlanJob = jobQueueItem as GoapPlanJob;
            Assert.IsNotNull(goapPlanJob);
            Character targetCharacter = goapPlanJob.targetPOI as Character;
            Assert.IsNotNull(targetCharacter);
            Party partyToJoin = targetCharacter.partyComponent.currentParty;
            return !character.partyComponent.hasParty && partyToJoin != null && !partyToJoin.isWaitTimeOver && !partyToJoin.isDisbanded && partyToJoin.IsAllowedToJoin(character);
        }
    }
}