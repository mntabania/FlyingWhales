using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GluttonBehaviour : CharacterBehaviourComponent {
    public GluttonBehaviour() {
        priority = 15;
        //attributes = new[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.WITHIN_HOME_SETTLEMENT_ONLY };
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        int chance = UnityEngine.Random.Range(0, 100);
#if DEBUG_LOG
        log += $"\n-{character.name} is glutton, 15% chance to add Fullness Recovery Normal Job if Hungry";
        log += $"\n  -RNG roll: {chance}";
#endif
        if (chance < 15) {
            if (character.needsComponent.isHungry) {
                character.needsComponent.PlanFullnessRecoveryGlutton(out producedJob);
                return true;
            }
        }
        producedJob = null;
        return false;
    }
}