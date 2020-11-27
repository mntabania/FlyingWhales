using System;
namespace Quests.Steps {
    public class ClickOnCharacterStep : QuestStep {
        private readonly Func<Character, bool> _validityChecker;
        public ClickOnCharacterStep(string stepDescription = "Click on a character", 
            System.Func<Character, bool> validityChecker = null) : base(stepDescription) {
            _validityChecker = validityChecker;
        }
        protected override void SubscribeListeners() {
            Messenger.AddListener<ISelectable>(ControlsSignals.SELECTABLE_LEFT_CLICKED, CheckForCompletion);
            Messenger.AddListener<InfoUIBase>(UISignals.MENU_OPENED, CheckForCompletion);
            Character selectedCharacter = UIManager.Instance.GetCurrentlySelectedCharacter();
            if (selectedCharacter != null) {
                CheckForCompletion(selectedCharacter);
            }
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<ISelectable>(ControlsSignals.SELECTABLE_LEFT_CLICKED, CheckForCompletion);
            Messenger.RemoveListener<InfoUIBase>(UISignals.MENU_OPENED, CheckForCompletion);
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
        private void CheckForCompletion(InfoUIBase openedUI) {
            if (openedUI is CharacterInfoUI characterInfoUI) {
                if (_validityChecker != null) {
                    if (_validityChecker.Invoke(characterInfoUI.activeCharacter)) {
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