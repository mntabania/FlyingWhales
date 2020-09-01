using Traits;
using UnityEngine;
using UnityEngine.Assertions;
namespace Goap.Job_Checkers {
    public class CanTakeRemoveStatus : CanTakeJobChecker {
        public override string key => JobManager.Can_Take_Remove_Status;
        public override bool CanTakeJob(Character character, JobQueueItem jobQueueItem) {
            GoapPlanJob goapPlanJob = jobQueueItem as GoapPlanJob;
            Assert.IsNotNull(goapPlanJob);
            Character targetCharacter = goapPlanJob.targetPOI as Character;
            Assert.IsNotNull(targetCharacter);
            string traitName = goapPlanJob.goal.conditionKey;
            Trait trait = targetCharacter.traitContainer.GetNormalTrait<Trait>(traitName);
            if (trait == null) {
                Debug.LogWarning($"{targetCharacter.name} has remove status {traitName} in settlement job queue but does not have that trait!");
                return false;
            }
            bool isNotHostileAndNotDead = !character.IsHostileWith(targetCharacter) && !targetCharacter.isDead;
            bool isResponsibleForTrait = trait.IsResponsibleForTrait(character);

            //if special illness, check if character is healer
            if (TraitManager.Instance.specialIllnessTraits.Contains(trait.name)) {
                return isNotHostileAndNotDead &&
                       character.relationshipContainer.HasOpinionLabelWithCharacter(targetCharacter,
                           RelationshipManager.Rival, RelationshipManager.Enemy) == false 
                       && isResponsibleForTrait == false
                       && !character.traitContainer.HasTrait("Psychopath")
                       && character.traitContainer.HasTrait("Healing Expert");	
            }
				
            return isNotHostileAndNotDead &&
                   character.relationshipContainer.HasOpinionLabelWithCharacter(targetCharacter,
                       RelationshipManager.Rival, RelationshipManager.Enemy) == false 
                   && isResponsibleForTrait == false
                   && !character.traitContainer.HasTrait("Psychopath");
            
        }
    }
}