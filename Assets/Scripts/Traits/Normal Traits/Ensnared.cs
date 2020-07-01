using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Ensnared : Status {
        public override bool isSingleton => true;

        public Ensnared() {
            name = "Ensnared";
            description = "This is ensnared.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(3);
            hindersMovement = true;
            hindersPerform = true;
            moodEffect = -5;
        }
    }
}
