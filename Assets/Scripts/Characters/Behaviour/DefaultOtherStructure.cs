﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultOtherStructure : CharacterBehaviourComponent {
    public DefaultOtherStructure() {
        priority = 8;
        //attributes = new BEHAVIOUR_COMPONENT_ATTRIBUTE[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.WITHIN_HOME_SETTLEMENT_ONLY };
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        if (character.currentStructure.isInterior && character.currentStructure != character.homeStructure
             && character.trapStructure.IsTrapped() == false && character.trapStructure.IsTrappedInArea() == false) {
#if DEBUG_LOG
            log += $"\n-{character.name} is in Interior Structure that is not his/her and he/she is not trapped there";
            log += "\n-If character has a Home Structure or Territory, Return Home";
#endif
            if ((character.homeStructure != null && !character.homeStructure.hasBeenDestroyed) || character.HasTerritory()) {
#if DEBUG_LOG
                log += $"\n  -{character.name} will do action Return Home";
#endif
                return character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
            } else {
                //log += "\n-Character does not have home structure or territory, 25% chance to set home";
                //if (UnityEngine.Random.Range(0, 100) < 25) {
                //    log += "\n-Character will set home";
                //    character.interruptComponent.TriggerInterrupt(INTERRUPT.Set_Home, null);
                //}
#if DEBUG_LOG
                log += "\n-Character will stand nearby";
#endif
                return character.jobComponent.TriggerStand(out producedJob);
            }
        } else {
            producedJob = null;
        }
        return false;
    }
}
