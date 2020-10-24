namespace Goap.Job_Checkers {
    public class CanStealCorpse : CanTakeJobChecker {
        public override string key => JobManager.Can_Steal_Corpse;
        public override bool CanTakeJob(Character character, JobQueueItem jobQueueItem) {
            return character.traitContainer.HasTrait("Cultist");
        }
    }
}