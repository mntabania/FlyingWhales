namespace Goap.Job_Checkers {
    public class CanTakeRepairJob : CanTakeJobChecker {
        public override string key => JobManager.Can_Take_Repair;
        public override bool CanTakeJob(Character character, JobQueueItem jobQueueItem) {
            bool canTakeRepairJob = false;
            if(jobQueueItem is GoapPlanJob planJob) {
                if(planJob.targetPOI is TileObject targetTileObject) {
                    canTakeRepairJob = targetTileObject.canBeRepaired;
                }
            }
            return canTakeRepairJob;
        }
    }
}