using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class FireProne : Trait {
        public override bool isSingleton => true;

        public FireProne() {
            name = "Fire Prone";
            description = "Takes double damage from Fire.";
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            mutuallyExclusive = new[] { "Fireproof" };
        }
    }
}
