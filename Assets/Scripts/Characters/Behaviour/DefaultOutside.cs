using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultOutside : CharacterBehaviourComponent {
    public DefaultOutside() {
        priority = 4;
        //attributes = new BEHAVIOUR_COMPONENT_ATTRIBUTE[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.WITHIN_HOME_SETTLEMENT_ONLY };
    }
    public override bool TryDoBehaviour(Character character, ref string log) {
        if (!character.currentStructure.isInterior && character.isAtHomeRegion) {
            // if ((character.currentStructure.structureType == STRUCTURE_TYPE.WORK_AREA || character.currentStructure.structureType == STRUCTURE_TYPE.WILDERNESS || 
            //      character.currentStructure.structureType == STRUCTURE_TYPE.CEMETERY || character.currentStructure.structureType == STRUCTURE_TYPE.CITY_CENTER) && character.isAtHomeRegion) {
            log += $"\n-{character.name} is in the Exterior structure of home location";

            log += "\n-If it is Morning or Afternoon, 25% chance to enter Stroll Outside Mode";
            TIME_IN_WORDS currentTimeOfDay = GameManager.GetCurrentTimeInWordsOfTick(character);
            if (currentTimeOfDay == TIME_IN_WORDS.MORNING || currentTimeOfDay == TIME_IN_WORDS.LUNCH_TIME || currentTimeOfDay == TIME_IN_WORDS.AFTERNOON) {
                log += $"\n  -Time of Day: {currentTimeOfDay}";
                int chance = UnityEngine.Random.Range(0, 100);
                log += $"\n  -RNG roll: {chance}";
                if (chance < 25) {
                    log += $"\n  -Morning or Afternoon: {character.name} will enter Stroll Outside State";
                    character.PlanIdleStrollOutside(); //character.currentStructure
                    return true;
                }
            } else {
                log += $"\n  -Time of Day: {currentTimeOfDay}";
            }
            // log += "\n-Otherwise, if it is Morning or Afternoon or Early Night, and the character has a positive relationship with someone currently Paralyzed or Catatonic, 30% chance to Check Out one at random";
            // if (currentTimeOfDay == TIME_IN_WORDS.MORNING || currentTimeOfDay == TIME_IN_WORDS.AFTERNOON || currentTimeOfDay == TIME_IN_WORDS.EARLY_NIGHT) {
            //     log += "\n  -Time of Day: " + currentTimeOfDay.ToString();
            //     int chance = UnityEngine.Random.Range(0, 100);
            //     log += "\n  -RNG roll: " + chance;
            //     if (chance < 30) {
            //         Character chosenCharacter = character.GetParalyzedOrCatatonicCharacterToCheckOut();
            //         if (chosenCharacter != null) {
            //             log += "\n  -Will Check Out character " + chosenCharacter.name;
            //             character.PlanIdle(JOB_TYPE.CHECK_PARALYZED_FRIEND, new GoapEffect(GOAP_EFFECT_CONDITION.IN_VISION, string.Empty, false, GOAP_EFFECT_TARGET.TARGET), chosenCharacter);
            //             return true;
            //         } else {
            //             log += "\n  -No available character to check out ";
            //         }
            //     }
            // } else {
            //     log += "\n  -Time of Day: " + currentTimeOfDay.ToString();
            // }
            log += "\n-Otherwise, return home";
            log += $"\n  -{character.name} will do action Return Home";
            character.PlanIdleReturnHome();
            return true;
        }
        return false;
    }
}
