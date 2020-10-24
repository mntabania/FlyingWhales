using System;
using System.Collections;
using System.Collections.Generic;
using Goap.Job_Checkers;
using UnityEngine;

public class JobManager : BaseMonoBehaviour {
    public static JobManager Instance;

    private List<GoapPlanJob> goapJobPool;
    private List<CharacterStateJob> stateJobPool;

    private Dictionary<string, CanTakeJobChecker> _canTakeJobCheckers;
    private Dictionary<string, JobApplicabilityChecker> _applicabilityCheckers;
    
    public const string Can_Take_Bury_Job = "CanTakeBury";
    public const string Can_Take_Join_Gathering = "CanTakeJoinGathering";
    public const string Can_Take_Remove_Status = "CanTakeRemoveStatus";
    public const string Can_Take_Counterattack = "CanTakeCounterattack";
    public const string Can_Take_Raid = "CanTakeRaid";
    public const string Can_Take_Hunt_Heirloom = "CanTakeHuntHeirloom";
    public const string Can_Take_Repair = "CanTakeRepair";
    public const string Can_Take_Haul = "CanTakeHaul";
    public const string Can_Take_Judgement = "CanTakeJudgement";
    public const string Can_Take_Apprehend = "CanTakeApprehend";
    public const string Can_Take_Obtain_Personal_Food = "CanTakeObtainPersonalFood";
    public const string Can_Take_Restrain = "CanTakeRestrain";
    public const string Can_Take_Remove_Fire = "CanTakeRemoveFire";
    public const string Can_Take_Exterminate = "CanTakeExterminate";
    public const string Can_Brew_Potion = "CanBrewPotion";
    public const string Can_Craft_Tool = "CanCraftTool";
    public const string Can_Brew_Antidote = "CanBrewAntidote";
    public const string Can_Craft_Well = "CanCraftWell";
    public const string Can_Craft_Phylactery = "CanCraftPhylactery";
    public const string Can_Steal_Corpse = "CanStealCorpse";
    
    //applicability
    public const string Destroy_Applicability = "IsDestroyApplicable";
    public const string Remove_Status_Applicability = "IsRemoveStatusApplicable";
    public const string Remove_Status_Self_Applicability = "IsRemoveStatusSelfApplicable";
    public const string Remove_Status_Target_Applicability = "IsRemoveStatusTargetApplicable";
    public const string Heal_Self_Applicability = "IsHealSelfApplicable";
    public const string Bury_Settlement_Applicability = "IsBurySettlementApplicable";
    public const string Bury_Applicability = "IsBuryApplicable";
    public const string Apprehend_Applicability = "IsApprehendApplicable";
    public const string Produce_Resource_Applicability = "IsProduceResourceApplicable";
    public const string Repair_Applicability = "IsRepairApplicable";
    public const string Haul_Applicability = "IsHaulApplicable";
    public const string Judge_Applicability = "IsJudgeApplicable";
    public const string Apprehend_Settlement_Applicability = "IsApprehendSettlementApplicable";
    public const string Obtain_Personal_Food_Applicability = "IsObtainPersonalFoodApplicable";
    public const string Combine_Stockpile_Applicability = "IsCombineStockpileApplicable";
    public const string Restrain_Applicability = "IsRestrainApplicable";
    void Awake() {
        Instance = this;
    }
    protected override void OnDestroy() {
        base.OnDestroy();
        Instance = null;
    }

    public void Initialize() {
        ConstructInitialJobPool();
        _canTakeJobCheckers = new Dictionary<string, CanTakeJobChecker>() {
            {Can_Take_Join_Gathering, new CanTakeJoinGathering()},
            {Can_Take_Remove_Status, new CanTakeRemoveStatus()},
            {Can_Take_Bury_Job, new CanTakeBuryJob()},
            {Can_Take_Counterattack, new CanTakeCounterattackJob()},
            {Can_Take_Raid, new CanTakeRaidJob()},
            {Can_Take_Hunt_Heirloom, new CanTakeHuntHeirloomJob()},
            {Can_Take_Repair, new CanTakeRepairJob()},
            {Can_Take_Haul, new CanTakeHaulJob()},
            {Can_Take_Judgement, new CanTakeJudgement()},
            {Can_Take_Apprehend, new CanTakeApprehend()},
            {Can_Take_Obtain_Personal_Food, new CanTakeObtainPersonalFood()},
            {Can_Take_Restrain, new CanTakeRestrainJob()},
            {Can_Take_Remove_Fire, new CanTakeRemoveFire()},
            {Can_Take_Exterminate, new CanTakeExterminateJob()},
            {Can_Brew_Potion, new CanBrewPotion()},
            {Can_Craft_Tool, new CanCraftTool()},
            {Can_Brew_Antidote, new CanBrewAntidote()},
            {Can_Craft_Well, new CanCraftWell()},
            {Can_Craft_Phylactery, new CanCraftPhylactery()},
        };
        _applicabilityCheckers = new Dictionary<string, JobApplicabilityChecker>() {
            {Destroy_Applicability, new DestroyJobApplicabilityChecker()},
            {Remove_Status_Applicability, new RemoveStatusApplicabilityChecker()},
            {Remove_Status_Self_Applicability, new RemoveStatusSelfApplicabilityChecker()},
            {Remove_Status_Target_Applicability, new RemoveStatusTargetApplicabilityChecker()},
            {Heal_Self_Applicability, new HealSelfApplicabilityChecker()},
            {Bury_Settlement_Applicability, new BurySettlementApplicabilityChecker()},
            {Bury_Applicability, new BuryApplicabilityChecker()},
            {Apprehend_Applicability, new ApprehendApplicabilityChecker()},
            {Produce_Resource_Applicability, new ProduceResourceApplicabilityChecker()},
            {Repair_Applicability, new RepairApplicabilityChecker()},
            {Haul_Applicability, new HaulApplicabilityChecker()},
            {Judge_Applicability, new JudgeApplicabilityChecker()},
            {Apprehend_Settlement_Applicability, new ApprehendSettlementApplicabilityChecker()},
            {Obtain_Personal_Food_Applicability, new ObtainPersonalFoodApplicabilityChecker()},
            {Combine_Stockpile_Applicability, new CombineStockpileApplicabilityChecker()},
            {Restrain_Applicability, new RestrainApplicabilityChecker()},
        };
    }

    private void ConstructInitialJobPool() {
        goapJobPool = new List<GoapPlanJob>();
        stateJobPool = new List<CharacterStateJob>();
        for (int i = 0; i < 20; i++) {
            goapJobPool.Add(new GoapPlanJob());
            stateJobPool.Add(new CharacterStateJob());
        }
    }
    public void OnFinishJob(JobQueueItem job) {
        if (job is GoapPlanJob planJob) {
            ReturnGoapPlanJobToPool(planJob);
        } else if (job is CharacterStateJob stateJob) { //
            ReturnCharacterStateJobToPool(stateJob);
        }
    }

    #region Goap Plan Job
    public GoapPlanJob CreateNewGoapPlanJob(JOB_TYPE jobType, GoapEffect goal, IPointOfInterest targetPOI, IJobOwner owner) {
        GoapPlanJob job = GetGoapPlanJobFromPool();
        job.Initialize(jobType, goal, targetPOI, owner);
        return job;
    }
    //public GoapPlanJob CreateNewGoapPlanJob(JOB_TYPE jobType, GoapEffect goal, IPointOfInterest targetPOI, Dictionary<INTERACTION_TYPE, object[]> otherData, IJobOwner owner) {
    //    GoapPlanJob job = new GoapPlanJob(jobType, goal, targetPOI, otherData, owner);
    //    job.Initialize(jobType, goal, targetPOI, owner);
    //    return job;
    //}
    public GoapPlanJob CreateNewGoapPlanJob(JOB_TYPE jobType, INTERACTION_TYPE targetInteractionType, IPointOfInterest targetPOI, IJobOwner owner) {
        GoapPlanJob job = GetGoapPlanJobFromPool();
        job.Initialize(jobType, targetInteractionType, targetPOI, owner);
        return job;
    }
    //public GoapPlanJob CreateNewGoapPlanJob(JOB_TYPE jobType, INTERACTION_TYPE targetInteractionType, IPointOfInterest targetPOI, Dictionary<INTERACTION_TYPE, object[]> otherData, IJobOwner owner) {
    //    GoapPlanJob job = new GoapPlanJob(jobType, targetInteractionType, targetPOI, otherData, owner);
    //    job.Initialize(jobType, goal, targetPOI, owner);
    //    return job;
    //}
    public GoapPlanJob CreateNewGoapPlanJob(SaveDataGoapPlanJob data) {
        GoapPlanJob job = GetGoapPlanJobFromPool();
        job.Initialize(data);
        return job;
    }
    private GoapPlanJob GetGoapPlanJobFromPool() {
        if(goapJobPool.Count > 0) {
            GoapPlanJob job = goapJobPool[0];
            goapJobPool.RemoveAt(0);
            return job;
        }
        return new GoapPlanJob();
    }
    private void ReturnGoapPlanJobToPool(GoapPlanJob job) {
        job.Reset();
        goapJobPool.Add(job);
    }
    #endregion

    #region Character State Jobs
    public CharacterStateJob CreateNewCharacterStateJob(JOB_TYPE jobType, CHARACTER_STATE state, IPointOfInterest targetPOI, IJobOwner owner) {
        CharacterStateJob job = GetCharacterStateJobFromPool();
        job.Initialize(jobType, state, targetPOI, owner);
        return job;
    }
    public CharacterStateJob CreateNewCharacterStateJob(JOB_TYPE jobType, CHARACTER_STATE state, IJobOwner owner) {
        CharacterStateJob job = GetCharacterStateJobFromPool();
        job.Initialize(jobType, state, owner);
        return job;
    }
    public CharacterStateJob CreateNewCharacterStateJob(SaveDataCharacterStateJob data) {
        CharacterStateJob job = GetCharacterStateJobFromPool();
        job.Initialize(data);
        return job;
    }

    private CharacterStateJob GetCharacterStateJobFromPool() {
        if (stateJobPool.Count > 0) {
            CharacterStateJob job = stateJobPool[0];
            stateJobPool.RemoveAt(0);
            return job;
        }
        return new CharacterStateJob();
    }
    private void ReturnCharacterStateJobToPool(CharacterStateJob job) {
        job.Reset();
        stateJobPool.Add(job);
    }
    #endregion

    #region Job Checkers
    public CanTakeJobChecker GetJobChecker(string key) {
        if (_canTakeJobCheckers.ContainsKey(key)) {
            return _canTakeJobCheckers[key];
        }
        throw new Exception($"Could not find job checker with key {key}");
    }
    #endregion
    
    #region Applicability Checkers
    public JobApplicabilityChecker GetApplicabilityChecker(string key) {
        if (_applicabilityCheckers.ContainsKey(key)) {
            return _applicabilityCheckers[key];
        }
        throw new Exception($"Could not find applicability checker with key {key}");
    }
    #endregion
}
