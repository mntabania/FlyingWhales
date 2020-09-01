﻿using UnityEngine.Assertions;
namespace Goap.Job_Checkers {
    public class RemoveStatusTargetApplicabilityChecker : JobApplicabilityChecker {
        public override string key => JobManager.Remove_Status_Target_Applicability;
        public override bool IsJobStillApplicable(JobQueueItem job) {
            GoapPlanJob goapPlanJob = job as GoapPlanJob;
            Assert.IsNotNull(goapPlanJob);
            Character target = goapPlanJob.targetPOI as Character;
            Assert.IsNotNull(target);
            string traitName = goapPlanJob.goal.conditionKey;
            
            if (target.gridTileLocation == null || target.isDead) {
                return false;
            }
            if (!target.traitContainer.HasTrait(traitName)) {
                return false; //target no longer has the given trait
            }
            return true;
        }
    }
}