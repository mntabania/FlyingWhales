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
            if (character.isAtHomeRegion && !character.traitContainer.HasTrait("Criminal") && !character.traitContainer.HasTrait("Coward") && 
                character.homeSettlement != null && character.homeSettlement.prison != null) {
                Criminal criminalTrait = targetCharacter.traitContainer.GetTraitOrStatus<Criminal>("Criminal");
                if (criminalTrait == null || !criminalTrait.isImprisoned) {
                    if (character.relationshipContainer.IsFriendsWith(targetCharacter)) {
                        return false;
                    } else if ((character.relationshipContainer.IsFamilyMember(targetCharacter) || character.relationshipContainer.HasRelationshipWith(targetCharacter, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR))
                               && !character.relationshipContainer.IsEnemiesWith(targetCharacter)) {
                        return false;
                    }
                    return true;
                }
            }
            return false;
        }
    }
}