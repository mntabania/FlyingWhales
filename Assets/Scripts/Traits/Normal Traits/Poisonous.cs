using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Poisonous : Trait {
        public override bool isSingleton => true;

        public Poisonous() {
            name = "Poisonous";
            description = "Continually produces Poison.";
            type = TRAIT_TYPE.BUFF;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            AddTraitOverrideFunctionIdentifier(TraitManager.Hour_Started_Trait);
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            for (int i = 0; i < 3; i++) {
                addedTo.traitContainer.AddTrait(addedTo, "Poisoned", bypassElementalChance: true, overrideDuration: 0);
            }
        }
        public override void OnHourStarted(ITraitable traitable) {
            base.OnHourStarted(traitable);
            traitable.traitContainer.AddTrait(traitable, "Poisoned", bypassElementalChance: true, overrideDuration: 0);
        }
        #endregion
    }
}

