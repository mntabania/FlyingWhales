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
            if (targetCharacter.traitContainer.HasTrait("Cultist") && character.traitContainer.HasTrait("Cultist")) {
                //if target character is a cultist, only take restrain job if character is not cultist
                return false;  
            }
            if (character.faction != null && character.faction.isPlayerFaction) {
                //if this character is part of the player faction and the other character is allied with the player, then do not consider as hostile
                if (targetCharacter.isAlliedWithPlayer) {
                    return false;
                }
            }
            if (character.isAlliedWithPlayer) {
                //if this character is allied with the player and the other character is part of the player faction, then do not consider as hostile
                if (targetCharacter.faction != null && targetCharacter.faction.isPlayerFaction) {
                    return false;
                }
            }
            return true;
        }
    }
}