using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Inner_Maps;
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
    public void CreateHaulForCampJob(ResourcePile target, Area p_area) {
        if (_owner.jobBoard.HasJob(JOB_TYPE.HAUL, target) == false) {
            if (target.gridTileLocation.area == p_area) {
                //Only create haul job for camp if resource pile is not in camp
            } else {
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HAUL, new GoapEffect(GOAP_EFFECT_CONDITION.DEPOSIT_RESOURCE, string.Empty, false, GOAP_EFFECT_TARGET.TARGET), target, _owner);
                job.AddOtherData(INTERACTION_TYPE.DEPOSIT_RESOURCE_PILE, new object[] { p_area });
                job.SetDoNotRecalculate(true);
                _owner.jobBoard.AddToAvailableJobs(job);
            }
        }
    }
    public bool CreateSnatchJob(Character targetCharacter, LocationGridTile targetLocation, LocationStructure structure) {
        if (_owner.jobBoard.HasJob(JOB_TYPE.SNATCH, targetCharacter) == false) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.SNATCH, INTERACTION_TYPE.DROP_RESTRAINED, targetCharacter, _owner);
            job.SetCanTakeThisJobChecker(JobManager.Can_Take_Snatch_Job);
            job.AddOtherData(INTERACTION_TYPE.DROP_RESTRAINED, new object[] { structure, targetLocation });
            _owner.jobBoard.AddToAvailableJobs(job);
            return true;
        }
        return false;
    }
    #endregion
}
