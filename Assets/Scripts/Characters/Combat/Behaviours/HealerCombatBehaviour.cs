using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealerCombatBehaviour : CharacterCombatBehaviour {
    public override string description => "Occasionally heals allies.";

    public HealerCombatBehaviour() : base(CHARACTER_COMBAT_BEHAVIOUR.Healer) {

    }
}
