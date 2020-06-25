using System.Collections;
using System.Collections.Generic;
using Quests;
using Quests.Steps;
using UnityEngine;
namespace Tutorial {
    public class CharacterInfo : ImportantTutorial {
        
        public CharacterInfo() : base("Character Info", TutorialManager.Tutorial.Character_Info) { }

        #region Criteria
        protected override void ConstructCriteria() {
            _activationCriteria = new List<QuestCriteria>() {
                new HasCompletedTutorialQuest(TutorialManager.Tutorial.Torture_Chambers),
            };
        }
        #endregion
        
        #region Overrides
        protected override void ConstructSteps() {
            steps = new List<QuestStepCollection>() {
                new QuestStepCollection(
                    new ClickOnCharacterStep($"Click on a Villager", validityChecker: IsSelectedCharacterValid),
                    new ToggleTurnedOnStep("CharacterInfo_Info", "Open its Info tab")
                        .SetCompleteAction(OnClickInfo)
                        .SetOnTopmostActions(OnTopMostInfo, OnNoLongerTopMostInfo),
                    new ToggleTurnedOnStep("CharacterInfo_Mood", "Open its Mood tab")
                        .SetCompleteAction(OnClickMood)
                        .SetOnTopmostActions(OnTopMostMood, OnNoLongerTopMostMood),
                    new ToggleTurnedOnStep("CharacterInfo_Relations", "Open its Relations tab")
                        .SetCompleteAction(OnClickRelations)
                        .SetOnTopmostActions(OnTopMostRelations, OnNoLongerTopMostRelations),
                    new ToggleTurnedOnStep("CharacterInfo_Logs", "Open its Log tab")
                        .SetCompleteAction(OnClickLogs)
                        .SetOnTopmostActions(OnTopMostLogs, OnNoLongerTopMostLogs)
                )
            };
        }
        #endregion

        #region Step Helpers
        private bool IsSelectedCharacterValid(Character character) {
            return character.isNormalCharacter;
        }
        #endregion
        
        #region Step Completion Actions
        private void OnClickInfo() {
            UIManager.Instance.generalConfirmationWithVisual.ShowGeneralConfirmation("Info Tab",
                $"The Info tab provides you with basic information about the \nVillager such as its Combat Stats, " +
                "Affiliations, temporary Statuses, permanent Traits and Items held.",
                TutorialManager.Instance.infoTab);
        }
        private void OnClickMood() {
            UIManager.Instance.generalConfirmationWithVisual.ShowGeneralConfirmation("Mood Tab",
                $"The Mood tab provides you with an overview of the Villager's current state of mind. " +
                $"A Villager's Mood is primarily affected by Statuses. " +
                $"The lower a Villager's Mood is, the less cooperative it is with others, and may even eventually run amok!" +
                $"\n\nA Villager also has several Needs that apply various Statuses depending on how high or low they are.",
                TutorialManager.Instance.moodTab);
        }
        private void OnClickRelations() {
            UIManager.Instance.generalConfirmationWithVisual.ShowGeneralConfirmation("Relations Tab",
                $"The Relations tab shows a Villager's relationship with its neighbors. " +
                $"A Villager will not cooperate with its enemies, " +
                "so one subtle way of reducing a Village's power is by having its residents dislike each other.",
                TutorialManager.Instance.relationsTab);
        }
        private void OnClickLogs() {
            UIManager.Instance.generalConfirmationWithVisual.ShowGeneralConfirmation("Log Tab",
                $"The Log tab provides you with a timestamped list of what a \nVillager has done.",
                TutorialManager.Instance.logsTab);
        }
        #endregion

        #region Info Tab
        private void OnTopMostInfo() {
            Messenger.Broadcast(Signals.SHOW_SELECTABLE_GLOW, "CharacterInfo_Info");
        }
        private void OnNoLongerTopMostInfo() {
            Messenger.Broadcast(Signals.HIDE_SELECTABLE_GLOW, "CharacterInfo_Info");
        }
        #endregion
        
        #region Mood Tab
        private void OnTopMostMood() {
            Messenger.Broadcast(Signals.SHOW_SELECTABLE_GLOW, "CharacterInfo_Mood");
        }
        private void OnNoLongerTopMostMood() {
            Messenger.Broadcast(Signals.HIDE_SELECTABLE_GLOW, "CharacterInfo_Mood");
        }
        #endregion
        
        #region Relations Tab
        private void OnTopMostRelations() {
            Messenger.Broadcast(Signals.SHOW_SELECTABLE_GLOW, "CharacterInfo_Relations");
        }
        private void OnNoLongerTopMostRelations() {
            Messenger.Broadcast(Signals.HIDE_SELECTABLE_GLOW, "CharacterInfo_Relations");
        }
        #endregion
        
        #region Logs Tab
        private void OnTopMostLogs() {
            Messenger.Broadcast(Signals.SHOW_SELECTABLE_GLOW, "CharacterInfo_Logs");
        }
        private void OnNoLongerTopMostLogs() {
            Messenger.Broadcast(Signals.HIDE_SELECTABLE_GLOW, "CharacterInfo_Logs");
        }
        #endregion
    }
}