using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Locations.Settlements;

public class PartyJobTriggerComponent : JobTriggerComponent {
    private readonly Party _owner;

    public PartyJobTriggerComponent(Party owner) {
        _owner = owner;
    }

    #region Campc
    public bool CreateBuildCampfireJob(JOB_TYPE jobType) {
        if (!_owner.jobBoard.HasJob(jobType)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.BUILD_CAMPFIRE, null, _owner);
            job.SetDoNotRecalculate(true);
            _owner.jobBoard.AddToAvailableJobs(job);
            return true;
        }
        return false;
    }
    public bool CreateProduceFoodForCampJob() {
        if (!_owner.jobBoard.HasJob(JOB_TYPE.PRODUCE_FOOD_FOR_CAMP)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PRODUCE_FOOD_FOR_CAMP, new GoapEffect(GOAP_EFFECT_CONDITION.PRODUCE_FOOD, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR), null, _owner);
            job.SetDoNotRecalculate(true);
            _owner.jobBoard.AddToAvailableJobs(job);
            return true;
        }
        return false;
    }
    public void CreateHaulForCampJob(ResourcePile target, HexTile hex) {
        if (_owner.jobBoard.HasJob(JOB_TYPE.HAUL, target) == false) {
            if (target.gridTileLocation.collectionOwner.isPartOfParentRegionMap && target.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner == hex) {
                //Only create haul job for camp if resource pile is not in camp
            } else {
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HAUL, new GoapEffect(GOAP_EFFECT_CONDITION.DEPOSIT_RESOURCE, string.Empty, false, GOAP_EFFECT_TARGET.TARGET), target, _owner);
                job.AddOtherData(INTERACTION_TYPE.DEPOSIT_RESOURCE_PILE, new object[] { hex });
                _owner.jobBoard.AddToAvailableJobs(job);
            }
        }
    }
    #endregion
}
