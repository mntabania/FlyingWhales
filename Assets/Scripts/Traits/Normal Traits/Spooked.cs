using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Spooked : Status {
        public Character owner { get; private set; }

        public Spooked() {
            name = "Spooked";
            description = "Something recently scared it.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(12);
            moodEffect = -3;
            isStacking = true;
            stackLimit = 5;
            stackModifier = 0.5f;
            hindersSocials = true;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable sourcePOI) {
            if (sourcePOI is Character) {
                owner = sourcePOI as Character;
            }
            base.OnAddTrait(sourcePOI);
        }
        #endregion

        public bool TriggerFeelingSpooked() {
            return owner.interruptComponent.TriggerInterrupt(INTERRUPT.Feeling_Spooked, owner);
        }
    }
}
