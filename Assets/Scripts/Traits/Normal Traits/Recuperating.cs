using System.Collections.Generic;
using System.Linq;
using Characters.Components;
using Inner_Maps.Location_Structures;
using UnityEngine;
namespace Traits {
    public class Recuperating : Status {
        public override bool isSingleton => true;
        
        public Recuperating() {
            name = "Recuperating";
            description = "Recovering from an illness.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.POSITIVE;
            ticksDuration = 0;
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.FEED };
            isHidden = true;
        }
    }
}

