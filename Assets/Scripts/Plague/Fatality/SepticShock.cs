using Traits;
using UtilityScripts;
namespace Plague.Fatality {
    public class SepticShock : Fatality {
        public override FATALITY fatalityType => FATALITY.Septic_Shock;
        
        public override void StartListeningForTrigger() {
            Messenger.AddListener<ITraitable, Trait>(TraitSignals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
        }
        private void OnTraitableGainedTrait(ITraitable p_traitable, Trait p_gainedTrait) {
            if (p_traitable is Character character && p_gainedTrait.name == "Starving") {
                if (GameUtilities.RollChance(5)) {
                    ActivateFatality(character);
                }
            }
        }
        protected override void ActivateFatality(Character p_character) {
            p_character.interruptComponent.TriggerInterrupt(INTERRUPT.Septic_Shock, p_character);
        }
    }
}