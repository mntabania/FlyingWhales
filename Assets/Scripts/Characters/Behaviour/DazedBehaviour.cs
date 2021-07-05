using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DazedBehaviour : CharacterBehaviourComponent {
    public DazedBehaviour() {
        priority = 10;
        //attributes = new[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.WITHIN_HOME_SETTLEMENT_ONLY };
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
#if DEBUG_LOG
        log += $"\n-{character.name} is dazed, will only return home";
#endif
        return character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
    }
    public override void OnAddBehaviourToCharacter(Character character) {
        base.OnAddBehaviourToCharacter(character);
        character.behaviourComponent.OnBecomeDazed();
    }
    public override void OnRemoveBehaviourFromCharacter(Character character) {
        base.OnRemoveBehaviourFromCharacter(character);
        character.behaviourComponent.OnNoLongerDazed();
    }
    public override void OnLoadBehaviourToCharacter(Character character) {
        base.OnLoadBehaviourToCharacter(character);
        character.behaviourComponent.OnBecomeDazed();
    }
}
