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
    public bool isAssigned;
    public SaveDataGoapPlan saveDataGoapPlan;
    public Dictionary<INTERACTION_TYPE, List<ILocationSaveData>> priorityLocations { get; private set; }

    public override void Save(JobQueueItem job) {
        base.Save(job);
        GoapPlanJob goapJob = job as GoapPlanJob;
        Assert.IsNotNull(goapJob);
        goal = goapJob.goal;
        targetPOIID = goapJob.targetPOI == null ? string.Empty : goapJob.targetPOI.persistentID;
        targetPOIObjectType = goapJob.targetPOI == null ? OBJECT_TYPE.Character : goapJob.targetPOI.objectType;
        targetInteractionType = goapJob.targetInteractionType;
        isAssigned = goapJob.isAssigned;

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

        if(goapJob.priorityLocations != null) {
            priorityLocations = new Dictionary<INTERACTION_TYPE, List<ILocationSaveData>>();
            foreach (KeyValuePair<INTERACTION_TYPE, List<ILocation>> item in goapJob.priorityLocations) {
                if(item.Value != null) {
                    priorityLocations.Add(item.Key, new List<ILocationSaveData>());
                    for (int i = 0; i < item.Value.Count; i++) {
                        ILocation ilocation = item.Value[i];
                        ILocationSaveData data = new ILocationSaveData() {
                            persistentID = ilocation.persistentID,
                            objectType = ilocation.objectType
                        };
                        priorityLocations[item.Key].Add(data);
                    }
                }
            }
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