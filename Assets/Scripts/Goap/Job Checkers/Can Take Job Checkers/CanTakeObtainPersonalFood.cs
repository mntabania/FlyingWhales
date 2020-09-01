using UnityEngine.Assertions;
namespace Goap.Job_Checkers {
    public class CanTakeObtainPersonalFood : CanTakeJobChecker {
        public override string key => JobManager.Can_Take_Obtain_Personal_Food;
        public override bool CanTakeJob(Character character, JobQueueItem jobQueueItem) {
            GoapPlanJob goapPlanJob = jobQueueItem as GoapPlanJob;
            Assert.IsNotNull(goapPlanJob);
            if(goapPlanJob.targetPOI != null && goapPlanJob.targetPOI.gridTileLocation != null) {
                return goapPlanJob.targetPOI.gridTileLocation.structure.IsResident(character);
            } else if (goapPlanJob.targetPOI != null && goapPlanJob.targetPOI.gridTileLocation != null && goapPlanJob.targetPOI is TileObject targetTileObject) {
                if (targetTileObject.IsOwnedBy(character)) {
                    return true;
                }
            }
            return false;
        }
    }
}