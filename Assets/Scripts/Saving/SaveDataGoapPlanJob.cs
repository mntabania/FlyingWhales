using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

public class SaveDataGoapPlanJob : SaveDataJobQueueItem {
    public GoapEffect goal;
    public string targetPOIID;
    public OBJECT_TYPE targetPOIObjectType;
    public INTERACTION_TYPE targetInteractionType;
    public Dictionary<INTERACTION_TYPE, SaveDataOtherData[]> otherData;
    public bool shouldBeCancelledOnDeath;
    public SaveDataGoapPlan saveDataGoapPlan;

    public override void Save(JobQueueItem job) {
        base.Save(job);
        GoapPlanJob goapJob = job as GoapPlanJob;
        Assert.IsNotNull(goapJob);
        goal = goapJob.goal;
        targetPOIID = goapJob.targetPOI == null ? string.Empty : goapJob.targetPOI.persistentID;
        targetPOIObjectType = goapJob.targetPOI == null ? OBJECT_TYPE.Character : goapJob.targetPOI.objectType;
        targetInteractionType = goapJob.targetInteractionType;

        otherData = new Dictionary<INTERACTION_TYPE, SaveDataOtherData[]>();
        foreach (var data in goapJob.otherData) {
            SaveDataOtherData[] otherDataSave = new SaveDataOtherData[data.Value.Length];
            for (int i = 0; i < data.Value.Length; i++) {
                OtherData baseOtherData = data.Value[i];
                if (baseOtherData != null) {
                    otherDataSave[i] = baseOtherData.Save();    
                }
            }
            otherData.Add(data.Key, otherDataSave);
        }
        shouldBeCancelledOnDeath = goapJob.shouldBeCancelledOnDeath;
        if (goapJob.assignedPlan != null) {
            saveDataGoapPlan = new SaveDataGoapPlan();
            saveDataGoapPlan.Save(goapJob.assignedPlan);
        }
    }
    public override JobQueueItem Load() {
        GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(this);
        return goapPlanJob;
    }
}