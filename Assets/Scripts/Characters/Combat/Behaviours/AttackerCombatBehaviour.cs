using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackerCombatBehaviour : CharacterCombatBehaviour {
    public override string description => "Normal combatant with good attack and health.";
    public AttackerCombatBehaviour() : base(CHARACTER_COMBAT_BEHAVIOUR.Attacker) {

    }
}
