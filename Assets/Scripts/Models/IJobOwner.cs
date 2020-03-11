using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IJobOwner {
    JOB_OWNER ownerType { get; }
    string name { get; }
    JobTriggerComponent jobTriggerComponent { get; }
    List<JobQueueItem> forcedCancelJobsOnTickEnded { get; }

    void OnJobAddedToCharacterJobQueue(JobQueueItem job, Character character);
    void OnJobRemovedFromCharacterJobQueue(JobQueueItem job, Character character);
    bool ForceCancelJob(JobQueueItem job);
    void AddForcedCancelJobsOnTickEnded(JobQueueItem job);
    void ProcessForcedCancelJobsOnTickEnded();
}
