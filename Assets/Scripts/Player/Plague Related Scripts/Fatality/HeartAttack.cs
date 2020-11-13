using Traits;
using UnityEngine;
using UtilityScripts;

namespace Plague.Fatality {
    public class HeartAttack : Fatality {
        
        public override PLAGUE_FATALITY fatalityType => PLAGUE_FATALITY.Heart_Attack;
        protected override void ActivateFatality(Character p_character) {
            p_character.interruptComponent.TriggerInterrupt(INTERRUPT.Heart_Attack, p_character);
            Debug.Log("Activated Heart Attack Fatality");
        }
        public override void CharacterGainedTrait(Character p_character, Trait p_gainedTrait) {
            int chance = 0;
            if (p_gainedTrait.name == "Spent") {
                chance = 1;
            } else if (p_gainedTrait.name == "Drained") {
                chance = 2;
            }
            if (GameUtilities.RollChance(chance)) {
                ActivateFatality(p_character);
            }
        }
    }
}