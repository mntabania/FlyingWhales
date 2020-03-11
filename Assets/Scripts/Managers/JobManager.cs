using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JobManager : MonoBehaviour {
    public static JobManager Instance;

    private List<GoapPlanJob> goapJobPool;
    private List<CharacterStateJob> stateJobPool;

    void Awake() {
        Instance = this;
    }

    public void Initialize() {
        ConstructInitialJobPool();
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
        if (job is GoapPlanJob) {
            ReturnGoapPlanJobToPool(job as GoapPlanJob);
        } else { //if (job is CharacterStateJob) 
            ReturnCharacterStateJobToPool(job as CharacterStateJob);
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
    public CharacterStateJob CreateNewCharacterStateJob(JOB_TYPE jobType, CHARACTER_STATE state, Region targetRegion, IJobOwner owner) {
        CharacterStateJob job = GetCharacterStateJobFromPool();
        job.Initialize(jobType, state, targetRegion, owner);
        return job;
    }
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
}
