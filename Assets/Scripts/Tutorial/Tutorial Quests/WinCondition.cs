using System.Collections.Generic;
using Inner_Maps;
namespace Tutorial {
    public class WinCondition : TutorialQuest {
        public WinCondition() : base("Win Condition", TutorialManager.Tutorial.Win_Condition) { }

        #region Criteria
        protected override void ConstructCriteria() {
            _activationCriteria = new List<TutorialQuestCriteria>() {
                new CharacterDied(IsDeadCharacterValidForActivation)
            };
        }
        private bool IsDeadCharacterValidForActivation(Character character) {
            return character.IsNormalCharacter() &&
                   character.currentRegion == InnerMapManager.Instance.currentlyShowingLocation;
        }
        #endregion
        

        protected override void ConstructSteps() {
            steps = new List<TutorialQuestStepCollection>() {
                new TutorialQuestStepCollection(new ClickOnCharacterStep("Find a dead Resident", 
                        IsCharacterValid).SetCompleteAction(OnFindDeadCharacter)
                )
            };
        }


        #region Step Helpers
        private bool IsCharacterValid(Character character) {
            return character.IsNormalCharacter() && character.isDead;
        }
        #endregion

        #region Step Completion
        private void OnFindDeadCharacter() {
            UIManager.Instance.generalConfirmationWithVisual.ShowGeneralConfirmation("Win Condition",
                "Winning is quite simple: all Residents must be dead. Use the limited amount of abilities " +
                "you have wisely to succeed!\n\nYou can also abandon a playthrough by clicking on the Abandon button. " +
                "You will gain minimal amount of Experience Points if you do, regardless of how many Residents have " +
                "been killed.", TutorialManager.Instance.deadCharactersImage);
        }
        #endregion
    }
}