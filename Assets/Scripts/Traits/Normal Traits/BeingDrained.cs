using UnityEngine;

namespace Traits {
    public class BeingDrained : Status {

        public BeingDrained() {
            name = "Being Drained";
            description = "This character is being drained!";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = 0;
            isHidden = true;
            hindersSocials = true;
            hindersPerform = true;
            hindersWitness = true;
            AddTraitOverrideFunctionIdentifier(TraitManager.Tick_Ended_Trait);
        }
        public override bool OnDeath(Character character) {
            character.traitContainer.RemoveTrait(character, this);
            return base.OnDeath(character);
        }
        public override void OnTickEnded(ITraitable traitable) {
            base.OnTickEnded(traitable);
            if (traitable is Character character) {
                DrainPerTick(character);
            }
        }
        private void DrainPerTick(Character p_character) {
            if (!p_character.isDead) {
                int hpReduction = Mathf.RoundToInt(p_character.maxHP * ((PlayerSkillManager.Instance.GetIncreaseStatsPercentagePerLevel(PLAYER_SKILL_TYPE.DRAIN_SPIRIT) / 100f)));
                p_character.AdjustHP(-hpReduction, ELEMENTAL_TYPE.Normal, true, this, showHPBar: true);
                if (p_character.isDead && p_character.skillCauseOfDeath == PLAYER_SKILL_TYPE.NONE) {
                    p_character.skillCauseOfDeath = PLAYER_SKILL_TYPE.DRAIN_SPIRIT;
                    int spiritEnergy = p_character is Summon ? 1 : 2;
                    Messenger.Broadcast(PlayerSignals.CREATE_SPIRIT_ENERGY, p_character.deathTilePosition.centeredWorldLocation, spiritEnergy, p_character.deathTilePosition.parentMap);
                }
            }
        }
    }
}