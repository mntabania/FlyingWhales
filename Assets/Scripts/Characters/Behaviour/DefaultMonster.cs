using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultMonster : BaseMonsterBehaviour {
	public DefaultMonster() {
		priority = 8;
	}
	protected override bool WildBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
		return DefaultWildMonsterBehaviour(character, ref log, out producedJob);
	}
}
