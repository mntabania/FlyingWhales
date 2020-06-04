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
            log += $"\n-{character.name} is in an exterior structure";
            log += "\n-If it is Morning, Lunch Time or Afternoon, 25% chance to enter Stroll Outside Mode";
            TIME_IN_WORDS currentTimeOfDay = GameManager.GetCurrentTimeInWordsOfTick(character);
            if (currentTimeOfDay == TIME_IN_WORDS.MORNING || currentTimeOfDay == TIME_IN_WORDS.LUNCH_TIME || currentTimeOfDay == TIME_IN_WORDS.AFTERNOON) {
                log += $"\n  -Time of Day: {currentTimeOfDay}";
                int chance = UnityEngine.Random.Range(0, 100);
                log += $"\n  -RNG roll: {chance}";
                if (chance < 25) {
                    log += $"\n  -Morning or Afternoon: {character.name} will enter Stroll Outside State";
                    character.PlanIdleStrollOutside(out producedJob); //character.currentStructure
                    return true;
                }
            } else {
                log += $"\n  -Time of Day: {currentTimeOfDay}";
            }
            log += "\n-Otherwise, if character has a Home Structure or Territory, Return Home";
            if((character.homeStructure != null && !character.homeStructure.hasBeenDestroyed) || character.HasTerritory()) {
                log += $"\n  -{character.name} will do action Return Home";
                return character.PlanIdleReturnHome(out producedJob);
            } else {
                log += "\n-Character does not have home structure or territory, 25% chance to set home";
                if (UnityEngine.Random.Range(0, 100) < 25) {
                    log += "\n-Character will set home";
                    character.interruptComponent.TriggerInterrupt(INTERRUPT.Set_Home, null);
                }
                log += "\n-Character will stand nearby";
                return character.jobComponent.TriggerStand(out producedJob);
            }
        } else {
            producedJob = null;
        }
        return false;
    }
}
