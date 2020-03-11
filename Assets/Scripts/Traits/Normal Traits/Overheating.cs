using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Overheating : Status {

        public Overheating() {
            name = "Overheating";
            description = "This is overheating.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(4);
            isStacking = true;
            moodEffect = -6;
            stackLimit = 5;
            stackModifier = 1.5f;
        }
    }
}
