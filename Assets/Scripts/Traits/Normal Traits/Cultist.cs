using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Cultist : Trait {

        public Cultist() {
            name = "Cultist";
            description = "Cultists are secret followers of the Ruinarch.";
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = 0;
        }
    }
}
