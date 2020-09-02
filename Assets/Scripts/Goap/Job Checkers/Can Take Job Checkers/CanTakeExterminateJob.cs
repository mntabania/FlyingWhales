namespace Goap.Job_Checkers {
    public class CanTakeExterminateJob : CanTakeJobChecker {
        public override string key => JobManager.Can_Take_Exterminate;
        public override bool CanTakeJob(Character character, JobQueueItem jobQueueItem) {
            Party partyToJoin = character.partyComponent.currentParty;
            return !character.partyComponent.hasParty;
        }
    }
}