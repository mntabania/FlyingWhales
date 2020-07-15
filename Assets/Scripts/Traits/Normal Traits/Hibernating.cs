using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Hibernating : Status {
        public override bool isNotSavable {
            get { return true; }
        }

        public override bool isSingleton => true;

        public Hibernating() {
            name = "Hibernating";
            description = "This character is hibernating.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            //hindersMovement = true;
            hindersWitness = true;
            hindersPerform = true;
        }
    }
}

