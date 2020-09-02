namespace Goap.Job_Checkers {
    public class CanCraftTool : CanTakeJobChecker {
        public override string key => JobManager.Can_Craft_Tool;
        public override bool CanTakeJob(Character character, JobQueueItem jobQueueItem) {
            return TILE_OBJECT_TYPE.TOOL.CanBeCraftedBy(character);
        }
    }
}