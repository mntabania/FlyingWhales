using System.Collections.Generic;
using Locations;
using Locations.Settlements;
using UnityEngine.Assertions;

public class SaveDataNPCSettlement : SaveDataBaseSettlement {
    public string regionID;
    public List<string> jobIDs;
    public List<string> forceCancelJobIDs;
    public string prisonID;
    public string mainStorageID;
    public string rulerID;
    public SaveDataSettlementType settlementType;
    public SaveDataLocationEventManager eventManager;
    public SaveDataSettlementClassTracker classTracker;
    public List<TILE_OBJECT_TYPE> neededObjects;
    public bool hasTriedToStealCorpse;
    public bool isUnderSiege;
    public bool isPlagued;
    public GameDate plaguedExpiry;
    public bool hasPeasants;
    public bool hasWorkers;

    public override void Save(BaseSettlement baseSettlement) {
        base.Save(baseSettlement);
        NPCSettlement npcSettlement = baseSettlement as NPCSettlement;
        Assert.IsNotNull(npcSettlement);
        hasTriedToStealCorpse = npcSettlement.hasTriedToStealCorpse;
        regionID = npcSettlement.region.persistentID;
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
        eventManager = new SaveDataLocationEventManager();
        eventManager.Save(npcSettlement.eventManager);
        
        classTracker = new SaveDataSettlementClassTracker();
        classTracker.Save(npcSettlement.settlementClassTracker);
        
        neededObjects = new List<TILE_OBJECT_TYPE>(npcSettlement.neededObjects);

        isUnderSiege = npcSettlement.isUnderSiege;
        isPlagued = npcSettlement.isPlagued;
        if (isPlagued) {
            plaguedExpiry = npcSettlement.plaguedExpiryDate;
        }
        hasPeasants = npcSettlement.hasPeasants;
        hasWorkers = npcSettlement.hasWorkers;
    }
    public override BaseSettlement Load() {
        return LandmarkManager.Instance.LoadNPCSettlement(this);
    }
}
