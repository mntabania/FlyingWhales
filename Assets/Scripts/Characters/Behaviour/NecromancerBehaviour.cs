using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;

public class NecromancerBehaviour : CharacterBehaviourComponent {
	public NecromancerBehaviour() {
		priority = 20;
	}
	public override bool TryDoBehaviour(Character character, ref string log) {
        log += $"\n-{character.name} is a necromancer";
        if (character.homeStructure != null && !character.homeStructure.hasBeenDestroyed && character.homeStructure.tiles.Count > 0 && character.homeStructure == character.necromancerTrait.lairStructure) {
            log += $"\n-Character has a home structure/territory";
            character.jobComponent.TriggerRoamAroundTile();
        } else {
            log += $"\n-Character does not have a home structure/territory";
            log += $"\n-Character will set lair";
            character.interruptComponent.TriggerInterrupt(INTERRUPT.Set_Lair, character);
            if(character.homeStructure == character.necromancerTrait.lairStructure) {
                log += $"\n-Lair is set, character home structure is set as the lair";
                log += $"\n-Character will return home";
                character.PlanIdleReturnHome();
            } else {
                log += $"\n-Lair is not set, will spawn lair";

            }
        }
        return false;
	}
}
