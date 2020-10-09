using System;
using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;

public class GoapPlanJob : JobQueueItem {

    public static string Target_Already_Dead_Reason = "target is already dead";
    
    public GoapEffect goal { get; protected set; }
    public GoapPlan assignedPlan { get; protected set; }
    public IPointOfInterest targetPOI { get; protected set; }
    public INTERACTION_TYPE targetInteractionType { get; protected set; } //Only used if the plan to be created uses interaction type
    //if INTERACTION_TYPE is NONE, it means that it is used by all
    public Dictionary<INTERACTION_TYPE, OtherData[]> otherData { get; protected set; } //TODO: Further optimize this by moving this dictionary to the actor itself
    public bool shouldBeCancelledOnDeath { get; private set; } //should this job be cancelled when the target dies?

    #region getters
    public override OBJECT_TYPE objectType => OBJECT_TYPE.Job;
    public override Type serializedData => typeof(SaveDataGoapPlanJob);
    #endregion
    
    public GoapPlanJob() : base() {
        otherData = new Dictionary<INTERACTION_TYPE, OtherData[]>();
    }

    public void Initialize(JOB_TYPE jobType, GoapEffect goal, IPointOfInterest targetPOI, IJobOwner owner) {
        Initialize(jobType, owner);
        this.goal = goal;
        this.targetPOI = targetPOI;
        shouldBeCancelledOnDeath = true;
        if (targetPOI is TileObject tileObject) {
            tileObject.AddExistingJobTargetingThis(this);
        }
    }
    public void Initialize(JOB_TYPE jobType, INTERACTION_TYPE targetInteractionType, IPointOfInterest targetPOI, IJobOwner owner) {
        Initialize(jobType, owner);
        this.targetPOI = targetPOI;
        this.targetInteractionType = targetInteractionType;
        shouldBeCancelledOnDeath = true;
        if (targetPOI is TileObject tileObject) {
            tileObject.AddExistingJobTargetingThis(this);
        }
    }
    public void Initialize(SaveDataGoapPlanJob data) {
        base.Initialize(data);
        goal = data.goal;
        targetInteractionType = data.targetInteractionType;
        shouldBeCancelledOnDeath = data.shouldBeCancelledOnDeath;
    }

    #region Loading
    public override void LoadSecondWave(SaveDataJobQueueItem saveData) {
        base.LoadSecondWave(saveData);
        SaveDataGoapPlanJob data = saveData as SaveDataGoapPlanJob;
        Assert.IsNotNull(data);
        if (!string.IsNullOrEmpty(data.targetPOIID)) {
            if (data.targetPOIObjectType == OBJECT_TYPE.Tile_Object) {
                targetPOI = DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentID(data.targetPOIID);
            } else if (data.targetPOIObjectType == OBJECT_TYPE.Character) {
                targetPOI = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(data.targetPOIID);
            }
        }
        foreach (var saveDataOtherData in data.otherData) {
            OtherData[] loadedOtherData = new OtherData[saveDataOtherData.Value.Length];
            for (int i = 0; i < loadedOtherData.Length; i++) {
                SaveDataOtherData saved = saveDataOtherData.Value[i];
                if (saved != null) {
                    loadedOtherData[i] = saved.Load();    
                }
            }
            otherData.Add(saveDataOtherData.Key, loadedOtherData);
        }
        if (data.saveDataGoapPlan != null) {
            GoapPlan goapPlan = data.saveDataGoapPlan.Load();
            assignedPlan = goapPlan;
        }
    }
    #endregion
    
    
    #region Overrides 
    public override bool ProcessJob() {
        if (hasBeenReset) { return false; }
        if(assignedPlan == null && originalOwner != null && assignedCharacter != null) {
            Character characterOwner = assignedCharacter;
            bool isPersonal = originalOwner.ownerType == JOB_OWNER.CHARACTER;
            IPointOfInterest target = targetPOI ?? assignedCharacter; //if provided target is null, default to the assigned character.
            if (targetInteractionType != INTERACTION_TYPE.NONE) {
                characterOwner.planner.StartGOAP(targetInteractionType, target, this, isPersonal);
            } else {
                characterOwner.planner.StartGOAP(goal, target, this, isPersonal);
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
        if (originalOwner != null && originalOwner.ownerType == JOB_OWNER.CHARACTER && assignedPlan == null) { //|| jobQueueParent.character.currentSleepTicks == CharacterManager.Instance.defaultSleepTicks
            //If original owner is character just get the assignedCharacter because for personal jobs, the assignedCharacter is always the owner
            //No need to cast the owner anymore
            if (assignedCharacter != null && persistentID == assignedCharacter.needsComponent.sleepScheduleJobID) {
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
            return target.carryComponent.IsNotBeingCarried();
        }
        if(jobType == JOB_TYPE.REMOVE_STATUS && !string.IsNullOrEmpty(goal.conditionKey) && targetPOI.traitContainer.GetTraitOrStatus<Trait>((string) goal.conditionKey).IsResponsibleForTrait(character)) {
            return false;
        }
        return base.CanTakeJob(character);
    }
    public override string ToString() {
        return GetJobDetailString();
    }
    public override void AddOtherData(INTERACTION_TYPE actionType, object[] data) {
        Assert.IsFalse(otherData.ContainsKey(actionType), $"Job {name} already has other data for {actionType.ToString()}");
        OtherData[] convertedDataArray = new OtherData[data.Length];
        for (int i = 0; i < data.Length; i++) {
            object obj = data[i];
            OtherData convertedData = null;
            if (obj is LocationGridTile locationGridTile) {
                convertedData = new LocationGridTileOtherData(locationGridTile);
            } else if (obj is LocationStructure locationStructure) {
                convertedData = new LocationStructureOtherData(locationStructure);
            } else if (obj is HexTile hexTile) {
                convertedData = new HexTileOtherData(hexTile);
            } else if (obj is int integer) {
                convertedData = new IntOtherData(integer);
            } else if (obj is TileObject tileObject) {
                convertedData = new TileObjectOtherData(tileObject);
            } else if (obj is Region region) {
                convertedData = new RegionOtherData(region);
            } else if (obj is BaseSettlement settlement) {
                convertedData = new SettlementOtherData(settlement);
            } else if (obj is Faction faction) {
                convertedData = new FactionOtherData(faction);
            } else if (obj is CrimeData crimeData) {
                convertedData = new CrimeDataOtherData(crimeData);
            } else if (obj is ICrimeable crimeable) {
                convertedData = new CrimeableOtherData(crimeable);
            } else if (obj is ActualGoapNode actualGoapNode) {
                convertedData = new ActualGoapNodeOtherData(actualGoapNode);
            } else if (obj is Rumor rumor) {
                convertedData = new RumorOtherData(rumor);
            } else if (obj is Character character) {
                convertedData = new CharacterOtherData(character);
            } else if (obj is string str) {
                convertedData = new StringOtherData(str);
            }
            if (convertedData != null) {
                convertedDataArray[i] = convertedData;    
            } 
            // else {
            //     throw new Exception($"No Other Data class type for {obj}");
            // }
        }
        otherData[actionType] = convertedDataArray;
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

    #region Misc
    public void SetAssignedPlan(GoapPlan plan) {
        //if (assignedPlan != plan) {
        //    GoapPlan prevPlan = assignedPlan;
        //    assignedPlan = plan;
        //    if (prevPlan != null) {
        //        prevPlan.OnUnattachPlanToJob(this);
        //    }
        //    if (plan != null) {
        //        plan.OnAttachPlanToJob(this);
        //    }
        //}
        GoapPlan prevPlan = assignedPlan;
        assignedPlan = plan;
        if (plan != null) {
            plan.OnAttachPlanToJob(this);
        } else {
            if (prevPlan != null) {
                prevPlan.OnUnattachPlanToJob(this);
            }
        }
    }
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
    public void SetCancelOnDeath(bool state) {
        shouldBeCancelledOnDeath = state;
    }
    public bool HasOtherData(INTERACTION_TYPE actionType) {
        return otherData.ContainsKey(actionType);
    }
    public OtherData[] GetOtherData(INTERACTION_TYPE actionType) {
        if (HasOtherData(actionType)) {
            return otherData[actionType];
        }
        return null;
    }
    #endregion

    #region Goap Effects
    public bool HasGoalConditionKey(string key) {
        return goal.conditionKey == key;
    }
    public bool HasGoalConditionType(GOAP_EFFECT_CONDITION conditionType) {
        return goal.conditionType == conditionType;
    }
    #endregion

    #region Job Object Pool
    public override void Reset() {
        if (targetPOI != null) {
            targetPOI.RemoveJobTargetingThis(this);
            if (targetPOI is TileObject tileObject) {
                tileObject.RemoveExistingJobTargetingThis(this);
            }
        }
        base.Reset();
        goal.Reset();
        targetPOI = null;
        targetInteractionType = INTERACTION_TYPE.NONE;
        otherData.Clear();
        SetAssignedPlan(null);
        shouldBeCancelledOnDeath = true;
    }
    #endregion
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