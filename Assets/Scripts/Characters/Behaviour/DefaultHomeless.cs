using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;

public class DefaultHomeless : CharacterBehaviourComponent {
    public DefaultHomeless() {
        priority = 10;
        attributes = new BEHAVIOUR_COMPONENT_ATTRIBUTE[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.DO_NOT_SKIP_PROCESSING/*, BEHAVIOUR_COMPONENT_ATTRIBUTE.ONCE_PER_DAY*/ };
    }

    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        if (character.homeStructure == null || character.homeStructure.hasBeenDestroyed) {
            log += $"\n-{character.name} is homeless, 25% chance to find home";
            if (UnityEngine.Random.Range(0, 100) < 25) {
                log += $"\n-Character will try to set home";
                character.interruptComponent.TriggerInterrupt(INTERRUPT.Set_Home, null);
                return true;
            }
        }
        return false;
    }
}
