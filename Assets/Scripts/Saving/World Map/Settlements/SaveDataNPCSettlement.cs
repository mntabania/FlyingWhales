using System.Collections.Generic;
using Locations.Settlements;
using UnityEngine.Assertions;

public class SaveDataNPCSettlement : SaveDataBaseSettlement {
    public List<string> jobIDs;
    public string prisonID;
    public string mainStorageID;
    
    public override void Save(BaseSettlement baseSettlement) {
        base.Save(baseSettlement);
        NPCSettlement npcSettlement = baseSettlement as NPCSettlement;
        Assert.IsNotNull(npcSettlement);
        jobIDs = new List<string>();
        for (int i = 0; i < npcSettlement.availableJobs.Count; i++) {
            JobQueueItem job = npcSettlement.availableJobs[i];    
            jobIDs.Add(job.persistentID);
            SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(job);
        }
        prisonID = npcSettlement.prison != null ? npcSettlement.prison.persistentID : string.Empty;
        mainStorageID = npcSettlement.mainStorage != null ? npcSettlement.mainStorage.persistentID : string.Empty;
    }
    public override BaseSettlement Load() {
        return LandmarkManager.Instance.LoadNPCSettlement(this);
    }
}
