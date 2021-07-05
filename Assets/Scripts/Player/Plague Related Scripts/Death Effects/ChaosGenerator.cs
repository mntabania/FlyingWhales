using Traits;
using UnityEngine;
using UtilityScripts;
using Inner_Maps;
using System.Collections.Generic;

namespace Plague.Death_Effect {
    public class ChaosGenerator : PlagueDeathEffect {
        
        public override PLAGUE_DEATH_EFFECT deathEffectType => PLAGUE_DEATH_EFFECT.Chaos_Generator;

        protected override void ActivateEffect(Character p_character) {
            switch (_level) {
                case 1:
                    CreateChaosOrbs(1, p_character);
                    break;
                case 2:
                    CreateChaosOrbs(2, p_character);
                    break;
                case 3:
                    CreateChaosOrbs(3, p_character);
                    break;
            }
#if DEBUG_LOG
            Debug.Log("Activated Chaos Generator Effect");
#endif
        }
        protected override int GetNextLevelUpgradeCost() {
            switch (_level) {
                case 1:
                    return 50;
                case 2:
                    return 75;
                default:
                    return -1; //Max Level
            }
        }
        public override string GetCurrentEffectDescription() {
            switch (_level) {
                case 1:
                    return "1 Orbs";
                case 2:
                    return "2 Orbs";
                case 3:
                    return "3 Orbs";
                default:
                    return string.Empty;
            }
        }
        public override void OnDeath(Character p_character) {
            ActivateEffectOn(p_character);
        }

        private void CreateChaosOrbs(int amount, Character p_character) {
            if (p_character.marker && p_character.currentRegion != null) {
                Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, p_character.marker.transform.position, amount, p_character.currentRegion.innerMap);
            }
        }
    }
}