using System.Collections.Generic;
using System.Linq;
using UtilityScripts;

public class CarePlagueBearersBehaviour : CharacterBehaviourComponent {
    
    public CarePlagueBearersBehaviour() {
        priority = 790;
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        if (character.homeSettlement == null || !character.homeSettlement.HasStructure(STRUCTURE_TYPE.HOSPICE)) {
            //cancel care job
            producedJob = null;
            character.traitContainer.RemoveTrait(character, "Plague Caring");
            return false;
        } else {
            List<Character> quarantinedCharacterChoices = new List<Character>();
            for (int i = 0; i < character.homeSettlement.residents.Count; i++) {
                Character resident = character.homeSettlement.residents[i];
                if (IsCharacterQuarantinedAndUnTended(resident)) {
                    quarantinedCharacterChoices.Add(resident);
                }
            }
            if (quarantinedCharacterChoices.Count > 0) {
                Character target = quarantinedCharacterChoices[0];
                if (target.needsComponent.isHungry || target.needsComponent.isStarving) {
                    FoodPile foodPileInMainStorage = character.homeSettlement.mainStorage.GetResourcePileObjectWithLowestCount<FoodPile>();
                    if (foodPileInMainStorage != null && foodPileInMainStorage.resourceInPile >= 12) {
                        //only allow feed job if main storage has enough food for it, this is to prevent characters from being stuck in this behaviour if there are no more food piles.
                        if (character.jobComponent.TryTriggerFeed(target, out producedJob)) {
                            return true;
                        }    
                    }
                } 
                ActualGoapNode node = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.CARE], character, target, null, 0);
                GoapPlan goapPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(node, target);
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PLAGUE_CARE, INTERACTION_TYPE.CARE, target, character);
                goapPlan.SetDoNotRecalculate(true);
                job.SetCannotBePushedBack(true);
                job.SetAssignedPlan(goapPlan);
                producedJob = job;
                return true;
            } else {
                //cancel care job
                producedJob = null;
                character.traitContainer.RemoveTrait(character, "Plague Caring");
                return false;
            }    
        }
    }

    private bool IsCharacterQuarantinedAndUnTended(Character p_character) {
        return p_character.traitContainer.HasTrait("Quarantined") && !p_character.traitContainer.HasTrait("Plague Cared") && 
               p_character.gridTileLocation != null && p_character.gridTileLocation.structure.settlementLocation == p_character.homeSettlement;
    }
}