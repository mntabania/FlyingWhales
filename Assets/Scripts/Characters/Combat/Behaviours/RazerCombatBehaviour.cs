using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RazerCombatBehaviour : CharacterCombatBehaviour {
    public override string description => "Deals bonus damage to objects and structures.";

    public RazerCombatBehaviour() : base(CHARACTER_COMBAT_BEHAVIOUR.Razer) {

    }
}
