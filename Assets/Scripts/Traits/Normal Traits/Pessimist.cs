using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Pessimist : Trait {

        public Pessimist() {
            name = "Pessimist";
            description = "Pessimists lose happiness more quickly than normal.";
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            canBeTriggered = true;
            mutuallyExclusive = new string[] { "Optimist" };
        }

        #region Overrides
        public override string TriggerFlaw(Character character) {
            //Will reduce Happiness Meter to become Forlorn. If already Forlorn, reduce Happiness Meter by a further 1000.
            if (character.needsComponent.isSulking) {
                character.needsComponent.AdjustHappiness(-5f);
            } else {
                character.needsComponent.SetHappiness(CharacterNeedsComponent.SULKING_UPPER_LIMIT);
            }
            return base.TriggerFlaw(character);
        }
        #endregion
    }
}

