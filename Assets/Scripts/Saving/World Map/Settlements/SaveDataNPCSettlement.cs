using System.Collections.Generic;
using Locations.Settlements;
using UnityEngine.Assertions;

public class SaveDataNPCSettlement : SaveDataBaseSettlement {
    public List<string> jobIDs;
    public List<string> forceCancelJobIDs;
    public string prisonID;
    public string mainStorageID;
    public string rulerID;
    public SaveDataSettlementType settlementType;
    
    public override void Save(BaseSettlement baseSettlement) {
        base.Save(baseSettlement);
        NPCSettlement npcSettlement = baseSettlement as NPCSettlement;
        Assert.IsNotNull(npcSettlement);
        jobIDs = new List<string>();
        for (int i = 0; i < npcSettlement.availableJobs.Count; i++) {
            JobQueueItem job = npcSettlement.availableJobs[i];
            if(job.jobType != JOB_TYPE.NONE) {
                jobIDs.Add(job.persistentID);
                SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(job);
            }
        }
        forceCancelJobIDs = new List<string>();
        for (int i = 0; i < npcSettlement.forcedCancelJobsOnTickEnded.Count; i++) {
            JobQueueItem job = npcSettlement.forcedCancelJobsOnTickEnded[i];
            if(job.jobType != JOB_TYPE.NONE) {
                forceCancelJobIDs.Add(job.persistentID);
            }
        }
        rulerID = npcSettlement.ruler?.persistentID ?? string.Empty;
        prisonID = npcSettlement.prison != null ? npcSettlement.prison.persistentID : string.Empty;
        mainStorageID = npcSettlement.mainStorage != null ? npcSettlement.mainStorage.persistentID : string.Empty;
        
        if (npcSettlement.settlementType != null) {
            settlementType = new SaveDataSettlementType();
            settlementType.Save(npcSettlement.settlementType);
        }
    }
    public override BaseSettlement Load() {
        return LandmarkManager.Instance.LoadNPCSettlement(this);
    }
}
