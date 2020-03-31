using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;

public class GoapPlanJob : JobQueueItem {
    public GoapEffect goal { get; protected set; }
    public GoapPlan assignedPlan { get; protected set; }
    //public GoapPlan targetPlan { get; protected set; }
    public IPointOfInterest targetPOI { get; protected set; }
    //public bool willImmediatelyBeDoneAfterReceivingPlan { get; protected set; }

    //interaction type version
    public INTERACTION_TYPE targetInteractionType { get; protected set; } //Only used if the plan to be created uses interaction type

    //if INTERACTION_TYPE is NONE, it means that it is used by all
    public Dictionary<INTERACTION_TYPE, object[]> otherData { get; protected set; } //TODO: Further optimize this by moving this dictionary to the actor itself

    //forced interactions per effect
    //public Dictionary<GoapEffect, INTERACTION_TYPE> forcedActions { get; private set; }

    //plan constructor
    //public System.Func<GoapPlan> planConstructor { get; private set; } //if this is set, the job will execute this when creating a plan instead of using the normal behaviour

    //misc
    //public bool allowDeadTargets { get; private set; }

    public List<object> allOtherData { get; private set; }

    public GoapPlanJob() : base() {
        otherData = new Dictionary<INTERACTION_TYPE, object[]>();
        allOtherData = new List<object>();
    }

    public void Initialize(JOB_TYPE jobType, GoapEffect goal, IPointOfInterest targetPOI, IJobOwner owner) {
        Initialize(jobType, owner);
        this.goal = goal;
        this.targetPOI = targetPOI;
        //forcedActions = new Dictionary<GoapEffect, INTERACTION_TYPE>(new ForcedActionsComparer());
        //allowDeadTargets = false;
    }
    //public void Initialize(JOB_TYPE jobType, GoapEffect goal, IPointOfInterest targetPOI, Dictionary<INTERACTION_TYPE, object[]> otherData, IJobOwner owner){
    //    Initialize(jobType, owner);
    //    this.goal = goal;
    //    this.targetPOI = targetPOI;
    //    //forcedActions = new Dictionary<GoapEffect, INTERACTION_TYPE>(new ForcedActionsComparer());
    //    //allowDeadTargets = false;
    //    this.otherData = otherData;
    //    if (otherData != null) {
    //        isNotSavable = true;
    //        //allOtherData = new List<object>();
    //        foreach (object[] data in otherData.Values) {
    //            if (data != null) {
    //                for (int i = 0; i < data.Length; i++) {
    //                    allOtherData.Add(data[i]);
    //                }
    //            }
    //        }
    //    }
    //}
    //public GoapPlanJob(JOB_TYPE jobType, INTERACTION_TYPE targetInteractionType, IJobOwner owner) : base(jobType, owner) {
    //    //this.targetEffect = targetEffect;
    //    //this.targetPOI = targetEffect.targetPOI;
    //    this.targetInteractionType = targetInteractionType;
    //    this.otherData = null;
    //    //forcedActions = new Dictionary<GoapEffect, INTERACTION_TYPE>(new ForcedActionsComparer());
    //    //allowDeadTargets = false;
    //}
    //public GoapPlanJob(JOB_TYPE jobType, INTERACTION_TYPE targetInteractionType, Dictionary<INTERACTION_TYPE, object[]> otherData, IJobOwner owner) : base(jobType, owner) {
    //    //this.targetEffect = targetEffect;
    //    //this.targetPOI = targetEffect.targetPOI;
    //    this.targetInteractionType = targetInteractionType;
    //    this.otherData = otherData;
    //    //forcedActions = new Dictionary<GoapEffect, INTERACTION_TYPE>(new ForcedActionsComparer());
    //    //allowDeadTargets = false;
    //    if(otherData != null) {
    //        isNotSavable = true;
    //        allOtherData = new List<object>();
    //        foreach (object[] data in otherData.Values) {
    //            if(data != null) {
    //                for (int i = 0; i < data.Length; i++) {
    //                    allOtherData.Add(data[i]);
    //                }
    //            }
    //        }
    //    }
    //}
    public void Initialize(JOB_TYPE jobType, INTERACTION_TYPE targetInteractionType, IPointOfInterest targetPOI, IJobOwner owner) {
        Initialize(jobType, owner);
        //this.targetEffect = targetEffect;
        this.targetPOI = targetPOI;
        this.targetInteractionType = targetInteractionType;
        //this.otherData = otherData;
        //forcedActions = new Dictionary<GoapEffect, INTERACTION_TYPE>(new ForcedActionsComparer());
        //allowDeadTargets = false;
    }
    //public void Initialize(JOB_TYPE jobType, INTERACTION_TYPE targetInteractionType, IPointOfInterest targetPOI, Dictionary<INTERACTION_TYPE, object[]> otherData, IJobOwner owner) : base(jobType, owner) {
    //    //this.targetEffect = targetEffect;
    //    this.targetPOI = targetPOI;
    //    this.targetInteractionType = targetInteractionType;
    //    this.otherData = otherData;
    //    //forcedActions = new Dictionary<GoapEffect, INTERACTION_TYPE>(new ForcedActionsComparer());
    //    //allowDeadTargets = false;
    //    if (otherData != null) {
    //        isNotSavable = true;
    //        allOtherData = new List<object>();
    //        foreach (object[] data in otherData.Values) {
    //            if (data != null) {
    //                for (int i = 0; i < data.Length; i++) {
    //                    allOtherData.Add(data[i]);
    //                }
    //            }
    //        }
    //    }
    //}
    //public GoapPlanJob(JOB_TYPE jobType, GoapPlan targetPlan, IPointOfInterest targetPOI) : base(jobType) {
    //    this.targetPOI = targetPOI;
    //    this.targetPlan = targetPlan;
    //    forcedActions = new Dictionary<GoapEffect, INTERACTION_TYPE>(new ForcedActionsComparer());
    //    allowDeadTargets = false;
    //    isNotSavable = true;
    //}
    public void Initialize(SaveDataGoapPlanJob data) {
        Initialize(data);
        //goals = data.targetEffect.Load();
        //targetInteractionType = data.targetInteractionType;
        //allowDeadTargets = data.allowDeadTargets;
        //if(data.targetPOIID != -1) {
        //    if (data.targetPOIType == POINT_OF_INTEREST_TYPE.CHARACTER) {
        //        targetPOI = CharacterManager.Instance.GetCharacterByID(data.targetPOIID);
        //    } else if (data.targetPOIType == POINT_OF_INTEREST_TYPE.ITEM) {
        //        targetPOI = TokenManager.Instance.GetSpecialTokenByID(data.targetPOIID);
        //    } else if (data.targetPOIType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
        //        targetPOI = InteriorMapManager.Instance.GetTileObject(data.targetPOITileObjectType, data.targetPOIID);
        //    }
        //} else {
        //    targetPOI = null;
        //}
        //forcedActions = new Dictionary<GoapEffect, INTERACTION_TYPE>(new ForcedActionsComparer());
        //for (int i = 0; i < data.forcedActionsGoapEffect.Count; i++) {
        //    GoapEffect effect = data.forcedActionsGoapEffect[i].Load();
        //    forcedActions.Add(effect, data.forcedActionsType[i]);
        //}
    }

    #region Overrides 
    public override bool ProcessJob() {
        if (hasBeenReset) { return false; }
        if(assignedPlan == null && originalOwner != null) {
            Character characterOwner = assignedCharacter;
            bool isPersonal = originalOwner.ownerType == JOB_OWNER.CHARACTER;
            if (targetInteractionType != INTERACTION_TYPE.NONE) {
                characterOwner.planner.StartGOAP(targetInteractionType, targetPOI, this, isPersonal);
            } else {
                characterOwner.planner.StartGOAP(goal, targetPOI, this, isPersonal);
                //for (int i = 0; i < goals.Length; i++) {
                //    Precondition goal = goals[i];
                //    if(!goal.CanSatisfyCondition(characterOwner, targetPOI)) {
                //        characterOwner.planner.StartGOAP(goal.goapEffect, targetPOI, GOAP_CATEGORY.WORK, this, isPersonal);
                //    }
                //}
            }
            return true;
        }
        return base.ProcessJob();
    }
    public override bool CancelJob(bool shouldDoAfterEffect = true, string reason = "") {
        //if (id == -1) { return false; }
        if (assignedCharacter == null) {
            //Can only cancel jobs that are in character job queue
            return false;
        }
        return assignedCharacter.jobQueue.RemoveJobInQueue(this, shouldDoAfterEffect, reason);
        //if (assignedCharacter.jobQueue.RemoveJobInQueue(this, shouldDoAfterEffect, reason)) {
        //    if(reason != "") {
        //        assignedCharacter.RegisterLogAndShowNotifToThisCharacterOnly("Generic", "job_cancelled_cause", null, reason);
        //    }
        //    return true;
        //}
        //return false;
    }
    public override bool ForceCancelJob(bool shouldDoAfterEffect = true, string reason = "") {
        //if (id == -1) { return false; }
        if (assignedCharacter != null) {
            Character assignedCharacter = this.assignedCharacter;
            JOB_OWNER ownerType = originalOwner.ownerType;
            bool hasBeenRemoved = assignedCharacter.jobQueue.RemoveJobInQueue(this, shouldDoAfterEffect, reason);
            //if (hasBeenRemoved) {
            //    if (cause != "") {
            //        assignedCharacter.RegisterLogAndShowNotifToThisCharacterOnly("Generic", "job_cancelled_cause", null, cause);
            //    }
            //}
            if(ownerType == JOB_OWNER.CHARACTER) {
                return hasBeenRemoved;
            }
        }
        if(originalOwner != null) {
            return originalOwner.ForceCancelJob(this);
        } else {
            return true;
        }
    }
    public override void UnassignJob(bool shouldDoAfterEffect, string reason) {
        //if (id == -1) { return; }
        base.UnassignJob(shouldDoAfterEffect, reason);
        if (assignedCharacter != null) {
            if(assignedPlan != null) {
                //assignedCharacter.AdjustIsWaitingForInteraction(1);
                if (assignedCharacter.currentActionNode != null && assignedPlan.currentNode != null 
                    && assignedCharacter.currentActionNode == assignedPlan.currentActualNode) {
                    //if (assignedCharacter.currentParty.icon.isTravelling) {
                    //    if (assignedCharacter.currentParty.icon.travelLine == null) {
                    //        assignedCharacter.marker.StopMovement();
                    //    } else {
                    //        assignedCharacter.currentParty.icon.SetOnArriveAction(() => assignedCharacter.OnArriveAtAreaStopMovement());
                    //    }
                    //}
                    assignedCharacter.StopCurrentActionNode(shouldDoAfterEffect, reason);
                    //if (character.currentActionNode != null) {
                    //    character.SetCurrentActionNode(null);
                    //}
                    //character.DropPlan(assignedPlan);
                }
                //else {
                //    character.DropPlan(assignedPlan);
                //}
                //assignedCharacter.AdjustIsWaitingForInteraction(-1);
                SetAssignedPlan(null);
            }
            //If has assignedCharacter but has no plan yet, the assumption for this is that the assigned character is still processing the plan for this job
            /*Just remove the assigned character and when the plan is received from goap thread, there is a checker there that will check if the assigned character is no longer he/she,
            /that character will scrap the plan that was made*/
            SetAssignedCharacter(null);
        }
    }
    public override void OnAddJobToQueue() {
        if(targetPOI != null) {
            targetPOI.AddJobTargetingThis(this);
        }
    }
    public override bool OnRemoveJobFromQueue() {
        if (originalOwner.ownerType == JOB_OWNER.CHARACTER && assignedPlan == null) { //|| jobQueueParent.character.currentSleepTicks == CharacterManager.Instance.defaultSleepTicks
            //If original owner is character just get the assignedCharacter because for personal jobs, the assignedCharacter is always the owner
            //No need to cast the owner anymore
            if (assignedCharacter != null && id == assignedCharacter.needsComponent.sleepScheduleJobID) {
                //If a character's scheduled sleep job is removed from queue before even doing it, consider it as cancelled 
                assignedCharacter.needsComponent.SetHasCancelledSleepSchedule(true);
            }
        }
        if (targetPOI != null) {
            return targetPOI.RemoveJobTargetingThis(this);
        }
        return false;
    }
    protected override bool CanTakeJob(Character character) {
        if(targetPOI == null) {
            //Debug.Log(jobType.ToString() + " has null target");
            return true;
        }
        if(targetPOI.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            Character target = targetPOI as Character;
            return target.IsInOwnParty();
            //if (target.IsInOwnParty()) {
            //    if (!allowDeadTargets && target.isDead) {
            //        return false;
            //    }
            //} else {
            //    //Must not take job if the target is in another party
            //    return false;
            //}
        }
        if(jobType == JOB_TYPE.REMOVE_STATUS && !string.IsNullOrEmpty(goal.conditionKey) && targetPOI.traitContainer.GetNormalTrait<Trait>((string) goal.conditionKey).IsResponsibleForTrait(character)) {
            return false;
        }
        //if(character.HasTraitOf(TRAIT_TYPE.CRIMINAL) || character.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE)) {
        //    return false;
        //}
        return base.CanTakeJob(character);
    }
    public override bool CanCharacterTakeThisJob(Character character) {
        //if(character == assignedCharacter.owner) {
        //    return CanTakeJob(character);
        //}
        if(originalOwner.ownerType == JOB_OWNER.CHARACTER) {
            //All jobs that are personal will bypass _canTakeThisJob/_canTakeThisJobWithTarget function checkers
            return CanTakeJob(character);
        }
        if (canTakeThis != null) {
            if (canTakeThis(character)) {
                return CanTakeJob(character);
            }
            return false;
        } else if (canTakeThisJob != null) {
            if (canTakeThisJob(character, this)) {
                return CanTakeJob(character);
            }
            return false;
        } else if (canTakeThisJobWithTarget != null && targetPOI != null && targetPOI is Character) {
            if (canTakeThisJobWithTarget(character, targetPOI as Character)) {
                return CanTakeJob(character);
            }
            return false;
        }
        return CanTakeJob(character);
    }
    //public override void OnCharacterAssignedToJob(Character character) {
    //    base.OnCharacterAssignedToJob(character);
    //}
    public override string ToString() {
        return GetJobDetailString();
    }
    public override void AddOtherData(INTERACTION_TYPE actionType, object[] data) {
        Assert.IsFalse(otherData.ContainsKey(actionType), $"Job {name} already has other data for {actionType.ToString()}");
        otherData[actionType] = data;
    }
    public override bool CanBeInterruptedBy(JOB_TYPE jobType) {
        if(assignedPlan != null && assignedPlan.currentActualNode.actionStatus == ACTION_STATUS.PERFORMING) {
            if(jobType == JOB_TYPE.COMBAT) {
                return true;
            }
            return false;
        }
        return base.CanBeInterruptedBy(jobType);
    }
    protected override void CheckJobApplicability(JOB_TYPE jobType, IPointOfInterest targetPOI) {
        if (this.jobType == jobType && this.targetPOI == targetPOI) {
            if (!IsJobStillApplicable()) {
                // ForceCancelJob(false);
                originalOwner.AddForcedCancelJobsOnTickEnded(this);
            }
        }
    }
    protected override void CheckJobApplicability(IPointOfInterest targetPOI) {
        if (this.targetPOI == targetPOI) {
            if (!IsJobStillApplicable()) {
                originalOwner.AddForcedCancelJobsOnTickEnded(this);
                // ForceCancelJob(false);
            }
        }
    }
    #endregion

    #region Forced Actions
    /// <summary>
    /// Add a forced action to this job.
    /// Forced actions are used for plan generation, when a certain action in the plan has a precondition that is in the dictionary,
    /// the plan generation must use the specified action type here.
    /// </summary>
    /// <param name="precondition"></param>
    /// <param name="forcedAction"></param>
    //public void AddForcedInteraction(GoapEffect precondition, INTERACTION_TYPE forcedAction) {
    //    if (!forcedActions.ContainsKey(precondition)) {
    //        forcedActions.Add(precondition, forcedAction);
    //    }
    //}
    //public void ClearForcedActions() {
    //    forcedActions.Clear();
    //}
    #endregion

    #region Plan Constructor
    //public void SetPlanConstructor(System.Func<GoapPlan> planConstructor) {
    //    this.planConstructor = planConstructor;
    //}
    #endregion

    #region Misc
    public void SetAssignedPlan(GoapPlan plan) {
        //if (assignedPlan != null) {
        //    assignedPlan.SetJob(null);
        //}
        //if (plan != null) {
        //    plan.SetJob(this);
        //}
        assignedPlan = plan;
        if(assignedPlan != null) {
            plan.OnAttachPlanToJob(this);
        }
    }
    private void OnArriveAtLocationStopMovement() {
        if (assignedCharacter != null) {
            assignedCharacter.OnArriveAtAreaStopMovement();
        }
    }
    //public void SetWillImmediatelyBeDoneAfterReceivingPlan(bool state) {
    //    willImmediatelyBeDoneAfterReceivingPlan = state;
    //}
    //public void AllowDeadTargets() {
    //    allowDeadTargets = true;
    //}
    /// <summary>
    /// Helper function to get what this job is trying to do.
    /// eg: Specify specific trait when it is Remove Trait job, specify specific item when it is Obtain Item job.
    /// </summary>
    /// <returns>string value to represent what the job detail is (eg. Remove Trait Unconscious)</returns>
    public string GetJobDetailString() {
        switch (jobType) {
            case JOB_TYPE.OBTAIN_PERSONAL_ITEM:
            case JOB_TYPE.REMOVE_STATUS:
                string text = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(jobType.ToString());
                if (!string.IsNullOrEmpty(goal.conditionKey)) {
                    text += $" {UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(goal.conditionKey)}";
                }
                return text;
            case JOB_TYPE.FULLNESS_RECOVERY_NORMAL:
            case JOB_TYPE.FULLNESS_RECOVERY_URGENT:
                return "Hunger Recovery";
            case JOB_TYPE.HAPPINESS_RECOVERY:
                return "Happiness Recovery";
            case JOB_TYPE.ENERGY_RECOVERY_NORMAL:
            case JOB_TYPE.ENERGY_RECOVERY_URGENT:
                return "Tiredness Recovery";
            default:
                if (targetInteractionType != INTERACTION_TYPE.NONE) {
                    return UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(targetInteractionType.ToString());
                } else {
                    return name;
                }
        }
    }
    #endregion

    #region Goap Effects
    public bool HasGoalConditionKey(string key) {
        return goal.conditionKey == key;
        //for (int i = 0; i < goals.Length; i++) {
        //    if (goals[i].goapEffect.conditionKey == key) {
        //        return true;
        //    }
        //}
        //return false;
    }
    public bool HasGoalConditionType(GOAP_EFFECT_CONDITION conditionType) {
        return goal.conditionType == conditionType;
        //for (int i = 0; i < goals.Length; i++) {
        //    if (goals[i].goapEffect.conditionType == conditionType) {
        //        return true;
        //    }
        //}
        //return false;
    }
    #endregion

    #region Job Object Pool
    public override void Reset() {
        base.Reset();
        targetPOI = null;
        targetInteractionType = INTERACTION_TYPE.NONE;
        otherData.Clear();
        allOtherData.Clear();
        SetAssignedPlan(null);
    }
    #endregion
}

//public class ForcedActionsComparer : IEqualityComparer<GoapEffect> {

//    public bool Equals(GoapEffect x, GoapEffect y) {
//        bool matchKeys = false;
//        if(x.conditionKey is int && y.conditionKey is int) {
//            int effectInt = (int) x.conditionKey;
//            int preconditionInt = (int) y.conditionKey;
//            matchKeys = effectInt >= preconditionInt;
//        } else {
//            matchKeys = x.conditionKey == y.conditionKey;
//        }
//        return x.conditionType == y.conditionType && x.targetPOI == y.targetPOI && matchKeys;
//        //if (x.conditionType == y.conditionType) {
//        //    if (string.IsNullOrEmpty(x.conditionString()) || string.IsNullOrEmpty(y.conditionString())) {
//        //        return true;
//        //    } else {
//        //        return x.conditionString() == y.conditionString();
//        //    }
//        //}
//        //return false;
//    }

//    public int GetHashCode(GoapEffect obj) {
//        return obj.GetHashCode();
//    }
//}

public class SaveDataGoapPlanJob : SaveDataJobQueueItem {
    public SaveDataGoapEffect[] goalEffects;
    public int targetPOIID;
    public POINT_OF_INTEREST_TYPE targetPOIType;
    public TILE_OBJECT_TYPE targetPOITileObjectType;
    public INTERACTION_TYPE targetInteractionType;
    public bool allowDeadTargets;

    public List<SaveDataGoapEffect> forcedActionsGoapEffect;
    public List<INTERACTION_TYPE> forcedActionsType;

    public override void Save(JobQueueItem job) {
        base.Save(job);
        GoapPlanJob goapJob = job as GoapPlanJob;
        //allowDeadTargets = goapJob.allowDeadTargets;
        targetInteractionType = goapJob.targetInteractionType;
        if (goapJob.targetPOI != null) {
            targetPOIID = goapJob.targetPOI.id;
            targetPOIType = goapJob.targetPOI.poiType;
            if(goapJob.targetPOI is TileObject) {
                targetPOITileObjectType = (goapJob.targetPOI as TileObject).tileObjectType;
            }
        } else {
            targetPOIID = -1;
        }
        //goalEffects = new SaveDataGoapEffect[goapJob.goals.Length];
        //for (int i = 0; i < goapJob.goals.Length; i++) {
        //    GoapEffect goalEffect = goapJob.goals[i].goapEffect;
        //    SaveDataGoapEffect saveEffect = new SaveDataGoapEffect();
        //    saveEffect.Save(goalEffect);
        //    goalEffects[i] = saveEffect;
        //}


        //forcedActionsGoapEffect = new List<SaveDataGoapEffect>();
        //forcedActionsType = new List<INTERACTION_TYPE>();
        //foreach (KeyValuePair<GoapEffect, INTERACTION_TYPE> kvp in goapJob.forcedActions) {
        //    SaveDataGoapEffect goapEffect = new SaveDataGoapEffect();
        //    goapEffect.Save(kvp.Key);
        //    forcedActionsGoapEffect.Add(goapEffect);

        //    forcedActionsType.Add(kvp.Value);
        //}
    }

    //public override JobQueueItem Load() {
    //    return base.Load();
    //}
}

public interface IGoapJobPremadeNodeCreator {
    IPointOfInterest targetPOI { get; set; }
}

public struct ActionJobPremadeNodeCreator : IGoapJobPremadeNodeCreator {
    public INTERACTION_TYPE actionType;
    public IPointOfInterest targetPOI { get; set; }
}

public struct StateJobPremadeNodeCreator : IGoapJobPremadeNodeCreator {
    public CHARACTER_STATE stateType;
    public IPointOfInterest targetPOI { get; set; }
}