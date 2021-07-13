using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JobQueue {
    public Character owner { get; private set; }

    public List<JobQueueItem> jobsInQueue { get; private set; }
    
    public JobQueue(Character owner) {
        this.owner = owner;
        jobsInQueue = new List<JobQueueItem>();
    }

    #region Loading
    public void LoadReferences(SaveDataCharacter saveDataCharacter) { 
        for (int i = 0; i < saveDataCharacter.jobs.Count; i++) {
            string jobID = saveDataCharacter.jobs[i];
            JobQueueItem jobQueueItem = DatabaseManager.Instance.jobDatabase.GetJobWithPersistentIDSafe(jobID);
            if (jobQueueItem != null) {
                jobsInQueue.Add(jobQueueItem);
                jobQueueItem.OnAddJobToQueue();    
            }
        }
    }
    #endregion
    
    public bool AddJobInQueue(JobQueueItem job) { //, bool processLogicForPersonalJob = true
        if (!owner.limiterComponent.canPerform) {
            //We are only checking the jobs that has an assigned plan because we already have a handle for jobs that goes through multithread in ReceivePlanFromGoapThread
            //Adding job in queue with assigned plan means that the job has fixes steps and will not go to the multithread anymore to get a plan
            if(job is GoapPlanJob goapPlanJob && goapPlanJob.assignedPlan != null) {
                int canPerformValue = owner.limiterComponent.canPerformValue;
                if (canPerformValue == -1 && (owner.traitContainer.HasTrait("Paralyzed") || owner.traitContainer.HasTrait("Quarantined"))) {
                    //If the owner is paralyzed and the only reason he cannot perform is because of that paralyzed, the plan must not be scrapped
                } else {
#if DEBUG_LOG
                    owner.logComponent.PrintLogIfActive($"{owner.name} is scrapping plan since {owner.name} cannot perform. {job.name} is the job.");
#endif
                    return false;
                }
            }
        }
        if (job.jobType.IsFullnessRecoveryTypeJob() && !owner.limiterComponent.canDoFullnessRecovery) {
            //If character is fasting, prevent any fullness recovery job from being added.
            return false;
        }
        if (!CanJobBeAddedToQueue(job)) {
            return false;
        }
        job.SetAssignedCharacter(owner);

        //if (jobsInQueue.Count > 0) { //characterOwner.CanCurrentJobBeOverriddenByJob(job))
        //    JobQueueItem topJob = jobsInQueue[0];
        //    if (job.priority < topJob.priority) {
        //        if(topJob.CanBeInterrupted() || job.IsAnInterruptionJob()) {
        //            topJob.PushedBack(job); //This means that the job is inserted as the top most priority
        //            isNewJobTopPriority = true;
        //        }
        //    }
        //}
        bool isNewJobTopPriority = owner.minion != null || IsJobTopPriorityWhenAdded(job);
        if (isNewJobTopPriority) {
            bool isJobQueueEmpty = jobsInQueue.Count <= 0;
            //Push back current top priority first before adding the job
            if (!isJobQueueEmpty) {
                if (owner.minion != null) {
                    if (job.jobType == JOB_TYPE.COMBAT) {
                        jobsInQueue[0].PushedBack(job); //This means that the job is inserted as the top most priority
                    } else {
                        CancelAllJobs();
                    }
                } else {
                    jobsInQueue[0].PushedBack(job); //This means that the job is inserted as the top most priority
                }
                // jobsInQueue[0].PushedBack(job); //This means that the job is inserted as the top most priority
            }

            //Insert job in the top of the list
            jobsInQueue.Insert(0, job);

            //If job queue has jobs even before the new job is inserted, process it
            //UPDATE: Whether or not the job queue is empty, if this is the highest priority when it is added to queue, process it immediately.
            //Reason: So that the character will spend less time waiting to do an action, because right now, after doing an action, a character will wait 1-2 ticks before doing another one
            job.ProcessJob();

            //if (!isJobQueueEmpty) {
            //    job.ProcessJob();
            //}
        } else {
            bool hasBeenInserted = false;
            if(jobsInQueue.Count > 1) {
                for (int i = 1; i < jobsInQueue.Count; i++) {
                    if (job.priority > jobsInQueue[i].priority) {
                        jobsInQueue.Insert(i, job);
                        hasBeenInserted = true;
                        break;
                    }
                }
            }
            if (!hasBeenInserted) {
                jobsInQueue.Add(job);
            }
        }

        job.OnAddJobToQueue();
        job.originalOwner?.OnJobAddedToCharacterJobQueue(job, owner);
        Messenger.Broadcast(JobSignals.JOB_ADDED_TO_QUEUE, job, owner);
        if (job.jobType == JOB_TYPE.TRIGGER_FLAW) {
            job.isTriggeredFlaw = true;
        }
        return true;
    }
    public bool RemoveJobInQueue(JobQueueItem job, string reason = "") {
        if (jobsInQueue.Remove(job)) {
            Messenger.Broadcast(JobSignals.JOB_REMOVED_FROM_QUEUE, job, owner);
            owner.combatComponent.OnJobRemovedFromQueue(job);
            job.UnassignJob(reason);
            string ownerName = owner.name;
#if DEBUG_LOG
            Debug.Log(GameManager.Instance.TodayLogString() + $"{job.name} has been removed from {ownerName} job queue.");
#endif
            bool state = job.OnRemoveJobFromQueue();
            job.originalOwner?.OnJobRemovedFromCharacterJobQueue(job, owner);
            return state;
        }
        return false;
    }
    private bool IsJobTopPriorityWhenAdded(JobQueueItem newJob) {
        int highestBehaviourPriority = owner.behaviourComponent.GetHighestBehaviourPriority();
        if (highestBehaviourPriority > newJob.priority) {
            //if the highest priority behaviour is higher than the new job, then the new job should not be considered as top priority.
            //NOTE: If the new job has the same priority as the highest priority behaviour, then the new job will be processed as normal.
            return false;
        } else {
            if (jobsInQueue.Count > 0) { //characterOwner.CanCurrentJobBeOverriddenByJob(job))
                JobQueueItem topJob = jobsInQueue[0];
                if (newJob.priority > topJob.priority) {
                    if (topJob.CanBeInterruptedBy(newJob.jobType)) {
                        return true;
                    }
                }
                return false;
            }
            //If there are no jobs in queue, the new job is automatically the top priority
            return true;    
        }
    }
    public bool IsJobTopTypePriorityWhenAdded(JOB_TYPE jobType) {
        if (jobsInQueue.Count > 0) { //characterOwner.CanCurrentJobBeOverriddenByJob(job))
            JobQueueItem topJob = jobsInQueue[0];
            if (jobType.GetJobTypePriority() > topJob.priority) {
                if (topJob.CanBeInterruptedBy(jobType)) {
                    return true;
                }
            }
            return false;
        }
        //If there are no jobs in queue, the new job is automatically the top priority
        return true;
    }
    //public void MoveJobToTopPriority(JobQueueItem job) {
    //    if (jobsInQueue.Remove(job)) {
    //        jobsInQueue.Insert(0, job);
    //    }
    //}
    //public bool IsJobInTopPriority(JobQueueItem job) {
    //    return jobsInQueue.Count > 0 && jobsInQueue[0] == job;
    //}
    //public JobQueueItem GetFirstUnassignedJobInQueue(Character characterToDoJob) {
    //    if (jobsInQueue.Count > 0) {
    //        for (int i = 0; i < jobsInQueue.Count; i++) {
    //            JobQueueItem job = jobsInQueue[i];
    //            if (job.CanCharacterDoJob(characterToDoJob)) {
    //                return job;
    //            }
    //        }
    //    }
    //    return null;
    //}
    public bool ProcessFirstJobInQueue() {
        //if(owner.ownerType == JOB_OWNER.CHARACTER) {
        //    if (jobsInQueue.Count > 0) {
        //        jobsInQueue[0].ProcessJob();
        //        return true;
        //    }
        //} else {
        //    if (jobsInQueue.Count > 0) {
        //        for (int i = 0; i < jobsInQueue.Count; i++) {
        //            JobQueueItem job = jobsInQueue[i];
        //            if (characterToDoJob.jobQueue.AddJobInQueue(job)) {
        //                RemoveJobInQueue(job);
        //                return true;
        //            }
        //        }
        //    }
        //}
        if (jobsInQueue.Count > 0 && owner.HasSameOrHigherPriorityJobThanBehaviour()) {
            jobsInQueue[0].ProcessJob();
            return true;
        }
        return false;
    }
    //public void CurrentTopPriorityIsPushedBack() {
    //    //if(owner.ownerType != JOB_OWNER.CHARACTER) {
    //    //    return;
    //    //}
    //    if (owner.stateComponent.currentState != null) {
    //        owner.stateComponent.currentState.OnExitThisState();
    //        //This call is doubled so that it will also exit the previous major state if there's any
    //        if (owner.stateComponent.currentState != null) {
    //            owner.stateComponent.currentState.OnExitThisState();
    //        }
    //    }
    //    //else if (character.stateComponent.stateToDo != null) {
    //    //    character.stateComponent.SetStateToDo(null);
    //    //} 
    //    else {
    //        if (owner.currentParty.icon.isTravelling) {
    //            if (owner.currentParty.icon.travelLine == null) {
    //                owner.marker.StopMovement();
    //            } else {
    //                owner.currentParty.icon.SetOnArriveAction(() => owner.OnArriveAtAreaStopMovement());
    //            }
    //        }
    //        owner.StopCurrentActionNode(false, "Have something important to do");
    //        //owner.AdjustIsWaitingForInteraction(1);
    //        //owner.StopCurrentAction(false, "Have something important to do");
    //        //owner.AdjustIsWaitingForInteraction(-1);
    //    }

    //    //if (AssignCharacterToJob(job)) {
    //    //    if (job is CharacterStateJob) {
    //    //        //Will no longer stop what is currently doing if job is a state job because it will already be done by that state
    //    //        return;
    //    //    }
    //    //    if (characterOwner.stateComponent.currentState != null) {
    //    //        characterOwner.stateComponent.currentState.OnExitThisState();
    //    //        //This call is doubled so that it will also exit the previous major state if there's any
    //    //        if (characterOwner.stateComponent.currentState != null) {
    //    //            characterOwner.stateComponent.currentState.OnExitThisState();
    //    //        }
    //    //    }
    //    //    //else if (character.stateComponent.stateToDo != null) {
    //    //    //    character.stateComponent.SetStateToDo(null);
    //    //    //} 
    //    //    else {
    //    //        if (characterOwner.currentParty.icon.isTravelling) {
    //    //            if (characterOwner.currentParty.icon.travelLine == null) {
    //    //                characterOwner.marker.StopMovement();
    //    //            } else {
    //    //                characterOwner.currentParty.icon.SetOnArriveAction(() => characterOwner.OnArriveAtAreaStopMovement());
    //    //            }
    //    //        }
    //    //        characterOwner.AdjustIsWaitingForInteraction(1);
    //    //        characterOwner.StopCurrentAction(false, "Have something important to do");
    //    //        characterOwner.AdjustIsWaitingForInteraction(-1);
    //    //    }
    //    //}
    //}
    //public bool AssignCharacterToJob(JobQueueItem job) {
    //    if (CanJobBeAddedToQueue(job)) {
    //        //job.SetAssignedCharacter(characterToDoJob);
    //        if (job is GoapPlanJob) {
    //            GoapPlanJob goapPlanJob = job as GoapPlanJob;
    //            if (goapPlanJob.targetInteractionType != INTERACTION_TYPE.NONE) {
    //                characterToDoJob.StartGOAP(goapPlanJob.targetInteractionType, goapPlanJob.targetPOI, GOAP_CATEGORY.WORK, false, null, true, goapPlanJob, goapPlanJob.otherData, goapPlanJob.allowDeadTargets);
    //            } else {
    //                characterToDoJob.StartGOAP(goapPlanJob.goals, goapPlanJob.targetPOI, GOAP_CATEGORY.WORK, false, null, true, goapPlanJob, goapPlanJob.otherData, goapPlanJob.allowDeadTargets);
    //            }
    //            //if (goapPlanJob.targetPlan != null) {
    //            //    characterToDoJob.AddPlan(goapPlanJob.targetPlan);
    //            //    goapPlanJob.SetAssignedPlan(goapPlanJob.targetPlan);
    //            //} else {
    //            //    if (goapPlanJob.targetInteractionType != INTERACTION_TYPE.NONE) {
    //            //        characterToDoJob.StartGOAP(goapPlanJob.targetInteractionType, goapPlanJob.targetPOI, GOAP_CATEGORY.WORK, false, null, true, goapPlanJob, goapPlanJob.otherData, goapPlanJob.allowDeadTargets);
    //            //    } else {
    //            //        characterToDoJob.StartGOAP(goapPlanJob.targetEffect, goapPlanJob.targetPOI, GOAP_CATEGORY.WORK, false, null, true, goapPlanJob, goapPlanJob.otherData, goapPlanJob.allowDeadTargets);
    //            //    }
    //            //}
    //        } else if (job is CharacterStateJob) {
    //            CharacterStateJob stateJob = job as CharacterStateJob;
    //            CharacterState newState = characterToDoJob.stateComponent.SwitchToState(stateJob.targetState);
    //            if (newState != null) {
    //                stateJob.SetAssignedState(newState);
    //            } else {
    //                throw new System.Exception(characterToDoJob.name + " tried doing state " + stateJob.targetState.ToString() + " but was unable to do so! This must not happen!");
    //            }
    //        }
    //        return true;
    //    }
    //    return false;
    //}
    public bool CanJobBeAddedToQueue(JobQueueItem job) {
        //Personal jobs can only be added to job queue of original owner
        if (job.originalOwner.ownerType == JOB_OWNER.CHARACTER) {
            return job.originalOwner == owner;
        } else {
            //Only add npcSettlement/quest jobs if character it is the top priority and the owner of this job queue can do the job
            if (jobsInQueue.Count > 0) {
                if (job.priority > jobsInQueue[0].priority) {
                    return job.CanCharacterDoJob(owner);
                } else {
                    return false;
                }
            } else {
                return job.CanCharacterDoJob(owner);
            }
        }
    }
    //public void ForceAssignCharacterToJob(JobQueueItem job, Character characterToDoJob) {
    //    if (job.assignedCharacter == null) {
    //        job.SetAssignedCharacter(characterToDoJob);
    //        if (job is GoapPlanJob) {
    //            GoapPlanJob goapPlanJob = job as GoapPlanJob;
    //            //if (goapPlanJob.targetPlan != null) {
    //            //    characterToDoJob.AddPlan(goapPlanJob.targetPlan);
    //            //    goapPlanJob.SetAssignedPlan(goapPlanJob.targetPlan);
    //            //} else {
    //            //    if (goapPlanJob.targetInteractionType != INTERACTION_TYPE.NONE) {
    //            //        characterToDoJob.StartGOAP(goapPlanJob.targetInteractionType, goapPlanJob.targetPOI, GOAP_CATEGORY.WORK, false, null, true, goapPlanJob, goapPlanJob.otherData, goapPlanJob.allowDeadTargets);
    //            //    } else {
    //            //        characterToDoJob.StartGOAP(goapPlanJob.targetEffect, goapPlanJob.targetPOI, GOAP_CATEGORY.WORK, false, null, true, goapPlanJob, goapPlanJob.otherData, goapPlanJob.allowDeadTargets);
    //            //    }
    //            //}
    //            if (goapPlanJob.targetInteractionType != INTERACTION_TYPE.NONE) {
    //                characterToDoJob.StartGOAP(goapPlanJob.targetInteractionType, goapPlanJob.targetPOI, GOAP_CATEGORY.WORK, false, null, true, goapPlanJob, goapPlanJob.otherData, goapPlanJob.allowDeadTargets);
    //            } else {
    //                characterToDoJob.StartGOAP(goapPlanJob.goals, goapPlanJob.targetPOI, GOAP_CATEGORY.WORK, false, null, true, goapPlanJob, goapPlanJob.otherData, goapPlanJob.allowDeadTargets);
    //            }
    //        } else if (job is CharacterStateJob) {
    //            CharacterStateJob stateJob = job as CharacterStateJob;
    //            CharacterState newState = characterToDoJob.stateComponent.SwitchToState(stateJob.targetState);
    //            if (newState != null) {
    //                stateJob.SetAssignedState(newState);
    //            } else {
    //                throw new System.Exception(characterToDoJob.name + " tried doing state " + stateJob.targetState.ToString() + " but was unable to do so! This must not happen!");
    //            }
    //        }
    //    }
    //}

    public void CancelAllJobsRelatedTo(GOAP_EFFECT_CONDITION conditionType, IPointOfInterest poi) {
        for (int i = 0; i < jobsInQueue.Count; i++) {
            if(jobsInQueue[i] is GoapPlanJob) {
                GoapPlanJob job = jobsInQueue[i] as GoapPlanJob;
                if (job.HasGoalConditionType(conditionType) && job.targetPOI == poi) {
                    if (job.CancelJob()) {
                        i--;
                    }
                }
            }
        }
    }
    public void CancelAllJobsRelatedTo(CHARACTER_STATE state, Character actor) {
        for (int i = 0; i < jobsInQueue.Count; i++) {
            if (jobsInQueue[i] is CharacterStateJob) {
                CharacterStateJob job = jobsInQueue[i] as CharacterStateJob;
                if (job.targetState == state && job.assignedCharacter == actor) {
                    if (job.CancelJob()) {
                        i--;
                    }
                }
            }
        }
    }
    public bool HasJob(JobQueueItem job) {
        for (int i = 0; i < jobsInQueue.Count; i++) {
            if (job == jobsInQueue[i]) {
                return true;
            }
        }
        return false;
    }
    public bool HasJob(GoapEffect effect, IPointOfInterest target) {
        for (int i = 0; i < jobsInQueue.Count; i++) {
            JobQueueItem jqi = jobsInQueue[i];
            if (jqi is GoapPlanJob gpj) {
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
    public bool HasJob(params JOB_TYPE[] jobTypes) {
        for (int i = 0; i < jobsInQueue.Count; i++) {
            for (int j = 0; j < jobTypes.Length; j++) {
                if (jobsInQueue[i].jobType == jobTypes[j]) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool HasJob(JOB_TYPE jobType) {
        for (int i = 0; i < jobsInQueue.Count; i++) {
            if (jobsInQueue[i].jobType == jobType) {
                return true;
            }
        }
        return false;
    }
    public bool HasJob(JOB_TYPE jobType, IPointOfInterest targetPOI) {
        for (int i = 0; i < jobsInQueue.Count; i++) {
            if(jobsInQueue[i].jobType == jobType && jobsInQueue[i] is GoapPlanJob) {
                GoapPlanJob job = jobsInQueue[i] as GoapPlanJob;
                if (job.targetPOI == targetPOI) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool HasJob(JOB_TYPE jobType, INTERACTION_TYPE targetGoapActionType) {
        for (int i = 0; i < jobsInQueue.Count; i++) {
            if (jobsInQueue[i].jobType == jobType && jobsInQueue[i] is GoapPlanJob) {
                GoapPlanJob job = jobsInQueue[i] as GoapPlanJob;
                if (job.targetInteractionType == targetGoapActionType) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool HasJobRelatedTo(GOAP_EFFECT_CONDITION conditionType, IPointOfInterest poi) {
        for (int i = 0; i < jobsInQueue.Count; i++) {
            if (jobsInQueue[i] is GoapPlanJob) {
                GoapPlanJob job = jobsInQueue[i] as GoapPlanJob;
                if (job.HasGoalConditionType(conditionType) && job.targetPOI == poi) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool HasJobRelatedTo(GOAP_EFFECT_CONDITION conditionType, string conditionKey, IPointOfInterest poi) {
        for (int i = 0; i < jobsInQueue.Count; i++) {
            if (jobsInQueue[i] is GoapPlanJob) {
                GoapPlanJob job = jobsInQueue[i] as GoapPlanJob;
                if (job.HasGoalConditionType(conditionType) && job.HasGoalConditionKey(conditionKey) && job.targetPOI == poi) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool HasJobRelatedTo(CHARACTER_STATE state) {
        for (int i = 0; i < jobsInQueue.Count; i++) {
            if (jobsInQueue[i] is CharacterStateJob) {
                CharacterStateJob job = jobsInQueue[i] as CharacterStateJob;
                if (job.targetState == state) {
                    return true;
                }
            }
        }
        return false;
    }
    public JobQueueItem GetJob(JOB_TYPE jobType, IPointOfInterest targetPOI) {
        for (int i = 0; i < jobsInQueue.Count; i++) {
            if (jobsInQueue[i].jobType == jobType && jobsInQueue[i] is GoapPlanJob) {
                GoapPlanJob job = jobsInQueue[i] as GoapPlanJob;
                if (job.targetPOI == targetPOI) {
                    return job;
                }
            }
        }
        return null;
    }
    public JobQueueItem GetJob(params JOB_TYPE[] jobTypes) {
        for (int i = 0; i < jobsInQueue.Count; i++) {
            for (int j = 0; j < jobTypes.Length; j++) {
                if (jobsInQueue[i].jobType == jobTypes[j]) {
                    return jobsInQueue[i];
                }
            }
        }
        return null;
    }
    public JobQueueItem GetJobByID(int id) {
        for (int i = 0; i < jobsInQueue.Count; i++) {
            if (jobsInQueue[i].id == id) {
                return jobsInQueue[i];
            }
        }
        return null;
    }
    public JobQueueItem GetJobByName(string name) {
        for (int i = 0; i < jobsInQueue.Count; i++) {
            if (jobsInQueue[i].name == name) {
                return jobsInQueue[i];
            }
        }
        return null;
    }
    public List<JobQueueItem> GetJobs(JOB_TYPE jobType) {
        List<JobQueueItem> jobs = new List<JobQueueItem>();
        for (int i = 0; i < jobsInQueue.Count; i++) {
            if (jobsInQueue[i].jobType == jobType) {
                jobs.Add(jobsInQueue[i]);
            }
        }
        return jobs;
    }
    public int GetNumberOfJobsWith(CHARACTER_STATE state) {
        int count = 0;
        for (int i = 0; i < jobsInQueue.Count; i++) {
            if (jobsInQueue[i] is CharacterStateJob) {
                CharacterStateJob job = jobsInQueue[i] as CharacterStateJob;
                if (job.targetState == state) {
                    count++;
                }
            }
        }
        return count;
    }
    public int GetNumberOfJobsWith(JOB_TYPE type) {
        int count = 0;
        for (int i = 0; i < jobsInQueue.Count; i++) {
            if (jobsInQueue[i].jobType == type) {
                count++;
            }
        }
        return count;
    }
    /// <summary>
    /// Get the number of jobs that return true from the provided checker.
    /// </summary>
    /// <param name="checker">The function that checks if the item is valid</param>
    /// <returns></returns>
    public int GetNumberOfJobsWith(System.Func<JobQueueItem, bool> checker) {
        int count = 0;
        for (int i = 0; i < jobsInQueue.Count; i++) {
            if (checker.Invoke(jobsInQueue[i])) {
                count++;
            }
        }
        return count;
    }
    public void CancelAllJobs(JOB_TYPE jobType) {
        for (int i = 0; i < jobsInQueue.Count; i++) {
            JobQueueItem job = jobsInQueue[i];
            if (job.jobType == jobType) {
                if (job.CancelJob()) {
                    i--;
                }
            }
        }
    }
    public void CancelAllPartyJobs() {
        for (int i = 0; i < jobsInQueue.Count; i++) {
            JobQueueItem job = jobsInQueue[i];
            if (job.isThisAPartyJob) {
                if (job.CancelJob()) {
                    i--;
                }
            }
        }
    }
    public void CancelAllGatheringJobs() {
        for (int i = 0; i < jobsInQueue.Count; i++) {
            JobQueueItem job = jobsInQueue[i];
            if (job.isThisAGatheringJob) {
                if (job.CancelJob()) {
                    i--;
                }
            }
        }
    }
    public void CancelAllJobs(params JOB_TYPE[] jobTypes) {
        for (int i = 0; i < jobsInQueue.Count; i++) {
            for (int j = 0; j < jobTypes.Length; j++) {
                JobQueueItem job = jobsInQueue[i];
                if (job.jobType == jobTypes[j]) {
                    if (job.CancelJob()) {
                        i--;
                    }
                    break;
                }
            }
        }
    }
    public void CancelAllJobs(string reason = "") {
        for (int i = 0; i < jobsInQueue.Count; i++) {
            if (jobsInQueue[i].CancelJob(reason: reason)) {
                i--;
            }
        }
    }
    public void CancelFirstJob() {
        if(jobsInQueue.Count > 0) {
            jobsInQueue[0].CancelJob();
        }
    }
    public int GetJobQueueIndex(JobQueueItem job) {
        for (int i = 0; i < jobsInQueue.Count; i++) {
            if (jobsInQueue[i] == job) {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// Unassign all jobs that a certain character has taken.
    /// </summary>
    /// <param name="character">The character in question.</param>
    //public void UnassignAllJobsTakenBy(Character character) {
    //    string summary = "Unassigning all jobs taken by " + character.name;
    //    List<JobQueueItem> allJobs = new List<JobQueueItem>(jobsInQueue);
    //    for (int i = 0; i < allJobs.Count; i++) {
    //        JobQueueItem currJob = allJobs[i];
    //        if (currJob.assignedCharacter == character) {
    //            //if (character.currentAction != null && character.currentAction.parentPlan.job != null && character.currentAction.parentPlan.job == currJob) {
    //            //    //skip
    //            //    character.currentAction.parentPlan.job.SetAssignedCharacter(null);
    //            //    continue;
    //            //}
    //            summary += "\nUnassigned " + character.name + " from job " + currJob.name; 
    //            currJob.UnassignJob(false);
    //        }
    //    }
    //    character.PrintLogIfActive(summary);
    //}

    //public void AddPremadeJob(Character actor, JOB_TYPE jobType, GOAP_CATEGORY goapCategory, bool allowDeadTargets = false, bool isStealth = false, bool cancelOnFail = false,
    //    params IGoapJobPremadeNodeCreator[] premadeCreator) {

    //    List<GoapNode> nodes = new List<GoapNode>();
    //    for (int i = 0; i < premadeCreator.Length; i++) {
    //        if(premadeCreator[i] is ActionJobPremadeNodeCreator) {
    //            GoapAction action = InteractionManager.Instance.CreateNewGoapInteraction(((ActionJobPremadeNodeCreator) premadeCreator[i]).actionType, actor, premadeCreator[i].targetPOI);
    //            GoapNode parentNode = null;
    //            if(nodes.Count > 0) {
    //                parentNode = nodes[nodes.Count - 1];
    //            }
    //            GoapNode node = new GoapNode(parentNode, action.cost, action);
    //            nodes.Add(node);
    //        }
    //    }
    //    GoapNode goalNode = nodes[0];
    //    GoapNode startingNode = nodes[nodes.Count - 1];
    //    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, goalNode.action.goapType, goalNode.action.poiTarget);

    //    //GOAP_EFFECT_CONDITION[] goalEffects = new GOAP_EFFECT_CONDITION[goalNode.action.expectedEffects.Count];
    //    //for (int i = 0; i < goalNode.action.expectedEffects.Count; i++) {
    //    //    goalEffects[i] = goalNode.action.expectedEffects[i].conditionType;
    //    //}
    //    GoapPlan plan = new GoapPlan(startingNode, goalEffects, goapCategory);
    //    plan.ConstructAllNodes();
    //    plan.SetDoNotRecalculate(true);
    //    job.SetIsStealth(isStealth);
    //    job.SetAssignedPlan(plan);
    //    job.SetAssignedCharacter(actor);
    //    job.SetCancelOnFail(cancelOnFail);

    //    actor.jobQueue.AddJobInQueue(job, false);

    //    //TODO: Add plan immediately? Stop current action or state?
    //}
}