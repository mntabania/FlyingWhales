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
            description = "Indefinitely inactive. There must be something you can do to awaken it.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            //hindersMovement = true;
            hindersWitness = true;
            hindersPerform = true;
        }
    }
}

