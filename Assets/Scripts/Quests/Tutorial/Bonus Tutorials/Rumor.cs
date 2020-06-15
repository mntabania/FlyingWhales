using System.Collections;
using System.Collections.Generic;
using Quests;
using Quests.Steps;
using UnityEngine;
namespace Tutorial {
    public class Rumor : LogQuest {

        private ActualGoapNode _targetAction;
        
        public Rumor() : base("Rumor", TutorialManager.Tutorial.Rumor) { }

        #region Criteria
        protected override void ConstructCriteria() {
            _activationCriteria = new List<QuestCriteria>() {
                new CharacterFinishedAction(INTERACTION_TYPE.SHARE_INFORMATION).SetOnMeetAction(OnCharacterFinishedAction)
            };
        }
        private void OnCharacterFinishedAction(QuestCriteria criteria) {
            if (criteria is CharacterFinishedAction characterFinishedAction) {
                _targetAction = characterFinishedAction.finishedAction;
            }
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
            if (poi == _targetAction.actor && log == _targetAction.descriptionLog) {
                //consider this quest as failed if log associated with action was removed from the target object
                TutorialManager.Instance.FailTutorialQuest(this);
            }
        }
        #endregion
        
        #region Availability
        protected override void MakeAvailable() {
            base.MakeAvailable();
            TutorialManager.Instance.ActivateTutorialButDoNotShow(this);
            PlayerUI.Instance.ShowGeneralConfirmation("Rumors", 
                $"Someone is spreading an interesting {UtilityScripts.Utilities.ColorizeAction("rumor")}! A Tutorial Quest has been added to walk you through our Rumors feature.", 
                onClickOK: () => TutorialManager.Instance.ShowTutorial(this)
            );
        }
        #endregion
        
        #region Steps
        protected override void ConstructSteps() {
            steps = new List<QuestStepCollection>() {
                new QuestStepCollection(
                    new ClickOnCharacterStep("Find the Rumormonger", character => character == _targetAction.actor)
                        .SetCompleteAction(OnCompleteFindRumormonger)
                        .SetObjectsToCenter(_targetAction.actor),
                    new ToggleTurnedOnStep("CharacterInfo_Logs", "Click on Log tab", () => UIManager.Instance.GetCurrentlySelectedPOI() == _targetAction.actor),
                    new LogHistoryItemClicked("Click on Rumor recipient", IsClickedLogObjectValid)
                        .SetHoverOverAction(OnHoverRecipient)
                        .SetHoverOutAction(UIManager.Instance.HideSmallInfo)
                        .SetCompleteAction(OnCompleteReceiveRumor)
                )
            };
        }
        #endregion

        #region Step Helpers
        private bool IsClickedLogObjectValid(object obj, Log log, IPointOfInterest owner) {
            if (owner == _targetAction.actor && obj is Character clickedCharacter && clickedCharacter == _targetAction.poiTarget
                && log.file.Equals("Share Information") && log.key.Equals("share success_description")) {
                return true;
            }
            return false;
        }
        private void OnCompleteFindRumormonger() {
            TutorialManager.Instance.StartCoroutine(DelayedFindRumormongerPopup());
        }
        private IEnumerator DelayedFindRumormongerPopup() {
            yield return new WaitForSecondsRealtime(1.5f);
            PlayerUI.Instance.ShowGeneralConfirmation("Rumormonger",
                $"A {UtilityScripts.Utilities.VillagerIcon()}Villager may start spreading a rumor about " +
                $"someone that {UtilityScripts.Utilities.ColorizeAction("has recently offended them")}. " +
                "The lower their Mood, the higher the chance that this may occur." +
                $"\n\n{UtilityScripts.Utilities.ColorizeAction("Induce more negative interactions")} " +
                $"between {UtilityScripts.Utilities.VillagerIcon()}Villagers to trigger more damaging rumors!"
            );
        }
        private void OnCompleteReceiveRumor() {
            TutorialManager.Instance.StartCoroutine(DelayedReceiveRumorPopup());
        }
        private IEnumerator DelayedReceiveRumorPopup() {
            yield return new WaitForSecondsRealtime(1.5f);
            PlayerUI.Instance.ShowGeneralConfirmation("Receiving Rumors",
                $"A {UtilityScripts.Utilities.VillagerIcon()}Villager who receives a new rumor " +
                $"{UtilityScripts.Utilities.ColorizeAction("will evaluate it")} and may or may not believe it. " +
                "If they don't, they may try to confirm it with the rumor victim. " +
                "If they do, they will react to it as if it's true!"
            );
        }
        private void OnHoverRecipient(QuestStepItem item) {
            UIManager.Instance.ShowSmallInfo(
                $"A {UtilityScripts.Utilities.ColorizeAction("rumor log")} should have been registered in the Villager's" +
                $" Log tab. {UtilityScripts.Utilities.ColorizeAction("Click on the name")} of the Villager that receives it.",
                TutorialManager.Instance.recipientLog, "Infected Log", item.hoverPosition
            );
        }
        #endregion

    }
}