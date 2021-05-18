using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultOutside : CharacterBehaviourComponent {
    public DefaultOutside() {
        priority = 4;
        //attributes = new BEHAVIOUR_COMPONENT_ATTRIBUTE[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.WITHIN_HOME_SETTLEMENT_ONLY };
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        if (!character.currentStructure.isInterior) {
#if DEBUG_LOG
            log += $"\n-{character.name} is in an exterior structure";
            log += "\n-If it is Morning, Lunch Time or Afternoon, 25% chance to enter Stroll Outside Mode";
#endif
            TIME_IN_WORDS currentTimeOfDay = GameManager.Instance.GetCurrentTimeInWordsOfTick(character);
            if (currentTimeOfDay == TIME_IN_WORDS.MORNING || currentTimeOfDay == TIME_IN_WORDS.LUNCH_TIME || currentTimeOfDay == TIME_IN_WORDS.AFTERNOON) {
                int chance = UnityEngine.Random.Range(0, 100);
#if DEBUG_LOG
                log += $"\n  -Time of Day: {currentTimeOfDay}";
                log += $"\n  -RNG roll: {chance}";
#endif
                if (chance < 25) {
#if DEBUG_LOG
                    log += $"\n  -Morning or Afternoon: {character.name} will enter Stroll Outside State";
#endif
                    character.jobComponent.PlanIdleStrollOutside(out producedJob); //character.currentStructure
                    return true;
                }
            } else {
#if DEBUG_LOG
                log += $"\n  -Time of Day: {currentTimeOfDay}";
#endif
            }
#if DEBUG_LOG
            log += "\n-Otherwise, if character has a Home Structure or Territory, Return Home";
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
