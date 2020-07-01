using System;
namespace Traits {
    public class Invisible : Status {

        private IPointOfInterest _owner;
        private COMBAT_MODE _originalCombatMode;
        
        public Invisible() {
            name = "Invisible";
            description = "This is Invisible.";
            type = TRAIT_TYPE.BUFF;
            effect = TRAIT_EFFECT.POSITIVE;
            ticksDuration = 0;
            AddTraitOverrideFunctionIdentifier(TraitManager.Initiate_Map_Visual_Trait);
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
                if (poi is Character character) {
                    _originalCombatMode = character.combatComponent.combatMode;
                    character.combatComponent.SetCombatMode(COMBAT_MODE.Passive);
                    Messenger.AddListener<Character, CharacterState>(Signals.CHARACTER_STARTED_STATE, OnCharacterStartedState);
                    Messenger.AddListener<Character, int, object>(Signals.CHARACTER_ADJUSTED_HP, OnCharacterAdjustedHP);
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
                if (poi is Character character) {
                    character.combatComponent.SetCombatMode(_originalCombatMode);
                    Messenger.RemoveListener<Character, CharacterState>(Signals.CHARACTER_STARTED_STATE, OnCharacterStartedState);
                    Messenger.RemoveListener<Character, int, object>(Signals.CHARACTER_ADJUSTED_HP, OnCharacterAdjustedHP);
                    Messenger.RemoveListener<Character>(Signals.CHARACTER_CAN_NO_LONGER_MOVE, OnCharacterCanNoLongerMove);
                    Messenger.RemoveListener<Character>(Signals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterCanNoLongerPerform);
                }
            }
        }
        public override bool OnDeath(Character character) {
            character.traitContainer.RemoveTrait(character, this);
            return base.OnDeath(character);
        }
        public override void OnInitiateMapObjectVisual(ITraitable traitable) {
            if (traitable is IPointOfInterest poi) {
                if (poi.mapObjectVisual != null) {
                    poi.mapObjectVisual.visionTrigger.SetVisionTriggerCollidersState(false);
                    poi.mapObjectVisual.SetVisualAlpha(0.45f);
                }
            }
        }
        #endregion

        #region Listeners
        private void OnCharacterStartedState(Character character, CharacterState state) {
            if (character == _owner && state is CombatState) {
                character.traitContainer.RemoveTrait(character, this);
            }
        }
        private void OnCharacterAdjustedHP(Character character, int amount, object source) {
            //NOTE: Do not remove trait when damage was done to self.
            if (character == _owner && amount < 0 && source != character) {
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