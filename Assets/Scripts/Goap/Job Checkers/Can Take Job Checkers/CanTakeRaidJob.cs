namespace Goap.Job_Checkers {
    public class CanTakeRaidJob : CanTakeJobChecker {
        public override string key => JobManager.Can_Take_Raid;
        public override bool CanTakeJob(Character character, JobQueueItem jobQueueItem) {
            return !character.partyComponent.hasParty;
        }
    }
}