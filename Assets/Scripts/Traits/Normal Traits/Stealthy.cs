namespace Traits {
    public class Stealthy : Trait {

        private Character _owner;
        
        public Stealthy() {
            name = "Stealthy";
            description = "This is Stealthy.";
            type = TRAIT_TYPE.BUFF;
            effect = TRAIT_EFFECT.POSITIVE;
            ticksDuration = 0;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character character) {
                _owner = character;
                if (character.canMove && character.canPerform && character.isDead == false 
                    && character.stateComponent.currentState is CombatState == false) {
                    //Automatically add invisible trait if character meets criteria
                    character.traitContainer.AddTrait(character, "Invisible");
                }
                Messenger.AddListener<Character, CharacterState>(Signals.CHARACTER_ENDED_STATE, OnCharacterEndedState);
                Messenger.AddListener<Character>(Signals.CHARACTER_CAN_MOVE_AGAIN, OnCharacterCanMoveAgain);
                Messenger.AddListener<Character>(Signals.CHARACTER_CAN_PERFORM_AGAIN, OnCharacterCanPerformAgain);
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character) {
                _owner = null;
                Messenger.RemoveListener<Character, CharacterState>(Signals.CHARACTER_ENDED_STATE, OnCharacterEndedState);
                Messenger.RemoveListener<Character>(Signals.CHARACTER_CAN_MOVE_AGAIN, OnCharacterCanMoveAgain);
                Messenger.RemoveListener<Character>(Signals.CHARACTER_CAN_PERFORM_AGAIN, OnCharacterCanPerformAgain);
            }
        }
        #endregion

        #region Listeners
        private void OnCharacterEndedState(Character character, CharacterState state) {
            if (character == _owner && state is CombatState) {
                character.traitContainer.AddTrait(character, "Invisible");
            }
        }
        private void OnCharacterCanMoveAgain(Character character) {
            if (character == _owner && character.stateComponent.currentState is CombatState == false) {
                character.traitContainer.AddTrait(character, "Invisible");
            }
        }
        private void OnCharacterCanPerformAgain(Character character) {
            if (character == _owner && character.stateComponent.currentState is CombatState == false) {
                character.traitContainer.AddTrait(character, "Invisible");
            }
        }
        #endregion
    }
}