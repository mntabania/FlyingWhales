using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Malnourished : Status {

        private Character owner;
        private int deathDuration;
        private int currentDeathDuration;

        public Malnourished() {
            name = "Malnourished";
            description = "This character is malnourished.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = 0;
            moodEffect = -5;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            owner = addedTo as Character;
            deathDuration = GameManager.Instance.GetTicksBasedOnHour(24);
            currentDeathDuration = 0;
        }
        public override void OnTickStarted() {
            base.OnTickStarted();
            CheckDeath();
        }
        #endregion

        private void CheckDeath() {
            currentDeathDuration++;
            if(currentDeathDuration >= deathDuration) {
                owner.Death("starvation");
            }
        }
    }
}
