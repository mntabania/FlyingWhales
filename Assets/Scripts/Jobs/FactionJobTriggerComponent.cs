using Inner_Maps.Location_Structures;

public class FactionJobTriggerComponent : JobTriggerComponent {
    private readonly Faction _owner;

    public FactionJobTriggerComponent(Faction owner) {
        _owner = owner;
    }

    #region Party
    public bool TriggerCounterattackPartyJob(LocationStructure targetStructure) { //bool forceDoAction = false
        if (!_owner.HasJob(JOB_TYPE.COUNTERATTACK_PARTY)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.COUNTERATTACK_PARTY, INTERACTION_TYPE.COUNTERATTACK_ACTION, null, _owner);
            job.AddOtherData(INTERACTION_TYPE.COUNTERATTACK_ACTION, new object[] { targetStructure, _owner });
            job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeCounterattackPartyJob);
            _owner.AddToAvailableJobs(job);
            return true;
        }
        return false;
    }
    public void TriggerJoinPartyJob(Party party) {
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.JOIN_PARTY, INTERACTION_TYPE.JOIN_PARTY, party.leader, _owner);
        job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeJoinPartyJob);
        _owner.AddToAvailableJobs(job);
    }
    #endregion
}
