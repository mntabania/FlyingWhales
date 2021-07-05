namespace Goap.Job_Checkers {
    public class CanTakeSnatchJob : CanTakeJobChecker {
        
        public override string key => JobManager.Can_Take_Snatch_Job;
        public override bool CanTakeJob(Character character, JobQueueItem jobQueueItem) {
            //If target of snatch does not have a grid tile location (meaning, he is not on the ground), do not snatch
            return !jobQueueItem.poiTarget.isBeingSeized && jobQueueItem.poiTarget.isBeingCarriedBy == null;
        }
    }
}