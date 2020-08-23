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
            description = "Has not eaten for a very long time.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = 0;
            moodEffect = -5;
            AddTraitOverrideFunctionIdentifier(TraitManager.Tick_Started_Trait);
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            owner = addedTo as Character;
            deathDuration = GameManager.Instance.GetTicksBasedOnHour(24);
            currentDeathDuration = 0;
        }
        public override void OnTickStarted(ITraitable traitable) {
            base.OnTickStarted(traitable);
            CheckDeath(owner);
        }
        #endregion

        private void CheckDeath(Character owner) {
            currentDeathDuration++;
            if(currentDeathDuration >= deathDuration) {
                owner.Death("starvation");
            }
        }
    }
}
