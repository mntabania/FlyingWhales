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
            AddTraitOverrideFunctionIdentifier(TraitManager.Death_Trait);
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
                int hpReduction = 0;
                if (p_character.currentStructure != null && p_character.currentStructure.structureType == STRUCTURE_TYPE.TORTURE_CHAMBERS) {
                    hpReduction = Mathf.RoundToInt(p_character.maxHP * ((PlayerSkillManager.Instance.GetIncreaseStatsPercentagePerLevel(PLAYER_SKILL_TYPE.DRAIN_SPIRIT) / 100f)));
                } else {
                    hpReduction = Mathf.RoundToInt(p_character.maxHP * (((PlayerSkillManager.Instance.GetIncreaseStatsPercentagePerLevel(PLAYER_SKILL_TYPE.DRAIN_SPIRIT) * 2f) / 100f)));
                }
                
                p_character.AdjustHP(-hpReduction, ELEMENTAL_TYPE.Normal, true, this, showHPBar: true);
                int spiritEnergy = p_character is Summon ? 1 : 1;
                //Messenger.Broadcast(PlayerSignals.CREATE_SPIRIT_ENERGY, p_character.gridTileLocation.centeredWorldLocation, spiritEnergy, p_character.gridTileLocation.parentMap);
                Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, p_character.gridTileLocation.centeredWorldLocation, spiritEnergy, p_character.gridTileLocation.parentMap);
            }
        }
    }
}