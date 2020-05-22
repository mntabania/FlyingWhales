using Inner_Maps;
using Inner_Maps.Location_Structures;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;

public class PatrolBehaviour : CharacterBehaviourComponent {
    
    public PatrolBehaviour() {
        priority = 450;
    }
    public override bool TryDoBehaviour(Character character, ref string log) {
        LocationStructure chosenStructure = character.currentRegion.GetRandomStructure();
        LocationGridTile chosenTile = chosenStructure.GetRandomTile();
        GoapPlanJob job =
            JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PATROL, INTERACTION_TYPE.PATROL, character, character);
        job.AddOtherData(INTERACTION_TYPE.PATROL, new object[]{ chosenTile });
        job.SetCannotBePushedBack(true);
        character.jobQueue.AddJobInQueue(job);
        return true;
    }

}