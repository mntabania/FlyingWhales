using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Vigilant : Trait {
        public override bool isSingleton => true;

        public Vigilant() {
            name = "Vigilant";
            description = "Cannot be knocked-out or pickpocketed.";
            type = TRAIT_TYPE.BUFF;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
        }
    }
}

