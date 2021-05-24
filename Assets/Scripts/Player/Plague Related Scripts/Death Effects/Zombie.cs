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
                    VarietyZombie(p_character);
                    break;
            }
#if DEBUG_LOG
            Debug.Log("Activated Zombie Effect");
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
                p_character.visuals.UsePreviousClassAsset(true);
                p_character.classComponent.AssignClass("Walker Zombie");
            }
        }
        private void NightZombie(Character p_character) {
            if (!p_character.characterClass.IsZombie()) {
                p_character.visuals.UsePreviousClassAsset(true);
                p_character.classComponent.AssignClass("Night Zombie");
            }
        }
        private void VarietyZombie(Character p_character) {
            if (!p_character.characterClass.IsZombie()) {
                p_character.visuals.UsePreviousClassAsset(true);
                string className = "Boomer Zombie";
                int roll = GameUtilities.RandomBetweenTwoNumbers(0, 99);
                if (roll >= 0 && roll < 25) {
                    className = "Walker Zombie";
                } else if (roll >= 25 && roll < 50) {
                    className = "Fast Zombie";
                } else if (roll >= 50 && roll < 70) {
                    className = "Night Zombie";
                } else if (roll >= 70 && roll < 85) {
                    className = "Boomer Zombie";
                } else if (roll >= 85 && roll < 100) {
                    className = "Tank Zombie";
                }
                p_character.classComponent.AssignClass(className);
            }
        }
    }
}