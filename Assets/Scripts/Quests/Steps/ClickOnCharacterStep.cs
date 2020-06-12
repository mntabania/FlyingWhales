﻿using System;
namespace Quests.Steps {
    public class ClickOnCharacterStep : QuestStep {
        private readonly Func<Character, bool> _validityChecker;
        public ClickOnCharacterStep(string stepDescription = "Click on a character", 
            System.Func<Character, bool> validityChecker = null) : base(stepDescription) {
            _validityChecker = validityChecker;
        }
        protected override void SubscribeListeners() {
            Messenger.AddListener<ISelectable>(Signals.SELECTABLE_LEFT_CLICKED, CheckForCompletion);
            Character selectedCharacter = UIManager.Instance.GetCurrentlySelectedCharacter();
            if (selectedCharacter != null) {
                CheckForCompletion(selectedCharacter);
            }
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<ISelectable>(Signals.SELECTABLE_LEFT_CLICKED, CheckForCompletion);
        }

        #region Listeners
        private void CheckForCompletion(ISelectable selectable) {
            if (selectable is Character character) {
                if (_validityChecker != null) {
                    if (_validityChecker.Invoke(character)) {
                        Complete();
                    }
                } else {
                    Complete();    
                }
            }
        }
        #endregion
    }
}