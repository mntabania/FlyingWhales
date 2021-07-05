using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Goap.Job_Checkers;
using Traits;
using UnityEngine.Assertions;

public abstract class JobQueueItem : ISavable {
    public string persistentID { get; private set; }
    public abstract OBJECT_TYPE objectType { get; }
    public abstract Type serializedData { get; }
    public int id { get; protected set; }
    public IJobOwner originalOwner { get; protected set; } //The true original owner of this job
    public Character assignedCharacter { get; protected set; } //Only has value if job is inside character's job queue
    public string name { get; private set; }
    public JOB_TYPE jobType { get; protected set; }
    public bool isStealth { get; private set; }
    public bool finishedSuccessfully { get; protected set; }
    public List<Character> blacklistedCharacters { get; private set; }
    public int priority { get { return GetPriority(); } }
    public CanTakeJobChecker canTakeJobChecker { get; private set; }
    public JobApplicabilityChecker stillApplicable { get; protected set; }
    public bool doNotRecalculate { get; protected set; }
    public int invalidCounter { get; protected set; }
    public bool isThisAPartyJob { get; protected set; }
    public bool isThisAGatheringJob { get; protected set; }
    public bool cannotBePushedBack { get; protected set; }
    public bool shouldBeRemovedFromSettlementWhenUnassigned { get; protected set; }
    public bool forceCancelOnInvalid { get; protected set; }
    public bool isInMultithread { get; protected set; }
    public bool shouldForceCancelUponReceiving { get; protected set; }

    public bool isTriggeredFlaw { set; get; }

    //object pool
    /// <summary>
    /// Has this job been returned to the pool?
    /// </summary>
    public bool hasBeenReset { get; protected set; }

    protected int _priority; //The lower the amount the higher the priority

    public virtual IPointOfInterest poiTarget => null;

    public JobQueueItem() {
        id = -1;
        blacklistedCharacters = new List<Character>();
    }

    protected void Initialize(JOB_TYPE jobType, IJobOwner owner) {
        persistentID = UtilityScripts.Utilities.GetNewUniqueID();
        id = UtilityScripts.Utilities.SetID(this);
        hasBeenReset = false;
        this.jobType = jobType;
        originalOwner = owner;
        if (originalOwner == null) {
            throw new Exception($"Original owner of job {ToString()} is null");
        }
        name = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(this.jobType.ToString());
        SetInitialPriority();
        Messenger.AddListener<JOB_TYPE, IPointOfInterest>(JobSignals.CHECK_JOB_APPLICABILITY, CheckJobApplicability);
        Messenger.AddListener<IPointOfInterest>(JobSignals.CHECK_APPLICABILITY_OF_ALL_JOBS_TARGETING, CheckJobApplicability);
        Messenger.AddListener<JOB_TYPE>(JobSignals.CHECK_JOB_APPLICABILITY_OF_ALL_JOBS_OF_TYPE, CheckJobApplicability);
        DatabaseManager.Instance.jobDatabase.Register(this);
    }
    protected void Initialize(SaveDataJobQueueItem data) {
        persistentID = data.persistentID;
        id = UtilityScripts.Utilities.SetID(this, data.id);
        hasBeenReset = false;
        name = data.name;
        jobType = data.jobType;
        SetIsStealth(data.isStealth);
        SetDoNotRecalculate(data.doNotRecalculate);
        invalidCounter = data.invalidCounter;
        SetIsThisAPartyJob(data.isThisAPartyJob);
        SetIsThisAGatheringJob(data.isThisAGatheringJob);
        SetCannotBePushedBack(data.cannotBePushedBack);
        SetShouldBeRemovedFromSettlementWhenUnassigned(data.shouldBeRemovedFromSettlementWhenUnassigned);
        SetForceCancelOnInvalid(data.forceCancelOnInvalid);

        if (!string.IsNullOrEmpty(data.canTakeJobKey)) {
            SetCanTakeThisJobChecker(data.canTakeJobKey);    
        }
        if (!string.IsNullOrEmpty(data.applicabilityCheckerKey)) {
            SetStillApplicableChecker(data.applicabilityCheckerKey);
        }
        SetInitialPriority();
        Messenger.AddListener<JOB_TYPE, IPointOfInterest>(JobSignals.CHECK_JOB_APPLICABILITY, CheckJobApplicability);
        Messenger.AddListener<IPointOfInterest>(JobSignals.CHECK_APPLICABILITY_OF_ALL_JOBS_TARGETING, CheckJobApplicability);
        Messenger.AddListener<JOB_TYPE>(JobSignals.CHECK_JOB_APPLICABILITY_OF_ALL_JOBS_OF_TYPE, CheckJobApplicability);
        DatabaseManager.Instance.jobDatabase.Register(this);
    }

    #region Loading
    //Returns true if the job is still viable (meaning the data is not corrupted), false, if already corrupted, i.e. the original owner/actor/target is null even if it actually isn't
    public virtual bool LoadSecondWave(SaveDataJobQueueItem data) {
        bool isViable = true;
        if (!string.IsNullOrEmpty(data.originalOwnerID)) {
            if (data.originalOwnerType == OBJECT_TYPE.Settlement) {
                originalOwner = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentID(data.originalOwnerID) as NPCSettlement;
            } else if (data.originalOwnerType == OBJECT_TYPE.Character) {
                originalOwner = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(data.originalOwnerID);
            } else if (data.originalOwnerType == OBJECT_TYPE.Faction) {
                originalOwner = DatabaseManager.Instance.factionDatabase.GetFactionBasedOnPersistentID(data.originalOwnerID);
            } else if (data.originalOwnerType == OBJECT_TYPE.Party) {
                originalOwner = DatabaseManager.Instance.partyDatabase.GetPartyByPersistentID(data.originalOwnerID);
            }
            if (originalOwner == null) {
                isViable = false;
            }
        }
        if (!string.IsNullOrEmpty(data.assignedCharacterID)) {
            assignedCharacter = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(data.assignedCharacterID);
            if (assignedCharacter == null) {
                isViable = false;
            }
        }
        for (int i = 0; i < data.blacklistedCharacterIDs.Count; i++) {
            Character character = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(data.blacklistedCharacterIDs[i]);
            if (character != null) {
                blacklistedCharacters.Add(character);
            }
        }
        return isViable;
    }
    #endregion
    
    #region Virtuals
    protected virtual bool CanTakeJob(Character character) {
        //Character cannot take settlement job only if he is wanted by his current faction
        //If he is not a criminal or he is not wanted by his current faction, allow to take job
        bool isWantedByCurrentFaction = false;
        if (character.traitContainer.HasTrait("Criminal") && character.faction != null) {
            if (character.crimeComponent.activeCrimes.Count > 0) {
                for (int i = 0; i < character.crimeComponent.activeCrimes.Count; i++) {
                    CrimeData data = character.crimeComponent.activeCrimes[i];
                    if (data.IsWantedBy(character.faction)) {
                        isWantedByCurrentFaction = true;
                        break;
                    }
                }
            }
        }
        return !isWantedByCurrentFaction && character.limiterComponent.canPerform; //!character.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE)
    }
    public virtual void UnassignJob(string reason) { }
    public virtual void OnAddJobToQueue() { }
    public virtual bool OnRemoveJobFromQueue() { return true; }
    public virtual void AddOtherData(INTERACTION_TYPE actionType, object[] data) { }
    public virtual bool CanCharacterTakeThisJob(Character character) {
        if (originalOwner.ownerType == JOB_OWNER.CHARACTER) {
            //All jobs that are personal will bypass _canTakeThisJob/_canTakeThisJobWithTarget function checkers
            return CanTakeJob(character);
        } else if (originalOwner.ownerType == JOB_OWNER.SETTLEMENT || originalOwner.ownerType == JOB_OWNER.FACTION) {
            //if (!character.characterClass.CanDoJob(jobType) && character.jobComponent.primaryJob != jobType && 
            //    !character.jobComponent.priorityJobs.Contains(jobType)) {
            //    return false;
            //}
            if (!character.jobComponent.CanDoJob(jobType)) {
                return false;
            }
        }
        if (canTakeJobChecker != null) {
            if (canTakeJobChecker.CanTakeJob(character, this)) {
                return CanTakeJob(character);
            }
            return false;
        }
        // if(canTakeThis != null) {
        //     if (canTakeThis(character)) {
        //         return CanTakeJob(character);
        //     }
        //     return false;
        // } else if (canTakeThisJob != null) {
        //     if (canTakeThisJob(character, this)) {
        //         return CanTakeJob(character);
        //     }
        //     return false;
        // }
        return CanTakeJob(character);
    }
    public virtual void OnCharacterAssignedToJob(Character character) { }
    public virtual void OnCharacterUnassignedToJob(Character character) { }
    public virtual bool ProcessJob() { return false; }
    //Returns true or false if job was really removed in queue
    //reason parameter only applies if the job that is being cancelled is the currentActionNode's job
    public virtual bool CancelJob(string reason = "") {
        //When cancelling a job, we must check if it's personal or not because if it is a faction/npcSettlement job it cannot be removed from queue
        //The only way for a faction/npcSettlement job to be removed is if it is forced or it is actually finished
        if(assignedCharacter == null) {
            //Can only cancel jobs that are in character job queue
            return false;
        }
        return assignedCharacter.jobQueue.RemoveJobInQueue(this, reason);
        //if (process) {
        //    if (job is GoapPlanJob && cause != "") {
        //        GoapPlanJob planJob = job as GoapPlanJob;
        //        Character actor = null;
        //        if (!isAreaOrQuestJobQueue) {
        //            actor = this.owner;
        //        } else if (job.assignedCharacter != null) {
        //            actor = job.assignedCharacter;
        //        }
        //        if (actor != null && actor != planJob.targetPOI) { //only log if the actor is not the same as the target poi.
        //            actor.RegisterLogAndShowNotifToThisCharacterOnly("Generic", "job_cancelled_cause", null, cause);
        //        }
        //    }
        //    UnassignJob(shouldDoAfterEffect, reason);
        //}
        //return hasBeenRemovedInJobQueue;
    }
    public virtual bool ForceCancelJob(string reason = "") {
        if (assignedCharacter != null) {
            JOB_OWNER ownerType = originalOwner.ownerType;
            bool hasBeenRemoved = assignedCharacter.jobQueue.RemoveJobInQueue(this, reason);
            if (ownerType == JOB_OWNER.CHARACTER) {
                return hasBeenRemoved;
            }
        }
        return originalOwner.ForceCancelJob(this);
    }
    public virtual void PushedBack(JobQueueItem jobThatPushedBack) {
        if(!cannotBePushedBack || jobThatPushedBack.jobType == JOB_TYPE.DIG_THROUGH) {
            string stopText = string.Empty;
            if(jobThatPushedBack.jobType != JOB_TYPE.DIG_THROUGH) {
                stopText = "Have something important to do";
                if (assignedCharacter != null) {
                    if (jobThatPushedBack is CharacterStateJob stateJob && stateJob.targetState == CHARACTER_STATE.COMBAT && assignedCharacter.combatComponent.avoidInRange.Count > 0) {
                        stopText = "Got scared of something";
                    }
                }
            }
            // if (jobThatPushedBack.IsAnInterruptionJob()) {
            //     stopText = "Interrupted";
            // }
            assignedCharacter?.StopCurrentActionNode(stopText);
        } else {
            //If job is cannot be pushed back and it is pushed back, cancel it instead
            CancelJob();
        }
    }
    public virtual void StopJobNotDrop() {
        if (cannotBePushedBack) {
            //If job is cannot be pushed back and it is stopped, cancel it
            CancelJob();
        } else {
            assignedCharacter?.StopCurrentActionNode();
        }
    }
    public virtual bool CanBeInterruptedBy(JOB_TYPE jobType) { return true; }
    protected virtual void CheckJobApplicability(JOB_TYPE p_jobType, IPointOfInterest p_targetPOI) { }
    protected virtual void CheckJobApplicability(JOB_TYPE p_jobType) { }
    protected virtual void CheckJobApplicability(IPointOfInterest p_targetPOI) { }
    #endregion

    public void SetAssignedCharacter(Character character) {
        Character previousAssignedCharacter = null;
        if (assignedCharacter != null) {
            previousAssignedCharacter = assignedCharacter;
#if DEBUG_LOG
            assignedCharacter.logComponent.PrintLogIfActive($"{assignedCharacter.name} quit job {name}");
#endif
        }
#if DEBUG_LOG
        character?.logComponent.PrintLogIfActive($"{character.name} took job {name}");
#endif

        assignedCharacter = character;
        if (assignedCharacter != null) {
            OnCharacterAssignedToJob(assignedCharacter);
        } else if (assignedCharacter == null && previousAssignedCharacter != null) {
            OnCharacterUnassignedToJob(previousAssignedCharacter);
        }
    }

#region Can Take Job
    private void SetCanTakeThisJobChecker(CanTakeJobChecker canTakeJobChecker) {
        this.canTakeJobChecker = canTakeJobChecker;
    }
    public void SetCanTakeThisJobChecker(string canTakeJobCheckerKey) {
        SetCanTakeThisJobChecker(JobManager.Instance.GetJobChecker(canTakeJobCheckerKey));
    }
#endregion

#region Applicability
    public void SetStillApplicableChecker(string applicabilityKey) {
        SetStillApplicableChecker(JobManager.Instance.GetApplicabilityChecker(applicabilityKey));
    }
    public void SetStillApplicableChecker(JobApplicabilityChecker jobApplicabilityChecker) {
        stillApplicable = jobApplicabilityChecker;
    }
#endregion
    
    public void SetCannotBePushedBack (bool state) {
        cannotBePushedBack = state;
    }
    /// <summary>
    /// Should this job be removed from the settlement job queue that owns it when
    /// it has been dropped by its assigned character?
    /// </summary>
    /// <param name="state">Set the state of the condition.</param>
    public void SetShouldBeRemovedFromSettlementWhenUnassigned(bool state) {
        shouldBeRemovedFromSettlementWhenUnassigned = state;
    }
    public void AddBlacklistedCharacter(Character character) {
        if (!blacklistedCharacters.Contains(character)) {
            blacklistedCharacters.Add(character);
        }
    }
    public void RemoveBlacklistedCharacter(Character character) {
        blacklistedCharacters.Remove(character);
    }
    public void ClearBlacklist() {
        blacklistedCharacters.Clear();
    }
    public void SetIsStealth(bool state) {
        isStealth = state;
    }
    public void SetFinishedSuccessfully(bool state) {
        finishedSuccessfully = state;
    }
    public void SetForceCancelOnInvalid(bool state) {
        forceCancelOnInvalid = state;
    }

#region Priority
    public int GetPriority() {
        return _priority;
    }
    public void SetPriority(int amount) {
        _priority = amount;
    }
    private void SetInitialPriority() {
        int priority = jobType.GetJobTypePriority();
        Assert.IsTrue(priority > 0, $"Cannot set initial priority for {name} job because priority is {priority}");
        SetPriority(priority);
    }
#endregion

#region Utilities
    public bool CanCharacterDoJob(Character character) {
        return CanCharacterTakeThisJob(character) && !blacklistedCharacters.Contains(character);
    }
    public override string ToString() {
        return $"{jobType} assigned to {assignedCharacter?.name}" ?? "None";
    }
    public bool IsJobStillApplicable() {
        if (stillApplicable != null) {
            return stillApplicable.IsJobStillApplicable(this);
        }
        return true;
    }
    public void SetDoNotRecalculate(bool state) {
        doNotRecalculate = state;
    }
    public void IncreaseInvalidCounter() {
        invalidCounter++;
    }
    public void ResetInvalidCounter() {
        invalidCounter = 0;
    }
    public void SetIsThisAPartyJob(bool state) {
        isThisAPartyJob = state;
    }
    public void SetIsThisAGatheringJob(bool state) {
        isThisAGatheringJob = state;
    }
    public void SetIsInMultithread(bool state) {
        isInMultithread = state;
    }
    public void SetShouldForceCancelJobUponReceiving(bool state) {
        shouldForceCancelUponReceiving = state;
    }
#endregion

#region Job Object Pool
    public virtual void Reset() {
#if DEBUG_LOG
        Debug.Log($"{GameManager.Instance.TodayLogString()}Job {this} was reset with original owner {originalOwner}");
#endif
        DatabaseManager.Instance.jobDatabase.UnRegister(this);
        persistentID = string.Empty;
        hasBeenReset = true;
        shouldBeRemovedFromSettlementWhenUnassigned = false;
        id = -1;
        originalOwner = null;
        name = string.Empty;
        jobType = JOB_TYPE.NONE;
        blacklistedCharacters.Clear();
        canTakeJobChecker = null;
        assignedCharacter = null;
        stillApplicable = null;
        isTriggeredFlaw = false;
        SetIsStealth(false);
        SetPriority(-1);
        SetCannotBePushedBack(false);
        SetFinishedSuccessfully(false);
        SetDoNotRecalculate(false);
        SetIsThisAPartyJob(false);
        SetIsThisAGatheringJob(false);
        SetForceCancelOnInvalid(false);
        SetIsInMultithread(false);
        SetShouldForceCancelJobUponReceiving(false);
        ResetInvalidCounter();
        Messenger.RemoveListener<JOB_TYPE, IPointOfInterest>(JobSignals.CHECK_JOB_APPLICABILITY, CheckJobApplicability);
        Messenger.RemoveListener<IPointOfInterest>(JobSignals.CHECK_APPLICABILITY_OF_ALL_JOBS_TARGETING, CheckJobApplicability);
        Messenger.RemoveListener<JOB_TYPE>(JobSignals.CHECK_JOB_APPLICABILITY_OF_ALL_JOBS_OF_TYPE, CheckJobApplicability);
    }
#endregion
}