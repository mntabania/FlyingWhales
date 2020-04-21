using System.Collections.Generic;
using Inner_Maps;
namespace Tutorial {
    public class WinCondition : TutorialQuest {
        public WinCondition() : base("Win Condition", TutorialManager.Tutorial.Win_Condition) { }
        public override void WaitForAvailability() {
            Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        }
        protected override void StopWaitingForAvailability() {
            Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        }
        protected override void ConstructSteps() {
            steps = new List<TutorialQuestStepCollection>() {
                new TutorialQuestStepCollection(new ClickOnCharacterStep("Find a dead Resident",
                    validityChecker: IsCharacterDead).SetCompleteAction(OnFindDeadCharacter)
                )
            };
        }

        #region Listeners
        private void OnCharacterDied(Character character) {
            if (character.currentRegion == InnerMapManager.Instance.currentlyShowingLocation) {
                MakeAvailable();    
            }
        }
        #endregion

        #region Step Helpers
        private bool IsCharacterDead(Character character) {
            return character.isDead;
        }
        #endregion

        #region Step Completion
        private void OnFindDeadCharacter() {
            UIManager.Instance.generalConfirmationWithVisual.ShowGeneralConfirmation("Win Condition",
                "Winning is quite simple: all Residents must be dead. Use the limited amount of abilities " +
                "you have wisely to succeed!\n\nYou can also abandon a playthrough by clicking on the Abandon button. " +
                "You will gain minimal amount of Experience Points if you do, regardless of how many Residents have " +
                "been killed.", TutorialManager.Instance.spellPopUpPicture);
        }
        #endregion
    }
}