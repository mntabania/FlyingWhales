using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JobQueue {
    public Character owner { get; private set; }
    //public Quest quest { get; private set; } //If there is a quest it means that this is a quest job

    public List<JobQueueItem> jobsInQueue { get; private set; }

    //public bool isAreaOrQuestJobQueue {
    //    get { return owner == null; }
    //}
    public JobQueue(Character owner) {
        this.owner = owner;
        jobsInQueue = new List<JobQueueItem>();
    }

    public bool AddJobInQueue(JobQueueItem job) { //, bool processLogicForPersonalJob = true
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
            if (!isJobQueueEmpty) {
                job.ProcessJob();
            }
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
        Messenger.Broadcast(Signals.JOB_ADDED_TO_QUEUE, job, owner);
        //if(quest != null) {
        //    quest.OnAddJob(job);
        //}

        //if (processLogicForPersonalJob) {
        //    //bool hasProcessed = false;
        //    //Character characterOwner = owner as Character;
        //    //If the current action's job of the character is overridable and the added job has higher priority than it,
        //    //then process that job immediately and stop what the character is currently doing
        //    //Sometimes, in rare cases, the character still cannot be assigned to a job even if it is a personal job,
        //    //It might be because CanTakeJob/CanCharacterTakeThisJob function is not satisfied
        //    //if (processLogicForPersonalJob) {
        //    if (hasBeenInserted && insertIndex == 0){ //This means that the job is inserted as the top most priority //characterOwner.CanCurrentJobBeOverriddenByJob(job))
        //        CurrentTopPriorityIsPushedBackBy(job);
        //    } 
        //    //else {
        //    //    //If a character is a higher priority than one of the currently existing jobs
        //    //    //Check all currently existing plans of character
        //    //    //All plans' job of character that has lower priority than the added job must be dropped so that the job will be prioritized than those 
        //    //    if (hasBeenInserted) {
        //    //        for (int i = 0; i < owner.allGoapPlans.Count; i++) {
        //    //            GoapPlan plan = owner.allGoapPlans[i];
        //    //            if(plan.job == null || (job.priority < plan.job.priority)) {
        //    //                if(plan.job == null) {
        //    //                    owner.JustDropPlan(plan);
        //    //                } else {
        //    //                    if (plan.job.jobQueueParent.isAreaOrQuestJobQueue) {
        //    //                        plan.job.UnassignJob(false);
        //    //                    } else {
        //    //                        owner.JustDropPlan(plan);
        //    //                    }
        //    //                }
        //    //                i--;
        //    //            }
        //    //        }
        //    //    }
        //    //}
        //    //}
        //    //if (processLogicForPersonalJob && !hasProcessed) {
        //    //    if ((character.stateComponent.currentState != null && (character.stateComponent.currentState.characterState == CHARACTER_STATE.STROLL || character.stateComponent.currentState.characterState == CHARACTER_STATE.STROLL_OUTSIDE))
        //    //        || (character.currentAction != null && character.currentAction.goapType == INTERACTION_TYPE.RETURN_HOME &&
        //    //        (character.currentAction.parentPlan == null || character.currentAction.parentPlan.category == GOAP_CATEGORY.IDLE))) {
        //    //        character.jobQueue.ProcessFirstJobInQueue(character);
        //    //    }
        //    //}
        //}
        return true;
    }
    public bool RemoveJobInQueue(JobQueueItem job, bool shouldDoAfterEffect = true, string reason = "") {
        if (jobsInQueue.Remove(job)) {
            Messenger.Broadcast(Signals.JOB_REMOVED_FROM_QUEUE, job, owner);
            
            job.UnassignJob(shouldDoAfterEffect, reason);
            string ownerName = owner.name;
            string removeLog = $"{job.name} has been removed from {ownerName} job queue.";
            //removeLog += "\nIs Personal: " + (character != null ? character.name : "False");
            //removeLog += "\nAssigned Character: " + (job.assignedCharacter != null ? job.assignedCharacter.name : "None");
            //if(job is GoapPlanJob) {
            //    GoapPlanJob planJob = job as GoapPlanJob;
            //removeLog += "\nAssigned Plan: " + (planJob.assignedPlan != null);
            //}
            Debug.Log(GameManager.Instance.TodayLogString() + removeLog);
            //if (quest != null) {
            //    quest.OnRemoveJob(job);
            //}
            bool state = job.OnRemoveJobFromQueue();
            job.originalOwner.OnJobRemovedFromCharacterJobQueue(job, owner);
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
    public bool HasJobWithOtherData(JOB_TYPE jobType, object otherData) {
        for (int i = 0; i < jobsInQueue.Count; i++) {
            if (jobsInQueue[i].jobType == jobType && jobsInQueue[i] is GoapPlanJob) {
                GoapPlanJob job = jobsInQueue[i] as GoapPlanJob;
                if(job.allOtherData != null) {
                    for (int j = 0; j < job.allOtherData.Count; j++) {
                        object data = job.allOtherData[j];
                        if(data == otherData) {
                            return true;
                        }
                    }
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
    public void CancelAllJobs() {
        for (int i = 0; i < jobsInQueue.Count; i++) {
            if (jobsInQueue[i].CancelJob()) {
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