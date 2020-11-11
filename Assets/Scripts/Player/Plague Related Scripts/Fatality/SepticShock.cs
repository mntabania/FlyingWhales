﻿using Traits;
using UtilityScripts;
namespace Plague.Fatality {
    public class SepticShock : Fatality {
        public override FATALITY fatalityType => FATALITY.Septic_Shock;
        
        protected override void ActivateFatality(Character p_character) {
            p_character.interruptComponent.TriggerInterrupt(INTERRUPT.Septic_Shock, p_character);
        }
        public override void CharacterGainedTrait(Character p_character, Trait p_gainedTrait) {
            if (p_gainedTrait.name == "Starving") {
                if (GameUtilities.RollChance(5)) {
                    ActivateFatality(p_character);
                }
            }
        }
    }
}