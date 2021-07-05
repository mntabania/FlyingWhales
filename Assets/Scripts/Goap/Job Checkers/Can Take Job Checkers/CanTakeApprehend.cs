using Traits;
using UnityEngine.Assertions;
namespace Goap.Job_Checkers {
    public class CanTakeApprehend : CanTakeJobChecker {
        public override string key => JobManager.Can_Take_Apprehend;
        public override bool CanTakeJob(Character character, JobQueueItem jobQueueItem) {
            GoapPlanJob goapPlanJob = jobQueueItem as GoapPlanJob;
            Assert.IsNotNull(goapPlanJob);
            Character targetCharacter = goapPlanJob.targetPOI as Character;
            Assert.IsNotNull(targetCharacter);
            if (targetCharacter == character) { return false; }
            return InteractionManager.Instance.CanCharacterTakeApprehendJob(character, targetCharacter);
        }
    }
}