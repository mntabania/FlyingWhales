namespace Goap.Job_Checkers {
    public class CanCraftPhylactery : CanTakeJobChecker {
        public override string key => JobManager.Can_Craft_Phylactery;
        public override bool CanTakeJob(Character character, JobQueueItem jobQueueItem) {
            return TILE_OBJECT_TYPE.PHYLACTERY.CanBeCraftedBy(character);
        }
    }
}