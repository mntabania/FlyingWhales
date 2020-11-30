using Traits;
using UnityEngine;
using UtilityScripts;
using Inner_Maps;
using System.Collections.Generic;

namespace Plague.Death_Effect {
    public class Zombie : PlagueDeathEffect {
        
        public override PLAGUE_DEATH_EFFECT deathEffectType => PLAGUE_DEATH_EFFECT.Zombie;

        protected override void ActivateEffect(Character p_character) {
            switch (_level) {
                case 1:
                    WalkerZombie(p_character);
                    break;
                case 2:
                    NightZombie(p_character);
                    break;
                case 3:
                    //TODO: Variety Zombie
                    NightZombie(p_character);
                    break;
            }
            Debug.Log("Activated Zombie Effect");
        }
        public override int GetNextLevelUpgradeCost() {
            switch (_level) {
                case 1:
                    return 30;
                case 2:
                    return -1; //50 Disabled, since next level is still an upcoming feature
                default:
                    return -1; //Max Level
            }
        }
        public override string GetCurrentEffectDescription() {
            switch (_level) {
                case 1:
                    return "Walker Zombie";
                case 2:
                    return "Night Zombie";
                case 3:
                    return "Variety Zombie";
                default:
                    return string.Empty;
            }
        }
        public override void OnDeath(Character p_character) {
            ActivateEffectOn(p_character);
        }

        private void WalkerZombie(Character p_character) {
            if (!p_character.characterClass.IsZombie()) {
                p_character.AssignClass("Walker Zombie");
            }
        }
        private void NightZombie(Character p_character) {
            if (!p_character.characterClass.IsZombie()) {
                p_character.AssignClass("Night Zombie");
            }
        }
    }
}