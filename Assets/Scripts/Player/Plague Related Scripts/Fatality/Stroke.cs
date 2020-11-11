using Traits;
using UnityEngine;
using UtilityScripts;
namespace Plague.Fatality {
    public class Stroke : Fatality {
        public override FATALITY fatalityType => FATALITY.Stroke;
        
        protected override void ActivateFatality(Character p_character) {
            //TODO: Stroke interrupt
            Debug.Log("Activated Stroke Fatality");    
        }
        
        public override void CharacterGainedTrait(Character p_character, Trait p_gainedTrait) {
            if (p_gainedTrait.name == "Exhausted") {
                if (GameUtilities.RollChance(5)) {
                    ActivateFatality(p_character);
                }        
            }
        }
    }
}