namespace Goap.Job_Checkers {
    public class CanTakeChangeClass : CanTakeJobChecker {
        
        public override string key => JobManager.Can_Take_Change_Class;
        public override bool CanTakeJob(Character character, JobQueueItem jobQueueItem) {
            return !character.characterClass.IsSpecialClass();
        }
    }
}