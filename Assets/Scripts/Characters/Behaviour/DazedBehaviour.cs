using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DazedBehaviour : CharacterBehaviourComponent {
    public DazedBehaviour() {
        priority = 0;
        //attributes = new[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.WITHIN_HOME_SETTLEMENT_ONLY };
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        log += $"\n-{character.name} is dazed, will only stroll";
        character.PlanIdleStrollOutside(out producedJob);
        return true;
    }
}
