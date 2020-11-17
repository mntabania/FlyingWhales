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
        public override void OnDeath(Character p_character) {
            ActivateEffect(p_character);
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