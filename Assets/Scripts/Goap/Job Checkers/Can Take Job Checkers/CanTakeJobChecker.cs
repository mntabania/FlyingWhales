using Traits;
namespace Goap.Job_Checkers {
    public abstract class CanTakeJobChecker {

        public abstract string key { get; } 
        public abstract bool CanTakeJob(Character character, JobQueueItem jobQueueItem);
    }
}