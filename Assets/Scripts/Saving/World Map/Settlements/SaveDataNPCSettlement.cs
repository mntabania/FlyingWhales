using System.Collections.Generic;
using Locations.Settlements;
using UnityEngine.Assertions;

public class SaveDataNPCSettlement : SaveDataBaseSettlement {
    public List<SaveDataJobQueueItem> jobs;
    public string prisonID;
    public string mainStorageID;
    
    public override void Save(BaseSettlement baseSettlement) {
        base.Save(baseSettlement);
        NPCSettlement npcSettlement = baseSettlement as NPCSettlement;
        Assert.IsNotNull(npcSettlement);
        jobs = new List<SaveDataJobQueueItem>();
        // for (int i = 0; i < baseSettlement.availableJobs.Count; i++) {
        //     JobQueueItem job = baseSettlement.availableJobs[i];
        //     if (job.isNotSavable) {
        //         continue;
        //     }
        //     //SaveDataJobQueueItem data = System.Activator.CreateInstance(System.Type.GetType("SaveData" + job.GetType().ToString())) as SaveDataJobQueueItem;
        //     SaveDataJobQueueItem data = null;
        //     if (job is GoapPlanJob) {
        //         data = new SaveDataGoapPlanJob();
        //     } else if (job is CharacterStateJob) {
        //         data = new SaveDataCharacterStateJob();
        //     }
        //     data.Save(job);
        //     jobs.Add(data);
        // }
        prisonID = npcSettlement.prison != null ? npcSettlement.prison.persistentID : string.Empty;
        mainStorageID = npcSettlement.mainStorage != null ? npcSettlement.mainStorage.persistentID : string.Empty;
    }
    public override BaseSettlement Load() {
        return LandmarkManager.Instance.LoadNPCSettlement(this);
    }
}
