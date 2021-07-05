using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EscortCombatBehaviour : CharacterCombatBehaviour {
    public override string description => "Escorts.";

    public EscortCombatBehaviour() : base(CHARACTER_COMBAT_BEHAVIOUR.Escort) {

    }
}
