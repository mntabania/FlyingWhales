namespace Goap.Job_Checkers {
    public abstract class JobApplicabilityChecker {
        public abstract string key { get; }
        public abstract bool IsJobStillApplicable(JobQueueItem job);
    }
}