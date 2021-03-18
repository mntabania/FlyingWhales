using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerCombatBehaviour : CharacterCombatBehaviour {
    public override string description => "A ranged attacker that is unable to move.";

    public TowerCombatBehaviour() : base(CHARACTER_COMBAT_BEHAVIOUR.Tower) {

    }
}
