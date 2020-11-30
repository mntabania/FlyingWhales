using UnityEngine.Assertions;
namespace Goap.Job_Checkers {
    public class CanTakeRestrainJob : CanTakeJobChecker {
        public override string key => JobManager.Can_Take_Restrain;
        public override bool CanTakeJob(Character character, JobQueueItem jobQueueItem) {
            GoapPlanJob goapPlanJob = jobQueueItem as GoapPlanJob;
            Assert.IsNotNull(goapPlanJob);
            Character targetCharacter = goapPlanJob.targetPOI as Character;
            Assert.IsNotNull(targetCharacter);
            if (targetCharacter.traitContainer.HasTrait("Restrained")) {
                return false;
            }
            if (targetCharacter.traitContainer.HasTrait("Cultist")) {
                //if target character is a cultist, only take restrain job if character is not cultist
                return !character.traitContainer.HasTrait("Cultist");  
            }
            return true;
        }
    }
}