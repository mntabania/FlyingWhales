using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Narcoleptic : Trait {
        public Character owner { get; private set; }

        public Narcoleptic() {
            name = "Narcoleptic";
            description = "Randomly plops down to sleep.";
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            canBeTriggered = true;
            AddTraitOverrideFunctionIdentifier(TraitManager.Per_Tick_While_Stationary_Unoccupied);
        }

        #region Overrides
        public override void OnAddTrait(ITraitable sourceCharacter) {
            base.OnAddTrait(sourceCharacter);
            if (sourceCharacter is Character character) {
                owner = character;
            }
        }
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            if (addTo is Character character) {
                owner = character;
            }
        }
        public override bool PerTickWhileStationaryOrUnoccupied(Character p_character) {
            int napChance = UnityEngine.Random.Range(0, 100);
            if (napChance < 4) {
                return DoNarcolepticNap();
            }
            return false;
        }
        public override string TriggerFlaw(Character character) {
            DoNarcolepticNap();
            return base.TriggerFlaw(character);
        }
        private bool DoNarcolepticNap() {
            return owner.interruptComponent.TriggerInterrupt(INTERRUPT.Narcoleptic_Attack, owner);
        }
        #endregion
    }
}

