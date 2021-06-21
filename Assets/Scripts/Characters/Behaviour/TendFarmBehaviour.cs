using System.Collections.Generic;
using System.Linq;
using UtilityScripts;

public class TendFarmBehaviour : CharacterBehaviourComponent {
    
    public TendFarmBehaviour() {
        priority = 440;
        // attributes = new[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.WITHIN_HOME_SETTLEMENT_ONLY };
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        if (character.homeSettlement == null) {
            //cancel tend farm job
            producedJob = null;
            character.traitContainer.RemoveTrait(character, "Tending");
        } else {
            CornCrop crop = character.homeSettlement.GetFirstTileObjectFromStructuresThatIsUntended<CornCrop>(STRUCTURE_TYPE.FARM);
            if (crop != null) {
                CornCrop chosenCrop = crop;
                ActualGoapNode node = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.TEND], character, chosenCrop, null, 0);
                GoapPlan goapPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(node, chosenCrop);
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.TEND_FARM, INTERACTION_TYPE.TEND, chosenCrop, character);
                goapPlan.SetDoNotRecalculate(true);
                job.SetCannotBePushedBack(true);
                job.SetAssignedPlan(goapPlan);
                producedJob = job;
            } else {
                //cancel tend farm job
                producedJob = null;
                character.traitContainer.RemoveTrait(character, "Tending");
            }    
        }
        
        return true;
    }

    private bool IsCornCropUntended(CornCrop crop) {
        return crop.traitContainer.HasTrait("Tended") == false && crop.state == POI_STATE.ACTIVE;
    }
}