using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Ensnared : Status {
        public override bool isSingleton => true;

        public Ensnared() {
            name = "Ensnared";
            description = "Trapped and unable to move.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(3);
            hindersMovement = true;
            hindersPerform = true;
            moodEffect = -5;
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.REMOVE_ENSNARED };
        }
    }
}
