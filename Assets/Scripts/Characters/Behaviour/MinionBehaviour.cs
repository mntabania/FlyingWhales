using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionBehaviour : CharacterBehaviourComponent {

	public MinionBehaviour() {
		priority = 40;
	}
    
    public override bool TryDoBehaviour(Character character, ref string log) {
        log += $"\n-{character.name} is going to stroll...";
        character.PlanIdleStrollOutside();
        return true;
    }
}
