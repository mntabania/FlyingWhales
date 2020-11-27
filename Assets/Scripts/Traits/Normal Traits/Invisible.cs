using System;
using System.Diagnostics;
using Traits;
namespace Traits {
    public class Invisible : Status {

        private IPointOfInterest _owner;
        public COMBAT_MODE originalCombatMode { get; private set; }
        public override Type serializedData => typeof(SaveDataInvisible);
        public Invisible() {
            name = "Invisible";
            description = "You can't see me.";
            type = TRAIT_TYPE.BUFF;
            effect = TRAIT_EFFECT.POSITIVE;
            ticksDuration = 0;
            AddTraitOverrideFunctionIdentifier(TraitManager.Initiate_Map_Visual_Trait);
        }

        #region Loading
        public override void LoadFirstWaveInstancedTrait(SaveDataTrait saveDataTrait) {
            base.LoadFirstWaveInstancedTrait(saveDataTrait);
            SaveDataInvisible saveDataInvisible = saveDataTrait as SaveDataInvisible;
            originalCombatMode = saveDataInvisible.originalCombatMode;
        }
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            if (addTo is IPointOfInterest poi) {
                _owner = poi;
                if (poi.mapObjectVisual != null) {
                    poi.mapObjectVisual.visionTrigger.SetVisionTriggerCollidersState(false);
                    poi.mapObjectVisual.SetVisualAlpha(0.45f);
                }
                if (poi is Character character) {
                    Messenger.AddListener<Character, CharacterState>(CharacterSignals.CHARACTER_STARTED_STATE, OnCharacterStartedState);
                    Messenger.AddListener<Character, int, object>(CharacterSignals.CHARACTER_ADJUSTED_HP, OnCharacterAdjustedHP);
                    Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_MOVE, OnCharacterCanNoLongerMove);
                    Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterCanNoLongerPerform);
                }
            }
        }
        #endregion

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
                    originalCombatMode = character.combatComponent.combatMode;
                    character.combatComponent.SetCombatMode(COMBAT_MODE.Passive);
                    Messenger.AddListener<Character, CharacterState>(CharacterSignals.CHARACTER_STARTED_STATE, OnCharacterStartedState);
                    Messenger.AddListener<Character, int, object>(CharacterSignals.CHARACTER_ADJUSTED_HP, OnCharacterAdjustedHP);
                    Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_MOVE, OnCharacterCanNoLongerMove);
                    Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterCanNoLongerPerform);
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
                    character.combatComponent.SetCombatMode(originalCombatMode);
                    Messenger.RemoveListener<Character, CharacterState>(CharacterSignals.CHARACTER_STARTED_STATE, OnCharacterStartedState);
                    Messenger.RemoveListener<Character, int, object>(CharacterSignals.CHARACTER_ADJUSTED_HP, OnCharacterAdjustedHP);
                    Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_MOVE, OnCharacterCanNoLongerMove);
                    Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterCanNoLongerPerform);
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

#region Save Data
public class SaveDataInvisible : SaveDataTrait {
    public COMBAT_MODE originalCombatMode;
    public override void Save(Trait trait) {
        base.Save(trait);
        Invisible invisible = trait as Invisible;
        Debug.Assert(invisible != null, nameof(invisible) + " != null");
        originalCombatMode = invisible.originalCombatMode;
    }
}
#endregion