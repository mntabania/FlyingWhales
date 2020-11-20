using Traits;
namespace Characters.Components {
    public class CharacterEventDispatcher {

        public interface ITraitListener {
            void OnCharacterGainedTrait(Character p_character, Trait p_gainedTrait);
            void OnCharacterLostTrait(Character p_character, Trait p_lostTrait, Character p_removedBy);
        }

        private System.Action<Character, Trait> _characterGainedTrait;
        private System.Action<Character, Trait, Character> _characterLostTrait;
        
        public void SubscribeToCharacterGainedTrait(ITraitListener p_traitListener) {
            _characterGainedTrait += p_traitListener.OnCharacterGainedTrait;
        }
        public void UnsubscribeToCharacterGainedTrait(ITraitListener p_traitListener) {
            _characterGainedTrait -= p_traitListener.OnCharacterGainedTrait;
        }
        public void ExecuteCharacterGainedTrait(Character p_character, Trait p_gainedTrait) {
            _characterGainedTrait?.Invoke(p_character, p_gainedTrait);
        }

        public void SubscribeToCharacterLostTrait(ITraitListener p_traitListener) {
            _characterLostTrait += p_traitListener.OnCharacterLostTrait;
        }
        public void UnsubscribeToCharacterLostTrait(ITraitListener p_traitListener) {
            _characterLostTrait -= p_traitListener.OnCharacterLostTrait;
        }
        public void ExecuteCharacterLostTrait(Character p_character, Trait p_lostTrait, Character p_removedBy) {
            _characterLostTrait?.Invoke(p_character, p_lostTrait, p_removedBy);
        }
    }
}