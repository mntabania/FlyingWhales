using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Vigilant : Trait {
        public override bool isSingleton => true;

        public Vigilant() {
            name = "Vigilant";
            description = "Vigilant characters cannot be stealthily knocked out or pickpocketed.";
            type = TRAIT_TYPE.BUFF;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
        }

        #region Overrides
        public override bool TryStopAction(INTERACTION_TYPE action, Character actor, IPointOfInterest target, ref GoapActionInvalidity goapActionInvalidity) {
            if (action == INTERACTION_TYPE.STEAL || action == INTERACTION_TYPE.KNOCKOUT_CHARACTER) {
                goapActionInvalidity.isInvalid = true;
                goapActionInvalidity.stateName = "Target Missing"; //TODO: provide log instead.
            }
            return false;
        }
        #endregion
    }
}

