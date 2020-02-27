using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

public class Quest : IJobOwner {
    public JOB_OWNER ownerType { get { return JOB_OWNER.QUEST; } }
    public string name { get; protected set; }
    public string description { get; protected set; }
    public Faction factionOwner { get; protected set; }
    public Region region { get; protected set; }
    //public JobQueue jobQueue { get; protected set; }
    public bool isActivated { get; protected set; }
    public int duration { get; protected set; }
    public int currentDuration { get; protected set; }
    public List<JobQueueItem> availableJobs { get; protected set; }
    public List<JobQueueItem> forcedCancelJobsOnTickEnded { get; private set; }
    
    public JobTriggerComponent jobTriggerComponent { get; }
    
    public Quest(Faction factionOwner, Region region) {
        this.factionOwner = factionOwner;
        this.region = region;
        name = "Quest";
        availableJobs = new List<JobQueueItem>();
        forcedCancelJobsOnTickEnded = new List<JobQueueItem>();
        //jobQueue = new JobQueue(this);
        //jobQueue.SetQuest(this);
    }
    public Quest(SaveDataQuest data) {
        name = data.name;
        description = data.description;
        region = GridMap.Instance.GetRegionByID(data.regionID);
        factionOwner = FactionManager.Instance.GetFactionBasedOnID(data.factionOwnerID);
        availableJobs = new List<JobQueueItem>();
        //jobQueue = new JobQueue(this);
        //jobQueue.SetQuest(this);
    }

    #region Jobs
    public void AddToAvailableJobs(JobQueueItem job) {
        availableJobs.Add(job);
        OnAddJob(job);
    }
    public bool RemoveFromAvailableJobs(JobQueueItem job) {
        if (availableJobs.Remove(job)) {
            OnRemoveJob(job);
            JobManager.Instance.OnFinishJob(job);
            return true;
        }
        return false;
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
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem job = availableJobs[i];
            if (job.assignedCharacter == null && character.jobQueue.CanJobBeAddedToQueue(job)) {
                return job;
            }
        }
        return null;
    }
    public bool AssignCharacterToJobBasedOnVision(Character character) {
        List<JobQueueItem> choices = new List<JobQueueItem>();
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem job = availableJobs[i];
            if (job.assignedCharacter == null) {
                GoapPlanJob goapJob = job as GoapPlanJob;
                if (character.marker.inVisionPOIs.Contains(goapJob.targetPOI) && character.jobQueue.CanJobBeAddedToQueue(job)) {
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
                if (goapJob.targetPOI != null && character.marker.inVisionPOIs.Contains(goapJob.targetPOI) &&
                    character.jobQueue.CanJobBeAddedToQueue(job)) {
                    return job;
                }
            }
        }
        return null;
    }
    public bool HasJob(JOB_TYPE jobType) {
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem jqi = availableJobs[i];
            if (jqi.jobType == jobType) {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Virtuals
    //On its own, when quest is made, it is still not active, this must be called to activate quest
    public virtual void ActivateQuest() {
        isActivated = true;
        Messenger.AddListener(Signals.TICK_STARTED, PerTickOnQuest);
        Messenger.AddListener<IPointOfInterest, string>(Signals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, ForceCancelAllJobsTargettingCharacter);
        //Messenger.AddListener<IPointOfInterest, string, JOB_TYPE>(Signals.FORCE_CANCEL_ALL_JOB_TYPES_TARGETING_POI, ForceCancelJobTypesTargetingPOI);
        Messenger.AddListener(Signals.TICK_ENDED, OnTickEnded);
    }
    public virtual void FinishQuest() {
        isActivated = false;
        Messenger.RemoveListener(Signals.TICK_STARTED, PerTickOnQuest);
        Messenger.RemoveListener<IPointOfInterest, string>(Signals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, ForceCancelAllJobsTargettingCharacter);
        //Messenger.RemoveListener<IPointOfInterest, string, JOB_TYPE>(Signals.FORCE_CANCEL_ALL_JOB_TYPES_TARGETING_POI, ForceCancelJobTypesTargetingPOI);
        Messenger.RemoveListener(Signals.TICK_ENDED, OnTickEnded);
    }
    protected virtual void PerTickOnQuest() {
        if(duration > 0) {
            currentDuration++;
            if (currentDuration >= duration) {
                FinishQuest();
            }
        }
    }
    protected virtual void OnAddJob(JobQueueItem job) {
        Messenger.Broadcast(Signals.ADD_QUEST_JOB, this, job);
    }
    protected virtual void OnRemoveJob(JobQueueItem job) {
        Messenger.Broadcast(Signals.REMOVE_QUEST_JOB, this, job);
    }
    public virtual void AdjustCurrentDuration(int amount) {
        //int prevCurrDuration = currentDuration;
        currentDuration += amount;
        //currentDuration = Mathf.Max(0, currentDuration);
        //if(currentDuration < 0) {
        //    Debug.LogError("Adjusted current duration(" + prevCurrDuration + ") of " + name + " by " + amount + ", but the result is a negative value: " + currentDuration);
        //}
    }
    #endregion

    #region IJobOwner
    public void OnJobAddedToCharacterJobQueue(JobQueueItem job, Character character) {
        //RemoveFromAvailableJobs(job);
    }
    public void OnJobRemovedFromCharacterJobQueue(JobQueueItem job, Character character) {
        if (!job.IsJobStillApplicable()) {
            RemoveFromAvailableJobs(job);
        }
    }
    public bool ForceCancelJob(JobQueueItem job) {
        return RemoveFromAvailableJobs(job);
    }
    private void ForceCancelAllJobsTargettingCharacter(IPointOfInterest target, string reason) {
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem job = availableJobs[i];
            if (job is GoapPlanJob) {
                GoapPlanJob goapJob = job as GoapPlanJob;
                if (goapJob.targetPOI == target) {
                    if (goapJob.ForceCancelJob(false, reason)) {
                        i--;
                    }
                }
            }
        }
    }
    private void ForceCancelJobTypesTargetingPOI(IPointOfInterest target, string reason, JOB_TYPE jobType) {
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem job = availableJobs[i];
            if (job.jobType == jobType && job is GoapPlanJob) {
                GoapPlanJob goapJob = job as GoapPlanJob;
                if (goapJob.targetPOI == target) {
                    if (goapJob.ForceCancelJob(false, reason)) {
                        i--;
                    }
                }
            }
        }
    }
    public void AddForcedCancelJobsOnTickEnded(JobQueueItem job) {
        if (!forcedCancelJobsOnTickEnded.Contains(job)) {
            forcedCancelJobsOnTickEnded.Add(job);
        }
    }
    public void ProcessForcedCancelJobsOnTickEnded() {
        if (forcedCancelJobsOnTickEnded.Count > 0) {
            for (int i = 0; i < forcedCancelJobsOnTickEnded.Count; i++) {
                forcedCancelJobsOnTickEnded[i].ForceCancelJob(false);
            }
            forcedCancelJobsOnTickEnded.Clear();
        }
    }
    #endregion

    #region General
    public void SetCurrentDuration(int amount) {
        currentDuration = amount;
    }
    public void SetDuration(int amount) {
        duration = amount;
    }
    private void OnTickEnded() {
        ProcessForcedCancelJobsOnTickEnded();
    }
    #endregion
}

[System.Serializable]
public class SaveDataQuest {
    public string name;
    public string description;
    public int factionOwnerID;
    public int regionID;
    public bool isActivated;
    public List<SaveDataJobQueueItem> jobs;

    public virtual void Save(Quest quest) {
        name = quest.name;
        description = quest.description;
        factionOwnerID = quest.factionOwner.id;
        regionID = quest.region.id;
        isActivated = quest.isActivated;


        //    jobs = new List<SaveDataJobQueueItem>();
        //    for (int i = 0; i < quest.availableJobs.Count; i++) {
        //        JobQueueItem job = quest.availableJobs[i];
        //        if (job.isNotSavable) {
        //            continue;
        //        }
        //        //SaveDataJobQueueItem data = System.Activator.CreateInstance(System.Type.GetType("SaveData" + job.GetType().ToString())) as SaveDataJobQueueItem;
        //        SaveDataJobQueueItem data = null;
        //        if (job is GoapPlanJob) {
        //            data = new SaveDataGoapPlanJob();
        //        } else if (job is CharacterStateJob) {
        //            data = new SaveDataCharacterStateJob();
        //        }
        //        data.Save(job);
        //        jobs.Add(data);
        //    }
    }

    public virtual Quest Load() {
        string noSpacesName = UtilityScripts.Utilities.RemoveAllWhiteSpace(name);
        Quest quest = System.Activator.CreateInstance(System.Type.GetType(noSpacesName), this) as Quest;
        //for (int i = 0; i < jobs.Count; i++) {
        //    JobQueueItem job = jobs[i].Load();
        //    quest.jobQueue.AddJobInQueue(job, false);
        //    //if (jobs[i] is SaveDataCharacterStateJob) {
        //    //    SaveDataCharacterStateJob dataStateJob = jobs[i] as SaveDataCharacterStateJob;
        //    //    CharacterStateJob stateJob = job as CharacterStateJob;
        //    //    if (dataStateJob.assignedCharacterID != -1) {
        //    //        Character assignedCharacter = CharacterManager.Instance.GetCharacterByID(dataStateJob.assignedCharacterID);
        //    //        stateJob.SetAssignedCharacter(assignedCharacter);
        //    //        CharacterState newState = assignedCharacter.stateComponent.SwitchToState(stateJob.targetState, null, stateJob.targetSettlement);
        //    //        if (newState != null) {
        //    //            stateJob.SetAssignedState(newState);
        //    //        } else {
        //    //            throw new System.Exception(assignedCharacter.name + " tried doing state " + stateJob.targetState.ToString() + " but was unable to do so! This must not happen!");
        //    //        }
        //    //    }
        //    //}
        //}
        return quest;
    }
}
