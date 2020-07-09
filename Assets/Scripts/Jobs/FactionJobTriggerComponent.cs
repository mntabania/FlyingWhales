public class FactionJobTriggerComponent : JobTriggerComponent {
    private readonly Faction _owner;

    public FactionJobTriggerComponent(Faction owner) {
        _owner = owner;
    }

    #region Party
    public void TriggerJoinPartyJob(Party party) {
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.JOIN_PARTY, INTERACTION_TYPE.JOIN_PARTY, party.leader, _owner);
        job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeJoinPartyJob);
        _owner.AddToAvailableJobs(job);
    }
    #endregion
}
