namespace Goap.Job_Checkers {
    public class CanTakeCounterattackJob : CanTakeJobChecker {
        public override string key => JobManager.Can_Take_Counterattack;
        public override bool CanTakeJob(Character character, JobQueueItem jobQueueItem) {
            Party partyToJoin = character.partyComponent.currentParty;
            return !character.partyComponent.hasParty;
        }
    }
}