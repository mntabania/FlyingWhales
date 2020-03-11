using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BerserkBehaviour : CharacterBehaviourComponent {
    public BerserkBehaviour() {
        priority = 0;
        // attributes = new[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.WITHIN_HOME_SETTLEMENT_ONLY };
    }
    public override bool TryDoBehaviour(Character character, ref string log) {
        log += $"\n-{character.name} is berserked, will only stroll";
        character.PlanIdleStrollOutside();
        return true;
    }
}
