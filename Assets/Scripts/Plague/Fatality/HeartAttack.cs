using Traits;
using UtilityScripts;

namespace Plague.Fatality {
    public class HeartAttack : Fatality {
        
        public override FATALITY fatalityType => FATALITY.Heart_Attack;
        
        public override void StartListeningForTrigger() {
            Messenger.AddListener<ITraitable, Trait>(TraitSignals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
        }
        private void OnTraitableGainedTrait(ITraitable p_traitable, Trait p_gainedTrait) {
            if (p_traitable is Character character) {
                if (p_gainedTrait.name == "Spent") {
                    if (GameUtilities.RollChance(1)) {
                        ActivateFatality(character);    
                    }
                } else if (p_gainedTrait.name == "Drained") {
                    if (GameUtilities.RollChance(2)) {
                        ActivateFatality(character);
                    }
                }
            }
        }
        protected override void ActivateFatality(Character p_character) {
            p_character.interruptComponent.TriggerInterrupt(INTERRUPT.Septic_Shock, p_character);
        }
    }
}