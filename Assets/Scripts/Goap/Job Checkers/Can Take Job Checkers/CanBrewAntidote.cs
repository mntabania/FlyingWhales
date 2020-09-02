namespace Goap.Job_Checkers {
    public class CanBrewAntidote : CanTakeJobChecker {
        public override string key => JobManager.Can_Brew_Antidote;
        public override bool CanTakeJob(Character character, JobQueueItem jobQueueItem) {
            return TILE_OBJECT_TYPE.ANTIDOTE.CanBeCraftedBy(character);
        }
    }
}