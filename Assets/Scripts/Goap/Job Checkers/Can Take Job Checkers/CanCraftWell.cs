namespace Goap.Job_Checkers {
    public class CanCraftWell : CanTakeJobChecker {
        public override string key => JobManager.Can_Craft_Well;
        public override bool CanTakeJob(Character character, JobQueueItem jobQueueItem) {
            return TILE_OBJECT_TYPE.WATER_WELL.CanBeCraftedBy(character);
        }
    }
}