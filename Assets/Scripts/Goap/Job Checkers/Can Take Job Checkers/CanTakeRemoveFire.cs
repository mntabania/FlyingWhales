using UnityEngine.Assertions;
namespace Goap.Job_Checkers {
    public class CanTakeRemoveFire : CanTakeJobChecker {
        public override string key => JobManager.Can_Take_Remove_Fire;
        public override bool CanTakeJob(Character character, JobQueueItem jobQueueItem) {
            GoapPlanJob goapPlanJob = jobQueueItem as GoapPlanJob;
            Assert.IsNotNull(goapPlanJob);
            if (character.jobQueue.HasJob(JOB_TYPE.DOUSE_FIRE)) { return false; }
            if (goapPlanJob.targetPOI is Character targetCharacter) {
                if (character == targetCharacter) {
                    //the burning character is himself
                    return HasWaterAvailable(character);
                } else {
                    //if burning character is other character, make sure that the character that will do the job is not burning.
                    return !character.traitContainer.HasTrait("Burning", "Pyrophobic") 
                           && !character.relationshipContainer.IsEnemiesWith(targetCharacter)
                           && HasWaterAvailable(character);
                }
            } else {
                //make sure that the character that will do the job is not burning.
                return !character.traitContainer.HasTrait("Burning", "Pyrophobic") && HasWaterAvailable(character);
            }
        }
        
        private bool HasWaterAvailable(Character character) {
            return character.currentRegion.HasTileObjectOfType(TILE_OBJECT_TYPE.WATER_WELL);
        }
    }
}