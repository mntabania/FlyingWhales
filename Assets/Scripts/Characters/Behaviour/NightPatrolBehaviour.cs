using Inner_Maps;
using Inner_Maps.Location_Structures;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;

public class NightPatrolBehaviour : CharacterBehaviourComponent {
    
    public NightPatrolBehaviour() {
        priority = 200;
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        LocationStructure chosenStructure;
        if (character.homeSettlement != null) {
            chosenStructure = character.homeSettlement.GetRandomStructure();
        } else {
            chosenStructure = character.currentRegion.GetRandomStructure();
        }
        LocationGridTile chosenTile = chosenStructure.GetRandomTile();
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PATROL, INTERACTION_TYPE.PATROL, character, character);
        job.AddOtherData(INTERACTION_TYPE.PATROL, new object[]{ chosenTile });
        job.SetCannotBePushedBack(true);
        producedJob = job;    
        return true;
    }

}