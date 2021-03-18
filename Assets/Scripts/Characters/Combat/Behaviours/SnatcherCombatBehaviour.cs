using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnatcherCombatBehaviour : CharacterCombatBehaviour {
    public override string description => "Effective at knocking out a target.";

    public SnatcherCombatBehaviour() : base(CHARACTER_COMBAT_BEHAVIOUR.Snatcher) {

    }
}
