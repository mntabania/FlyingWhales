﻿using System;
using System.Collections.Generic;
namespace Quests.Steps {
    public class CharactersRemovedBehaviourStep : QuestStep {
        private readonly Func<List<Character>, int, string> _descriptionGetter;
        private readonly List<Character> _targets;
        private readonly int _initialCharactersToEliminate;
        private readonly CharacterBehaviourComponent _behaviourType;
        
        public CharactersRemovedBehaviourStep(string stepDescription, List<Character> targets, CharacterBehaviourComponent behaviourType) : base(stepDescription) {
            _targets = new List<Character>(targets);
            _behaviourType = behaviourType;
            _initialCharactersToEliminate = _targets.Count;
            for (int i = 0; i < targets.Count; i++) {
                Character target = targets[i];
                if (!target.behaviourComponent.HasBehaviour(behaviourType.GetType())) {
                    _targets.Remove(target);
                    objectsToCenter.Remove(target);
                }
            }
        }
        public CharactersRemovedBehaviourStep(Func<List<Character>, int, string> descriptionGetter, List<Character> targets, CharacterBehaviourComponent behaviourType) : base(string.Empty) {
            _descriptionGetter = descriptionGetter;
            _targets = targets;
            _behaviourType = behaviourType;
            _initialCharactersToEliminate = _targets.Count;
            for (int i = 0; i < targets.Count; i++) {
                Character target = targets[i];
                if (!target.behaviourComponent.HasBehaviour(behaviourType.GetType())) {
                    _targets.Remove(target);
                    objectsToCenter.Remove(target);
                }
            }
        }
        protected override void SubscribeListeners() {
            Messenger.AddListener<Character, CharacterBehaviourComponent>(CharacterSignals.CHARACTER_REMOVED_BEHAVIOUR, OnCharacterRemovedBehaviour);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<Character, CharacterBehaviourComponent>(CharacterSignals.CHARACTER_REMOVED_BEHAVIOUR, OnCharacterRemovedBehaviour);
            // Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        }

        #region Completion
        private void OnCharacterRemovedBehaviour(Character character, CharacterBehaviourComponent behaviourComponent) {
            if (_targets.Contains(character) && behaviourComponent == _behaviourType) {
                _targets.Remove(character);
                objectsToCenter.Remove(character);
                Messenger.Broadcast(UISignals.UPDATE_QUEST_STEP_ITEM, this as QuestStep);
                if (_targets.Count == 0) {
                    Complete();    
                }
            }
        }
        #endregion

        #region Description
        protected override string GetStepDescription() {
            if (_descriptionGetter != null) {
                return _descriptionGetter.Invoke(_targets, _initialCharactersToEliminate);
            }
            return base.GetStepDescription();
        }
        #endregion
    }
}