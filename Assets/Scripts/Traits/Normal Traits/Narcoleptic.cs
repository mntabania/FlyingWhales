using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Narcoleptic : Trait {
        //public Character owner { get; private set; }
        public override bool isSingleton => true;
        public Narcoleptic() {
            name = "Narcoleptic";
            description = "Randomly plops down to sleep. If afflicted by the player, will produce a Chaos Orb each time it enters narcoleptic sleep.";
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            canBeTriggered = true;
            AddTraitOverrideFunctionIdentifier(TraitManager.Per_Tick_While_Stationary_Unoccupied);
        }

        #region Overrides
        //public override void OnAddTrait(ITraitable sourceCharacter) {
        //    base.OnAddTrait(sourceCharacter);
        //    if (sourceCharacter is Character character) {
        //        owner = character;
        //    }
        //}
        //public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
        //    base.LoadTraitOnLoadTraitContainer(addTo);
        //    if (addTo is Character character) {
        //        owner = character;
        //    }
        //}
        public override bool PerTickWhileStationaryOrUnoccupied(Character p_character) {
            int chance = 4;
            INTERRUPT type = INTERRUPT.Narcoleptic_Nap;
            GetChanceAndInterruptType(p_character, ref chance, ref type);
            int roll = UnityEngine.Random.Range(0, 100);
            if (roll < chance) {
                return DoNarcolepticNap(p_character, type);
            }
            return false;
        }
        public override string TriggerFlaw(Character character) {
            int chance = 4;
            INTERRUPT type = INTERRUPT.Narcoleptic_Nap;
            GetChanceAndInterruptType(character, ref chance, ref type);
            DoNarcolepticNap(character, type);
            return base.TriggerFlaw(character);
        }
        private bool DoNarcolepticNap(Character p_owner, INTERRUPT p_interruptType) {
            if (p_owner.interruptComponent.TriggerInterrupt(p_interruptType, p_owner)) {
                if (p_owner.HasAfflictedByPlayerWith(name)) {
                    DispenseChaosOrbsForAffliction(p_owner, 1);
                }
                return true;
            }
            return false;
        }
        private void GetChanceAndInterruptType(Character p_owner, ref int p_chance, ref INTERRUPT p_interruptType) {
            int level = 0;
            if (p_owner.HasAfflictedByPlayerWith(name)) {
                level = PlayerSkillManager.Instance.GetAfflictionData(PLAYER_SKILL_TYPE.NARCOLEPSY).currentLevel;
            }
            if (level == 0) {
                p_chance = 3;
                p_interruptType = INTERRUPT.Narcoleptic_Nap;
            } else if (level == 1) {
                p_chance = 4;
                p_interruptType = INTERRUPT.Narcoleptic_Nap_Short;
            } else if (level == 2) {
                p_chance = 5;
                p_interruptType = INTERRUPT.Narcoleptic_Nap_Medium;
            } else if (level == 3) {
                p_chance = 6;
                p_interruptType = INTERRUPT.Narcoleptic_Nap_Long;
            }
        }
        #endregion
    }
}

