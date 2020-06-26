using System.Collections.Generic;
using Quests;
using Quests.Steps;

namespace Tutorial {
    public class Griefstricken : LogQuest {

        private Character _targetCharacter;
        
        public Griefstricken() : base("Griefstricken", TutorialManager.Tutorial.Griefstricken) { }

        #region Criteria
        protected override void ConstructCriteria() {
            _activationCriteria = new List<QuestCriteria>() {
                new CharacterGainedTrait("Griefstricken").SetOnMeetAction(OnCharacterGriefstricken)
            };
        }
        private void OnCharacterGriefstricken(QuestCriteria criteria) {
            if (criteria is CharacterGainedTrait characterGainedTrait) {
                _targetCharacter = characterGainedTrait.character;
            }
        }
        #endregion

        #region Availability
        protected override void MakeAvailable() {
            base.MakeAvailable();
            TutorialManager.Instance.ActivateTutorialButDoNotShow(this);
            PlayerUI.Instance.ShowGeneralConfirmation("Griefstricken Characters", 
                $"A Villager is {UtilityScripts.Utilities.ColorizeAction("Griefstricken")}! " +
                $"Griefstricken Villagers may sometimes {UtilityScripts.Utilities.ColorizeAction("refuse to eat")}. " +
                "A Tutorial Quest has been added to teach you how to figure out what happened.", 
                onClickOK: () => TutorialManager.Instance.ShowTutorial(this)
            );
        }
        #endregion

        #region Activation
        public override void Activate() {
            base.Activate();
            Messenger.AddListener<Log, IPointOfInterest>(Signals.LOG_REMOVED, OnLogRemoved);
        }
        public override void Deactivate() {
            base.Deactivate();
            Messenger.RemoveListener<Log, IPointOfInterest>(Signals.LOG_REMOVED, OnLogRemoved);
        }
        private void OnLogRemoved(Log log, IPointOfInterest poi) {
            if (poi == _targetCharacter && log.file.Equals("Griefstricken")) {
                //check if target character still has any logs about griefstricken
                if (poi.logComponent.GetLogsInCategory("Griefstricken").Count == 0) {
                    //consider this quest as failed if all griefstricken logs of this character has been deleted.
                    TutorialManager.Instance.FailTutorialQuest(this); 
                }
            }
        }
        #endregion
        
        #region Steps
        protected override void ConstructSteps() {
            steps = new List<QuestStepCollection>() {
                new QuestStepCollection(
                    new ClickOnCharacterStep("Find the griefstricken", IsCharacterValid).SetObjectsToCenter(_targetCharacter),
                    new ToggleTurnedOnStep("CharacterInfo_Logs", "Click on Log tab", () => UIManager.Instance.GetCurrentlySelectedPOI() == _targetCharacter),
                    new LogHistoryItemClicked("Find the cause", IsClickedLogObjectValid)
                        .SetHoverOverAction(OnHoverFindCause)
                        .SetHoverOutAction(UIManager.Instance.HideSmallInfo)
                )
            };
        }
        #endregion

        #region Step Helpers
        private bool IsCharacterValid(Character character) {
            return character == _targetCharacter;
        }
        private bool IsClickedLogObjectValid(object obj, Log log, IPointOfInterest owner) {
            if (owner == _targetCharacter && obj is Character clickedCharacter && clickedCharacter != _targetCharacter
                && log.file.Equals("Griefstricken") && log.key.Equals("gain")) {
                return true;
            }
            return false;
        }
        private void OnHoverFindCause(QuestStepItem item) {
            UIManager.Instance.ShowSmallInfo(
                "Check out the Villager's Log to see which event triggered this reaction " +
                $"and then {UtilityScripts.Utilities.ColorizeAction("click on the relevant name")} to find it. " +
                "You may also check out their Relationship to understand why.",
                TutorialManager.Instance.griefstrickenLog, "Browsing Logs", item.hoverPosition
            );
        }
        #endregion
    }
}