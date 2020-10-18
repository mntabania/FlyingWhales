using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UtilityScripts;

public class JobBoard {
    public List<JobQueueItem> availableJobs { get; }

    public JobBoard() {
        availableJobs = new List<JobQueueItem>();
    }

    public void Initialize() {
        availableJobs.Clear();
    }
    public void InitializeFromSaveData(SaveDataJobBoard data) {

    }

    #region Jobs
    public void AddToAvailableJobs(JobQueueItem job, int position = -1) {
        if (position == -1) {
            availableJobs.Add(job);
        } else {
            availableJobs.Insert(position, job);
        }
        //if (job is GoapPlanJob goapJob) {
        //    Debug.Log($"{GameManager.Instance.TodayLogString()}{goapJob} targeting {goapJob.targetPOI} was added to {name}'s available jobs");
        //} else {
        //    Debug.Log($"{GameManager.Instance.TodayLogString()}{job} was added to {name}'s available jobs");
        //}
    }
    public bool RemoveFromAvailableJobs(JobQueueItem job) {
        if (availableJobs.Remove(job)) {
            //if (job is GoapPlanJob) {
            //    GoapPlanJob goapJob = job as GoapPlanJob;
            //    Debug.Log($"{GameManager.Instance.TodayLogString()}{goapJob} targeting {goapJob.targetPOI?.name} was removed from {name}'s available jobs");
            //} else {
            //    Debug.Log($"{GameManager.Instance.TodayLogString()}{job} was removed from {name}'s available jobs");
            //}
            OnJobRemovedFromAvailableJobs(job);
            return true;
        }
        return false;
    }
    private void OnJobRemovedFromAvailableJobs(JobQueueItem job) {
        JobManager.Instance.OnFinishJob(job);
    }
    public int GetNumberOfJobsWith(JOB_TYPE type) {
        int count = 0;
        for (int i = 0; i < availableJobs.Count; i++) {
            if (availableJobs[i].jobType == type) {
                count++;
            }
        }
        return count;
    }
    public bool HasJob(JOB_TYPE job, IPointOfInterest target) {
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem jqi = availableJobs[i];
            if (jqi is GoapPlanJob) {
                GoapPlanJob gpj = jqi as GoapPlanJob;
                if (job == gpj.jobType && target == gpj.targetPOI) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool HasJob(params JOB_TYPE[] jobTypes) {
        for (int i = 0; i < availableJobs.Count; i++) {
            for (int j = 0; j < jobTypes.Length; j++) {
                if (availableJobs[i].jobType == jobTypes[j]) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool HasJob(GoapEffect effect, IPointOfInterest target) {
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem jqi = availableJobs[i];
            if (jqi is GoapPlanJob) {
                GoapPlanJob gpj = jqi as GoapPlanJob;
                if (effect.conditionType == gpj.goal.conditionType
                    && effect.conditionKey == gpj.goal.conditionKey
                    && effect.target == gpj.goal.target
                    && target == gpj.targetPOI) {
                    return true;
                }
            }
        }
        return false;
    }
    public JobQueueItem GetJob(params JOB_TYPE[] jobTypes) {
        for (int i = 0; i < availableJobs.Count; i++) {
            for (int j = 0; j < jobTypes.Length; j++) {
                JobQueueItem job = availableJobs[i];
                if (job.jobType == jobTypes[j]) {
                    return job;
                }
            }
        }
        return null;
    }
    public List<JobQueueItem> GetJobs(params JOB_TYPE[] jobTypes) {
        List<JobQueueItem> jobs = new List<JobQueueItem>();
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem job = availableJobs[i];
            if (jobTypes.Contains(job.jobType)) {
                jobs.Add(job);
            }
        }
        return jobs;
    }
    public JobQueueItem GetJob(JOB_TYPE job, IPointOfInterest target) {
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem jqi = availableJobs[i];
            if (jqi is GoapPlanJob) {
                GoapPlanJob gpj = jqi as GoapPlanJob;
                if (job == gpj.jobType && target == gpj.targetPOI) {
                    return gpj;
                }
            }
        }
        return null;
    }
    public bool AddFirstUnassignedJobToCharacterJob(Character character) {
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem job = availableJobs[i];
            if (job.assignedCharacter == null && character.jobQueue.AddJobInQueue(job)) {
                return true;
            }
        }
        return false;
    }
    public JobQueueItem GetFirstUnassignedJobToCharacterJob(Character character) {
        //JobQueueItem chosenPriorityJob = null;
        //JobQueueItem chosenSecondaryJob = null;
        //JobQueueItem chosenAbleJob = null;

        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem job = availableJobs[i];
            if (job.assignedCharacter == null && character.jobQueue.CanJobBeAddedToQueue(job)) {
                return job;
                //if (job.jobType == character.jobComponent.primaryJob) {
                //    return job;
                //} else if (chosenPriorityJob == null && character.characterClass.priorityJobs != null
                //    && (character.characterClass.priorityJobs.Contains(job.jobType) || character.jobComponent.priorityJobs.Contains(job.jobType))) {
                //    chosenPriorityJob = job;
                //} else if (chosenSecondaryJob == null && character.characterClass.secondaryJobs != null && character.characterClass.secondaryJobs.Contains(job.jobType)) {
                //    chosenSecondaryJob = job;
                //} else if (chosenAbleJob == null && character.characterClass.ableJobs != null && character.characterClass.ableJobs.Contains(job.jobType)) {
                //    chosenAbleJob = job;
                //}
            }
        }
        //if (chosenPriorityJob != null) {
        //    return chosenPriorityJob;
        //} else if (chosenSecondaryJob != null) {
        //    return chosenSecondaryJob;
        //} else if (chosenAbleJob != null) {
        //    return chosenAbleJob;
        //}
        return null;
    }
    public bool AssignCharacterToJobBasedOnVision(Character character) {
        List<JobQueueItem> choices = new List<JobQueueItem>();
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem job = availableJobs[i];
            if (job.assignedCharacter == null && job is GoapPlanJob) {
                GoapPlanJob goapJob = job as GoapPlanJob;
                if (goapJob.targetPOI != null && character.marker.IsPOIInVision(goapJob.targetPOI) &&
                    character.jobQueue.CanJobBeAddedToQueue(job)) {
                    choices.Add(job);
                }
            }
        }
        if (choices.Count > 0) {
            JobQueueItem job = CollectionUtilities.GetRandomElement(choices);
            return character.jobQueue.AddJobInQueue(job);
        }
        return false;
    }
    public JobQueueItem GetFirstJobBasedOnVision(Character character) {
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem job = availableJobs[i];
            if (job.assignedCharacter == null && job is GoapPlanJob) {
                GoapPlanJob goapJob = job as GoapPlanJob;
                if (goapJob.targetPOI != null && character.marker.IsPOIInVision(goapJob.targetPOI) &&
                    character.jobQueue.CanJobBeAddedToQueue(job)) {
                    return job;
                }
            }
        }
        return null;
    }
    public JobQueueItem GetFirstJobBasedOnVisionExcept(Character character, params JOB_TYPE[] jobTypes) {
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem job = availableJobs[i];
            if (job.assignedCharacter == null && job is GoapPlanJob goapJob && !jobTypes.Contains(goapJob.jobType)) {
                if (goapJob.targetPOI != null && character.marker.IsPOIInVision(goapJob.targetPOI) &&
                    character.jobQueue.CanJobBeAddedToQueue(goapJob)) {
                    return goapJob;
                }
            }
        }
        return null;
    }
    private void ClearAllBlacklistToAllExistingJobs() {
        for (int i = 0; i < availableJobs.Count; i++) {
            availableJobs[i].ClearBlacklist();
        }
    }
    #endregion

    #region Loading
    public void LoadReferences(SaveDataJobBoard data) {
        if(data.availableJobs != null) {
            for (int i = 0; i < data.availableJobs.Count; i++) {
                availableJobs.Add(DatabaseManager.Instance.jobDatabase.GetJobWithPersistentID(data.availableJobs[i]));
            }
        }
    }
    #endregion
}

[System.Serializable]
public class SaveDataJobBoard : SaveData<JobBoard> {
    public List<string> availableJobs;

    #region Overrides
    public override void Save(JobBoard data) {
        availableJobs = new List<string>();
        for (int i = 0; i < data.availableJobs.Count; i++) {
            JobQueueItem job = data.availableJobs[i];
            availableJobs.Add(job.persistentID);
            SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(job);
        }
    }
    #endregion
}
