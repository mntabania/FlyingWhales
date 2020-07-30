using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Withdrawal : Status {
        public override bool isSingleton => true;

        public Withdrawal() {
            name = "Withdrawal";
            description = "An alcoholic that has not had a drink for quite some time.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(48);
            moodEffect = -5;
            isStacking = true;
            stackLimit = 4;
            stackModifier = 2;
        }
    }
}
