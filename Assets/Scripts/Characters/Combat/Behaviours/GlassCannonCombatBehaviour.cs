using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlassCannonCombatBehaviour : CharacterCombatBehaviour {
    public override string description => "High damage ranged unit but has low HP.";

    public GlassCannonCombatBehaviour() : base(CHARACTER_COMBAT_BEHAVIOUR.Glass_Cannon) {

    }
}
