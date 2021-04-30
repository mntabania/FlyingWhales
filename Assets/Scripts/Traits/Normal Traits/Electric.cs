using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Electric : Trait {
        public override bool isSingleton => true;

        public Electric() {
            name = "Electric";
            description = "Deals Electric damage and receives significantly reduced damage from Electric attacks. Cannot be Zapped.";
            type = TRAIT_TYPE.BUFF;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            elementalType = ELEMENTAL_TYPE.Electric;
            resistancesType = new List<RESISTANCE>() { RESISTANCE.Electric };
            resistancesValue = new List<float>() { 85f };
        }
    }
}