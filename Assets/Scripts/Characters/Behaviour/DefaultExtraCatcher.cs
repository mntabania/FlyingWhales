using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultExtraCatcher : CharacterBehaviourComponent {
    public DefaultExtraCatcher() {
        priority = 0;
        //attributes = new BEHAVIOUR_COMPONENT_ATTRIBUTE[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.OUTSIDE_SETTLEMENT_ONLY };
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        if (!character.IsInHomeSettlement() && character.trapStructure.IsTrapped() == false) {
            log += $"\n-{character.name} is in another npcSettlement and Base Structure is empty";
            log += "\n-100% chance to return home";
            character.PlanIdleReturnHome(out producedJob);
            return true;
        } else {
            producedJob = null;
        }
        return false;
    }
}
