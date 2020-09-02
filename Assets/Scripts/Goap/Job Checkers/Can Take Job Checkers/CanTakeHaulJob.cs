namespace Goap.Job_Checkers {
    public class CanTakeHaulJob : CanTakeJobChecker {
        public override string key => JobManager.Can_Take_Haul;
        public override bool CanTakeJob(Character character, JobQueueItem jobQueueItem) {
            if (jobQueueItem is GoapPlanJob goapPlanJob && goapPlanJob.targetPOI.gridTileLocation != null) {
                return !character.movementComponent.structuresToAvoid.Contains(goapPlanJob.targetPOI.gridTileLocation.structure);
            }
            return false;
        }
    }
}