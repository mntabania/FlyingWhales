using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Herbalist : Trait {
        public Herbalist() {
            name = "Herbalist";
            description = "Herbalists can create Healing Potions.";
            type = TRAIT_TYPE.BUFF;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.CRAFT_TILE_OBJECT };
        }
    }
}

