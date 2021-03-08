using Inner_Maps.Location_Structures;
using Traits;
namespace Characters.Components {
    public class CharacterEventDispatcher {

        public interface ITraitListener {
            void OnCharacterGainedTrait(Character p_character, Trait p_gainedTrait);
            void OnCharacterLostTrait(Character p_character, Trait p_lostTrait, Character p_removedBy);
        }
        public interface ICarryListener {
            void OnCharacterCarried(Character p_character, Character p_carriedBy);
        }
        public interface ILocationListener {
            void OnCharacterLeftStructure(Character p_character, LocationStructure p_leftStructure);
        }
        public interface IDeathListener {
            void OnCharacterDied(Character p_character);
        }

        private System.Action<Character, Trait> _characterGainedTrait;
        private System.Action<Character, Trait, Character> _characterLostTrait;
        private System.Action<Character, Character> _characterCarried;
        private System.Action<Character, LocationStructure> _characterLeftStructure;
        private System.Action<Character> _characterDied;

        #region Gained Trait
        public void SubscribeToCharacterGainedTrait(ITraitListener p_traitListener) {
            _characterGainedTrait += p_traitListener.OnCharacterGainedTrait;
        }
        public void UnsubscribeToCharacterGainedTrait(ITraitListener p_traitListener) {
            _characterGainedTrait -= p_traitListener.OnCharacterGainedTrait;
        }
        public void ExecuteCharacterGainedTrait(Character p_character, Trait p_gainedTrait) {
            _characterGainedTrait?.Invoke(p_character, p_gainedTrait);
        }
        #endregion

        #region Lost Trait
        public void SubscribeToCharacterLostTrait(ITraitListener p_traitListener) {
            _characterLostTrait += p_traitListener.OnCharacterLostTrait;
        }
        public void UnsubscribeToCharacterLostTrait(ITraitListener p_traitListener) {
            _characterLostTrait -= p_traitListener.OnCharacterLostTrait;
        }
        public void ExecuteCharacterLostTrait(Character p_character, Trait p_lostTrait, Character p_removedBy) {
            _characterLostTrait?.Invoke(p_character, p_lostTrait, p_removedBy);
        }
        #endregion

        #region Carried
        public void SubscribeToCharacterCarried(ICarryListener p_carryListener) {
            _characterCarried += p_carryListener.OnCharacterCarried;
        }
        public void UnsubscribeToCharacterCarried(ICarryListener p_carryListener) {
            _characterCarried -= p_carryListener.OnCharacterCarried;
        }
        public void ExecuteCarried(Character p_character, Character p_carriedBy) {
            _characterCarried?.Invoke(p_character, p_carriedBy);
        }
        #endregion
        
        #region Structure
        public void SubscribeToCharacterLeftStructure(ILocationListener p_listener) {
            _characterLeftStructure += p_listener.OnCharacterLeftStructure;
        }
        public void UnsubscribeToCharacterLeftStructure(ILocationListener p_listener) {
            _characterLeftStructure -= p_listener.OnCharacterLeftStructure;
        }
        public void ExecuteCharacterLeftStructure(Character p_character, LocationStructure p_leftStructure) {
            _characterLeftStructure?.Invoke(p_character, p_leftStructure);
        }
        #endregion

        #region Death
        public void SubscribeToCharacterDied(IDeathListener p_listener) {
            _characterDied += p_listener.OnCharacterDied;
        }
        public void UnsubscribeToCharacterDied(IDeathListener p_listener) {
            _characterDied -= p_listener.OnCharacterDied;
        }
        public void ExecuteCharacterDied(Character p_character) {
            _characterDied?.Invoke(p_character);
        }
        #endregion
    }
}