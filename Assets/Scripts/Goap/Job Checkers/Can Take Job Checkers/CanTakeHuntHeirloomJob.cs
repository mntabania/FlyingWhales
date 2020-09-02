namespace Goap.Job_Checkers {
    public class CanTakeHuntHeirloomJob : CanTakeJobChecker {
        public override string key => JobManager.Can_Take_Hunt_Heirloom;
        public override bool CanTakeJob(Character character, JobQueueItem jobQueueItem) {
            Party partyToJoin = character.partyComponent.currentParty;
            return !character.partyComponent.hasParty;
        }
    }
}