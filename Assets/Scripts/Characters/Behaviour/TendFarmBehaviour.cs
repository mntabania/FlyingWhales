using System.Collections.Generic;
using System.Linq;
using UtilityScripts;

public class TendFarmBehaviour : CharacterBehaviourComponent {
    
    public TendFarmBehaviour() {
        priority = 440;
        // attributes = new[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.WITHIN_HOME_SETTLEMENT_ONLY };
    }
    public override bool TryDoBehaviour(Character character, ref string log) {
        List<CornCrop> crops = character.homeSettlement.GetTileObjectsFromStructures<CornCrop>(STRUCTURE_TYPE.FARM, IsCornCropUntended);
        if (crops.Count > 0) {
            CornCrop chosenCrop = crops[0];
            ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.TEND], character, chosenCrop, null, 0);
            GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, chosenCrop);
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.TEND_FARM, INTERACTION_TYPE.TEND, chosenCrop, character);
            goapPlan.SetDoNotRecalculate(true);
            job.SetCannotBePushedBack(true);
            job.SetAssignedPlan(goapPlan);
            
            character.jobQueue.AddJobInQueue(job);
        } else {
            //cancel tend farm job
            character.traitContainer.RemoveTrait(character, "Tending");
        }
        return true;
    }

    private bool IsCornCropUntended(CornCrop crop) {
        return crop.traitContainer.HasTrait("Tended") == false;
    }
}