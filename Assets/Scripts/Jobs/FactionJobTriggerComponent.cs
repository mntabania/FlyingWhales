﻿using Inner_Maps.Location_Structures;
using Locations.Settlements;

public class FactionJobTriggerComponent : JobTriggerComponent {
    private readonly Faction _owner;

    public FactionJobTriggerComponent(Faction owner) {
        _owner = owner;
    }

    #region Party
    //public bool TriggerCounterattackPartyJob(LocationStructure targetStructure) { //bool forceDoAction = false
    //    // if (!_owner.HasJob(JOB_TYPE.COUNTERATTACK_PARTY)) {
    //        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.COUNTERATTACK_PARTY, INTERACTION_TYPE.COUNTERATTACK_ACTION, null, _owner);
    //        job.AddOtherData(INTERACTION_TYPE.COUNTERATTACK_ACTION, new object[] { targetStructure });
    //        job.SetCanTakeThisJobChecker(JobManager.Can_Take_Counterattack);
    //        _owner.AddToAvailableJobs(job);
    //        return true;
    //    // }
    //    // return false;
    //}
    //public bool TriggerRaidJob(BaseSettlement targetSettlement) { //bool forceDoAction = false
    //    if (!_owner.HasJob(JOB_TYPE.RAID)) {
    //        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.RAID, INTERACTION_TYPE.RAID, null, _owner);
    //        job.AddOtherData(INTERACTION_TYPE.RAID, new object[] { targetSettlement, _owner });
    //        job.SetCanTakeThisJobChecker(JobManager.Can_Take_Raid);
    //        _owner.AddToAvailableJobs(job);
    //        return true;
    //    }
    //    return false;
    //}
    public void TriggerJoinGatheringJob(Gathering gathering) {
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.JOIN_GATHERING, INTERACTION_TYPE.JOIN_GATHERING, gathering.host, _owner);
        job.SetCanTakeThisJobChecker(JobManager.Can_Take_Join_Gathering);
        _owner.AddToAvailableJobs(job);
    }
    public bool TriggerHeirloomHuntJob(Region regionToSearch) { //bool forceDoAction = false
        if (!_owner.HasJob(JOB_TYPE.HUNT_HEIRLOOM)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HUNT_HEIRLOOM, INTERACTION_TYPE.HUNT_HEIRLOOM, _owner.factionHeirloom, _owner);
            job.AddOtherData(INTERACTION_TYPE.HUNT_HEIRLOOM, new object[] { regionToSearch });
            job.SetCanTakeThisJobChecker(JobManager.Can_Take_Hunt_Heirloom);
            _owner.AddToAvailableJobs(job);
            return true;
        }
        return false;
    }
    #endregion
}
