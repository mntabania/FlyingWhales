using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;
using Inner_Maps;

public class DefaultMinion : CharacterBehaviourComponent {
	public DefaultMinion() {
		priority = 8;
		// attributes = new[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.WITHIN_HOME_SETTLEMENT_ONLY };
	}
	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        log += $"\n-{character.name} will roam around assigned area!";
        character.jobComponent.TriggerRoamAroundTile(out producedJob);
        return true;
	}
}
