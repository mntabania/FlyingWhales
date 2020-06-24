using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultExtraCatcher : CharacterBehaviourComponent {
    public DefaultExtraCatcher() {
        priority = 0;
        //attributes = new BEHAVIOUR_COMPONENT_ATTRIBUTE[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.OUTSIDE_SETTLEMENT_ONLY };
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        log += $"\n-{character.name} is in default extra catcher behaviour";
        //if((character.homeStructure == null || character.homeStructure.hasBeenDestroyed) && !character.HasTerritory()) {
        //    log += "\n-Character does not have home structure or territory, 25% chance to set home";
        //    if(UnityEngine.Random.Range(0, 100) < 25) {
        //        log += "\n-Character will set home";
        //        character.interruptComponent.TriggerInterrupt(INTERRUPT.Set_Home, null);
        //    }
        //}
        log += "\n-Character will stand nearby";
        return character.jobComponent.TriggerStand(out producedJob);
    }
}
