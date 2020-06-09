using System;
using Traits;
namespace Quests {
    public class CharacterGainedTrait : QuestCriteria {

        private readonly string _traitName;
        private readonly Func<Trait, bool> _validityChecker;
        public Character character { get; private set; }

        public CharacterGainedTrait(string traitName, System.Func<Trait, bool> validityChecker = null) {
            _traitName = traitName;
            _validityChecker = validityChecker;
        }
        
        public override void Enable() {
            Messenger.AddListener<ITraitable, Trait>(Signals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
        }
        public override void Disable() {
            Messenger.RemoveListener<ITraitable, Trait>(Signals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
        }
        
        private void OnTraitableGainedTrait(ITraitable traitable, Trait trait) {
            if (traitable is Character characterThatGainedTrait && trait.name == _traitName) {
                if (_validityChecker != null) {
                    if (_validityChecker.Invoke(trait)) {
                        character = characterThatGainedTrait;
                        SetCriteriaAsMet();
                    }
                } else {
                    character = characterThatGainedTrait;
                    SetCriteriaAsMet();    
                }
                
            }
        }
    }
}