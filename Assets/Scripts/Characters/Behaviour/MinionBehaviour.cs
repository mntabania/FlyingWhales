using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionBehaviour : CharacterBehaviourComponent {

	public MinionBehaviour() {
		priority = 8;
	}
    
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        log += $"\n-{character.name} is going to stroll...";
        character.jobComponent.PlanIdleStrollOutside(out producedJob);
        return true;
    }
}
