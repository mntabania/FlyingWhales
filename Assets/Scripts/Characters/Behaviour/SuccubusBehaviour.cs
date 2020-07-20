using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;
using Traits;

public class SuccubusBehaviour : CharacterBehaviourComponent {
	public SuccubusBehaviour() {
		priority = 8;
		// attributes = new[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.WITHIN_HOME_SETTLEMENT_ONLY };
	}
	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        log += $"\n-{character.name} is a succubus";
        if (character.traitContainer.HasTrait("Disguised")) {
            Character targetCharacter = character.currentRegion.GetRandomAliveVillagerCharacterWithGender(GENDER.MALE);
            if (targetCharacter != null) {
                log += $"\n-Target for make love is: " + targetCharacter.name;
                if (character.jobComponent.TriggerMakeLoveJob(targetCharacter, out producedJob)) {
                    return true;
                }
            }
        } else {
            log += $"\n-Character is not in disguise, 10% chance to disguise";
            int roll = UnityEngine.Random.Range(0, 100);
            log += "\nRoll: " + roll;
            if(roll < 50) {
                Character targetCharacter = character.currentRegion.GetRandomAliveVillagerCharacterWithGender(GENDER.FEMALE);
                if(targetCharacter != null) {
                    log += $"\n-Target for disguise is: " + targetCharacter.name;
                    if (character.jobComponent.TriggerDisguiseJob(targetCharacter, out producedJob)) {
                        return true;
                    }
                }
            }
        }
        log += $"\n-Character will roam";
        character.jobComponent.TriggerRoamAroundTile(out producedJob);
        return true;
	}
}
