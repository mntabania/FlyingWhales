namespace Goap.Job_Checkers {
    public class CanTakeBuryJob : CanTakeJobChecker {
        
        public override string key => JobManager.Can_Take_Bury_Job;
        public override bool CanTakeJob(Character character, JobQueueItem jobQueueItem) {
            if (!character.traitContainer.HasTrait("Criminal") && character.isAtHomeRegion && character.isPartOfHomeFaction && character.race.IsSapient()) { //!character.traitContainer.HasTrait("Beast")
                return true;
            }
            return false;
        }
    }
}