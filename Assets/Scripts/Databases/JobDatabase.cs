using System;
using System.Collections.Generic;
namespace Databases {
    public class JobDatabase {
        public Dictionary<string, JobQueueItem> jobsByGUID { get; }
        public List<JobQueueItem> allJobs { get; }
        
        public JobDatabase() {
            jobsByGUID = new Dictionary<string, JobQueueItem>();
            allJobs = new List<JobQueueItem>();
        }

        public void Register(JobQueueItem job) {
            jobsByGUID.Add(job.persistentID, job);
            allJobs.Add(job);
        }
        public bool UnRegister(JobQueueItem job) {
            allJobs.Remove(job);
            if (jobsByGUID.Remove(job.persistentID)) {
                return true;
            }
            return false;
        }

        public JobQueueItem GetJobWithPersistentID(string id) {
            if (jobsByGUID.ContainsKey(id)) {
                return jobsByGUID[id];
            }
            throw new Exception($"Could not find job with id {id}");
        }
        public JobQueueItem GetJobWithPersistentIDSafe(string id) {
            if (jobsByGUID.ContainsKey(id)) {
                return jobsByGUID[id];
            }
            return null;
        }
    }
}