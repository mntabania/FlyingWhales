using System;
namespace Traits {
    public class Invisible : Status {

        private IPointOfInterest _owner;
        
        public Invisible() {
            name = "Invisible";
            description = "This is Invisible.";
            type = TRAIT_TYPE.BUFF;
            effect = TRAIT_EFFECT.POSITIVE;
            ticksDuration = 0;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is IPointOfInterest poi) {
                _owner = poi;
                if (poi.mapObjectVisual != null) {
                    poi.mapObjectVisual.visionTrigger.SetVisionTriggerCollidersState(false);
                    poi.mapObjectVisual.SetVisualAlpha(0.45f);
                }
                if (poi is Character) {
                    Messenger.AddListener<Character, CharacterState>(Signals.CHARACTER_STARTED_STATE, OnCharacterStartedState);
                    Messenger.AddListener<Character, int>(Signals.CHARACTER_ADJUSTED_HP, OnCharacterAdjustedHP);
                    Messenger.AddListener<Character>(Signals.CHARACTER_CAN_NO_LONGER_MOVE, OnCharacterCanNoLongerMove);
                    Messenger.AddListener<Character>(Signals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterCanNoLongerPerform);
                }
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is IPointOfInterest poi) {
                _owner = poi;
                if (poi.mapObjectVisual != null) {
                    poi.mapObjectVisual.visionTrigger.SetVisionTriggerCollidersState(true);
                    poi.mapObjectVisual.SetVisualAlpha(1f);
                }
                if (poi is Character) {
                    Messenger.RemoveListener<Character, CharacterState>(Signals.CHARACTER_STARTED_STATE, OnCharacterStartedState);
                    Messenger.RemoveListener<Character, int>(Signals.CHARACTER_ADJUSTED_HP, OnCharacterAdjustedHP);
                    Messenger.RemoveListener<Character>(Signals.CHARACTER_CAN_NO_LONGER_MOVE, OnCharacterCanNoLongerMove);
                    Messenger.RemoveListener<Character>(Signals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterCanNoLongerPerform);
                }
            }
        }
        public override bool OnDeath(Character character) {
            character.traitContainer.RemoveTrait(character, this);
            return base.OnDeath(character);
        }
        #endregion

        #region Listeners
        private void OnCharacterStartedState(Character character, CharacterState state) {
            if (character == _owner && state is CombatState) {
                character.traitContainer.RemoveTrait(character, this);
            }
        }
        private void OnCharacterAdjustedHP(Character character, int amount) {
            if (character == _owner && amount < 0) {
                character.traitContainer.RemoveTrait(character, this);
            }
        }
        private void OnCharacterCanNoLongerMove(Character character) {
            if (character == _owner) {
                character.traitContainer.RemoveTrait(character, this);
            }
        }
        private void OnCharacterCanNoLongerPerform(Character character) {
            if (character == _owner) {
                character.traitContainer.RemoveTrait(character, this);
            }
        }
        #endregion
    }
}