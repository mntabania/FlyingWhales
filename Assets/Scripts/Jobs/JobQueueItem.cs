using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using Traits;
using UnityEngine.Assertions;

public class JobQueueItem {
    public int id { get; protected set; }
    public IJobOwner originalOwner { get; protected set; } //The true original owner of this job
    public Character assignedCharacter { get; protected set; } //Only has value if job is inside character's job queue
    public string name { get; private set; }
    public JOB_TYPE jobType { get; protected set; }
    //public bool cannotCancelJob { get; private set; }
    //public bool cancelJobOnFail { get; private set; }
    //public bool cannotOverrideJob { get; private set; }
    //public bool canBeDoneInLocation { get; private set; } //If a character is unable to create a plan for this job and the value of this is true, push the job to the location job queue
    //public bool cancelJobOnDropPlan { get; private set; }
    public bool isStealth { get; private set; }
    public bool isNotSavable { get; protected set; }
    public List<Character> blacklistedCharacters { get; private set; }
    public int priority { get { return GetPriority(); } }

    public System.Func<Character, bool> canTakeThis { get; protected set; }
    public System.Func<Character, JobQueueItem, bool> canTakeThisJob { get; protected set; }
    public System.Func<Character, Character, bool> canTakeThisJobWithTarget { get; protected set; }
    public System.Func<bool> stillApplicable { get; protected set; }
    public System.Action<Character, JobQueueItem> onTakeJobAction { get; protected set; }

    protected int _priority; //The lower the amount the higher the priority

    //Additional data
    public bool cannotBePushedBack { get; protected set; }

    public JobQueueItem() {
        id = -1;
        blacklistedCharacters = new List<Character>();
    }

    protected void Initialize(JOB_TYPE jobType, IJobOwner owner) {
        id = UtilityScripts.Utilities.SetID(this);
        this.jobType = jobType;
        this.originalOwner = owner;
        if (this.originalOwner == null) {
            throw new Exception($"Original owner of job {this.ToString()} is null");
        }
        this.name = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(this.jobType.ToString());
        //this.blacklistedCharacters = new List<Character>();
        SetInitialPriority();
        Messenger.AddListener<JOB_TYPE, IPointOfInterest>(Signals.CHECK_JOB_APPLICABILITY, CheckJobApplicability);
        Messenger.AddListener<IPointOfInterest>(Signals.CHECK_APPLICABILITY_OF_ALL_JOBS_TARGETING, CheckJobApplicability);
    }
    protected void Initialize(SaveDataJobQueueItem data) {
        id = UtilityScripts.Utilities.SetID(this, data.id);
        name = data.name;
        jobType = data.jobType;
        isNotSavable = data.isNotSavable;
        //blacklistedCharacters = new List<Character>();
        for (int i = 0; i < data.blacklistedCharacterIDs.Count; i++) {
            blacklistedCharacters.Add(CharacterManager.Instance.GetCharacterByID(data.blacklistedCharacterIDs[i]));
        }
        //SetCannotCancelJob(data.cannotCancelJob);
        //SetCancelOnFail(data.cancelJobOnFail);
        //SetCannotOverrideJob(data.cannotOverrideJob);
        //SetCanBeDoneInLocation(data.canBeDoneInLocation);
        //SetCancelJobOnDropPlan(data.cancelJobOnDropPlan);
        SetIsStealth(data.isStealth);
        SetInitialPriority();
        Messenger.AddListener<JOB_TYPE, IPointOfInterest>(Signals.CHECK_JOB_APPLICABILITY, CheckJobApplicability);
        Messenger.AddListener<IPointOfInterest>(Signals.CHECK_APPLICABILITY_OF_ALL_JOBS_TARGETING, CheckJobApplicability);
    }

    #region Virtuals
    protected virtual bool CanTakeJob(Character character) {
        return !character.traitContainer.HasTrait("Criminal") && !character.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE);
    }
    public virtual void UnassignJob(bool shouldDoAfterEffect, string reason) { }
    public virtual void OnAddJobToQueue() { }
    public virtual bool OnRemoveJobFromQueue() { return true; }
    public virtual void AddOtherData(INTERACTION_TYPE actionType, object[] data) { }
    public virtual bool CanCharacterTakeThisJob(Character character) {
        if (originalOwner.ownerType == JOB_OWNER.CHARACTER) {
            //All jobs that are personal will bypass _canTakeThisJob/_canTakeThisJobWithTarget function checkers
            return CanTakeJob(character);
        }
        if(canTakeThis != null) {
            if (canTakeThis(character)) {
                return CanTakeJob(character);
            }
            return false;
        } else if (canTakeThisJob != null) {
            if (canTakeThisJob(character, this)) {
                return CanTakeJob(character);
            }
            return false;
        }
        return CanTakeJob(character);
    }
    public virtual void OnCharacterAssignedToJob(Character character) {
        onTakeJobAction?.Invoke(character, this);
    }
    public virtual void OnCharacterUnassignedToJob(Character character) { }
    public virtual bool ProcessJob() { return false; }

    //Returns true or false if job was really removed in queue
    //reason parameter only applies if the job that is being cancelled is the currentActionNode's job
    public virtual bool CancelJob(bool shouldDoAfterEffect = true, string reason = "") {
        //When cancelling a job, we must check if it's personal or not because if it is a faction/settlement job it cannot be removed from queue
        //The only way for a faction/settlement job to be removed is if it is forced or it is actually finished
        if(assignedCharacter == null) {
            //Can only cancel jobs that are in character job queue
            return false;
        }
        return assignedCharacter.jobQueue.RemoveJobInQueue(this, shouldDoAfterEffect, reason);
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
    public virtual bool ForceCancelJob(bool shouldDoAfterEffect = true, string reason = "") {
        if (assignedCharacter != null) {
            JOB_OWNER ownerType = originalOwner.ownerType;
            bool hasBeenRemoved = assignedCharacter.jobQueue.RemoveJobInQueue(this, shouldDoAfterEffect, reason);
            if (ownerType == JOB_OWNER.CHARACTER) {
                return hasBeenRemoved;
            }
        }
        return originalOwner.ForceCancelJob(this);
    }
    public virtual void PushedBack(JobQueueItem jobThatPushedBack) {
        if (cannotBePushedBack) {
            //If job is cannot be pushed back and it is pushed back, cancel it instead
            CancelJob(false);
        } else {
            string stopText = "Have something important to do";
            // if (jobThatPushedBack.IsAnInterruptionJob()) {
            //     stopText = "Interrupted";
            // }
            assignedCharacter?.StopCurrentActionNode(false, stopText);
            //if (originalOwner.ownerType != JOB_OWNER.CHARACTER) {
            //    //NOTE! This is temporary only! All jobs, even settlement jobs are just pushed back right now
            //    //assignedCharacter.jobQueue.RemoveJobInQueue(this, false, "Have something important to do");
            //    assignedCharacter.StopCurrentActionNode(false, stopText);
            //} else {
            //    assignedCharacter.StopCurrentActionNode(false, stopText);
            //}
        }
    }
    public virtual void StopJobNotDrop() {
        if (cannotBePushedBack) {
            //If job is cannot be pushed back and it is stopped, cancel it
            CancelJob(false);
        } else {
            assignedCharacter.StopCurrentActionNode(false);
        }
    }
    public virtual bool CanBeInterruptedBy(JOB_TYPE jobType) { return true; }
    protected virtual void CheckJobApplicability(JOB_TYPE jobType, IPointOfInterest targetPOI) { }
    protected virtual void CheckJobApplicability(IPointOfInterest targetPOI) { }
    #endregion

    public void SetAssignedCharacter(Character character) {
        Character previousAssignedCharacter = null;
        if (assignedCharacter != null) {
            previousAssignedCharacter = assignedCharacter;
            //assignedCharacter.SetCurrentJob(null);
            assignedCharacter.logComponent.PrintLogIfActive($"{assignedCharacter.name} quit job {name}");
        }
        if (character != null) {
            //character.SetCurrentJob(this);
            character.logComponent.PrintLogIfActive($"{character.name} took job {name}");
        }
        
        assignedCharacter = character;
        if (assignedCharacter != null) {
            OnCharacterAssignedToJob(assignedCharacter);
        } else if (assignedCharacter == null && previousAssignedCharacter != null) {
            OnCharacterUnassignedToJob(previousAssignedCharacter);
        }
    }
    public void SetCanTakeThisJobChecker(System.Func<Character, bool> function) {
        canTakeThis = function;
    }
    public void SetCanTakeThisJobChecker(System.Func<Character, JobQueueItem, bool> function) {
        canTakeThisJob = function;
    }
    public void SetCanTakeThisJobChecker(System.Func<Character, Character, bool> function) {
        canTakeThisJobWithTarget = function;
    }
    public void SetStillApplicableChecker(System.Func<bool> function) {
        stillApplicable = function;
    }
    public void SetOnTakeJobAction(System.Action<Character, JobQueueItem> action) {
        onTakeJobAction = action;
    }
    public void SetCannotBePushedBack (bool state) {
        cannotBePushedBack = state;
    }
    //public void SetCannotCancelJob(bool state) {
    //    cannotCancelJob = state;
    //}
    //public void SetCancelOnFail(bool state) {
    //    cancelJobOnFail = state;
    //}
    //public void SetCannotOverrideJob(bool state) {
    //    cannotOverrideJob = state;
    //}
    //public void SetCanBeDoneInLocation(bool state) {
    //    canBeDoneInLocation = state;
    //}
    //public void SetCancelJobOnDropPlan(bool state) {
    //    cancelJobOnDropPlan = state;
    //}
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
            return stillApplicable();
        }
        return true;
    }
    #endregion

    #region Job Object Pool
    public virtual void Reset() {
        id = -1;
        originalOwner = null;
        name = string.Empty;
        jobType = JOB_TYPE.NONE;
        isNotSavable = false;
        blacklistedCharacters.Clear();
        canTakeThis = null;
        canTakeThisJob = null;
        canTakeThisJobWithTarget = null;
        onTakeJobAction = null;
        assignedCharacter = null;
        SetIsStealth(false);
        SetPriority(-1);
        SetCannotBePushedBack(false);
        SetStillApplicableChecker(null);
        Messenger.RemoveListener<JOB_TYPE, IPointOfInterest>(Signals.CHECK_JOB_APPLICABILITY, CheckJobApplicability);
        Messenger.RemoveListener<IPointOfInterest>(Signals.CHECK_APPLICABILITY_OF_ALL_JOBS_TARGETING, CheckJobApplicability);
    }
    #endregion
}

[System.Serializable]
public class SaveDataJobQueueItem {
    public int id;
    //public string jobTypeIdentifier;
    //public Character assignedCharacter { get; protected set; }
    public string name;
    public JOB_TYPE jobType;
    public bool cannotCancelJob;
    public bool cancelJobOnFail;
    public bool cannotOverrideJob;
    public bool canBeDoneInLocation; //If a character is unable to create a plan for this job and the value of this is true, push the job to the location job queue
    public bool cancelJobOnDropPlan;
    public bool isStealth;
    public bool isNotSavable;
    public List<int> blacklistedCharacterIDs;

    public string canTakeThisJobMethodName;
    public string canTakeThisJobWithTargetMethodName;
    public string onTakeJobActionMethodName;

    public virtual void Save(JobQueueItem job) {
        id = job.id;
        //jobTypeIdentifier = job.GetType().ToString();
        name = job.name;
        jobType = job.jobType;
        //cannotCancelJob = job.cannotCancelJob;
        //cancelJobOnFail = job.cancelJobOnFail;
        //cannotOverrideJob = job.cannotOverrideJob;
        //canBeDoneInLocation = job.canBeDoneInLocation;
        //cancelJobOnDropPlan = job.cancelJobOnDropPlan;
        isStealth = job.isStealth;
        isNotSavable = job.isNotSavable;

        blacklistedCharacterIDs = new List<int>();
        for (int i = 0; i < job.blacklistedCharacters.Count; i++) {
            blacklistedCharacterIDs.Add(job.blacklistedCharacters[i].id);
        }

        if(job.canTakeThisJob != null) {
            canTakeThisJobMethodName = job.canTakeThisJob.Method.Name;
        } else {
            canTakeThisJobMethodName = string.Empty;
        }
        if (job.canTakeThisJobWithTarget != null) {
            canTakeThisJobWithTargetMethodName = job.canTakeThisJobWithTarget.Method.Name;
        } else {
            canTakeThisJobWithTargetMethodName = string.Empty;
        }
        if (job.onTakeJobAction != null) {
            onTakeJobActionMethodName = job.onTakeJobAction.Method.Name;
        } else {
            onTakeJobActionMethodName = string.Empty;
        }
    }

    public virtual JobQueueItem Load() {
        JobQueueItem job = System.Activator.CreateInstance(System.Type.GetType(GetType().ToString()), this) as JobQueueItem;

        Type thisType = typeof(InteractionManager);
        if(canTakeThisJobMethodName != string.Empty) {
            MethodInfo canTakeThisJobMethod = thisType.GetMethod(canTakeThisJobMethodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if(canTakeThisJobMethod != null) {
                Func<Character, JobQueueItem, bool> function = (Func<Character, JobQueueItem, bool>) Delegate.CreateDelegate(typeof(Func<Character, JobQueueItem, bool>), canTakeThisJobMethod, false);
                job.SetCanTakeThisJobChecker(function);
            }
        }
        if (canTakeThisJobWithTargetMethodName != string.Empty) {
            MethodInfo canTakeThisJobWithTargetMethod = thisType.GetMethod(canTakeThisJobWithTargetMethodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (canTakeThisJobWithTargetMethod != null) {
                Func<Character, Character, bool> function = (Func<Character, Character, bool>) Delegate.CreateDelegate(typeof(Func<Character, Character, bool>), canTakeThisJobWithTargetMethod, false);
                job.SetCanTakeThisJobChecker(function);
            }
        }
        if (onTakeJobActionMethodName != string.Empty) {
            MethodInfo onTakeJobActionMethod = thisType.GetMethod(onTakeJobActionMethodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (onTakeJobActionMethod != null) {
                Action<Character, JobQueueItem> function = (Action<Character, JobQueueItem>) Delegate.CreateDelegate(typeof(Action<Character, JobQueueItem>), onTakeJobActionMethod, false);
                job.SetOnTakeJobAction(function);
            }
        }
        return job;
    }
}