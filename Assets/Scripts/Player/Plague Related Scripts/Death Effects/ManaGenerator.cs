using Traits;
using UnityEngine;
using UtilityScripts;
using Inner_Maps;
using System.Collections.Generic;

namespace Plague.Death_Effect {
    public class ManaGenerator : PlagueDeathEffect {
        
        public override PLAGUE_DEATH_EFFECT deathEffectType => PLAGUE_DEATH_EFFECT.Mana_Generator;

        protected override void ActivateEffect(Character p_character) {
            switch (_level) {
                case 1:
                    CreateManaOrbs(1, p_character);
                    break;
                case 2:
                    CreateManaOrbs(GameUtilities.RandomBetweenTwoNumbers(2, 3), p_character);
                    break;
                case 3:
                    CreateManaOrbs(GameUtilities.RandomBetweenTwoNumbers(3, 5), p_character);
                    break;
            }
            Debug.Log("Activated Mana Generator Effect");
        }
        public override void OnDeath(Character p_character) {
            ActivateEffect(p_character);
        }

        private void CreateManaOrbs(int amount, Character p_character) {
            if (p_character.marker && p_character.currentRegion != null) {
                Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, p_character.marker.transform.position, amount, p_character.currentRegion.innerMap);
            }
        }
    }
}