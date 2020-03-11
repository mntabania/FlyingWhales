using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Withdrawal : Status {
        public Withdrawal() {
            name = "Withdrawal";
            description = "This character is in withdrawal.";
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
