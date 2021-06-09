using System.Collections.Generic;

public static class JobsListExtension {
    public static bool HasJobWithOtherData(this List<JobQueueItem> availableJobs, JOB_TYPE p_jobType, INTERACTION_TYPE p_otherDataType, object p_otherDataObj) {
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem jobQueueItem = availableJobs[i];
            if (jobQueueItem.jobType == p_jobType && jobQueueItem is GoapPlanJob goapPlanJob && goapPlanJob.HasOtherData(p_otherDataType, p_otherDataObj)) {
                return true;
            }
        }
        return false;
    }
    public static bool HasJobWithFoodProducerOtherData(this List<JobQueueItem> availableJobs, JOB_TYPE p_jobType, INTERACTION_TYPE p_otherDataType) {
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem jobQueueItem = availableJobs[i];
            if (jobQueueItem.jobType == p_jobType && jobQueueItem is GoapPlanJob goapPlanJob && goapPlanJob.HasFoodProducerOtherData(p_otherDataType)) {
                return true;
            }
        }
        return false;
    }
    public static List<JobQueueItem> GetJobsWithOtherData(this List<JobQueueItem> availableJobs, JOB_TYPE p_jobType, INTERACTION_TYPE p_otherDataType, object p_otherDataObj) {
        List<JobQueueItem> foundJobs = null;
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem jobQueueItem = availableJobs[i];
            if (jobQueueItem.jobType == p_jobType && jobQueueItem is GoapPlanJob goapPlanJob && goapPlanJob.HasOtherData(p_otherDataType, p_otherDataObj)) {
                if (foundJobs == null) { foundJobs = new List<JobQueueItem>(); }
                foundJobs.Add(jobQueueItem);
            }
        }
        return foundJobs;
    }
    public static void CancelJobs(this List<JobQueueItem> jobs, string reason = "") {
        for (int i = 0; i < jobs.Count; i++) {
            jobs[i].ForceCancelJob(reason);
        }
    }
}
