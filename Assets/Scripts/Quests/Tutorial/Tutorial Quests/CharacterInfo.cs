﻿using System.Collections;
using System.Collections.Generic;
using Quests;
using Quests.Steps;
using UnityEngine;
namespace Tutorial {
    public class CharacterInfo : TutorialQuest {
        
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
                    new ClickOnCharacterStep($"Click on a {UtilityScripts.Utilities.VillagerIcon()}Villager", validityChecker: IsSelectedCharacterValid),
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
                $"The Info tab provides you with basic information about the {UtilityScripts.Utilities.VillagerIcon()}Villager such as its Combat Stats, " +
                "Affiliations, temporary Statuses, permanent Traits and Items held.",
                TutorialManager.Instance.infoTab);
        }
        private void OnClickMood() {
            UIManager.Instance.generalConfirmationWithVisual.ShowGeneralConfirmation("Mood Tab",
                $"The Mood tab provides you with an overview of the {UtilityScripts.Utilities.VillagerIcon()}Villager's current state of mind. " +
                $"A {UtilityScripts.Utilities.VillagerIcon()}Villager's Mood is primarily affected by Statuses. " +
                $"The lower a {UtilityScripts.Utilities.VillagerIcon()}Villager's Mood is, the less cooperative it is with others, and may even eventually run amok!" +
                $"\n\nA {UtilityScripts.Utilities.VillagerIcon()}Villager also has several Needs that apply various Statuses depending on how high or low they are.",
                TutorialManager.Instance.moodTab);
        }
        private void OnClickRelations() {
            UIManager.Instance.generalConfirmationWithVisual.ShowGeneralConfirmation("Relations Tab",
                $"The Relations tab shows a {UtilityScripts.Utilities.VillagerIcon()}Villager's relationship with its neighbors. " +
                $"A {UtilityScripts.Utilities.VillagerIcon()}Villager will not cooperate with its enemies, " +
                "so one subtle way of reducing a Village's power is by having its residents dislike each other.",
                TutorialManager.Instance.relationsTab);
        }
        private void OnClickLogs() {
            UIManager.Instance.generalConfirmationWithVisual.ShowGeneralConfirmation("Log Tab",
                $"The Log tab provides you with a timestamped list of what a \n{UtilityScripts.Utilities.VillagerIcon()}Villager has done.",
                TutorialManager.Instance.logsTab);
        }
        #endregion

        #region Info Tab
        private void OnTopMostInfo() {
            Messenger.Broadcast(Signals.SHOW_SELECTABLE_GLOW, "Info Toggle");
        }
        private void OnNoLongerTopMostInfo() {
            Messenger.Broadcast(Signals.HIDE_SELECTABLE_GLOW, "Info Toggle");
        }
        #endregion
        
        #region Mood Tab
        private void OnTopMostMood() {
            Messenger.Broadcast(Signals.SHOW_SELECTABLE_GLOW, "Mood Toggle");
        }
        private void OnNoLongerTopMostMood() {
            Messenger.Broadcast(Signals.HIDE_SELECTABLE_GLOW, "Mood Toggle");
        }
        #endregion
        
        #region Relations Tab
        private void OnTopMostRelations() {
            Messenger.Broadcast(Signals.SHOW_SELECTABLE_GLOW, "Relations Toggle");
        }
        private void OnNoLongerTopMostRelations() {
            Messenger.Broadcast(Signals.HIDE_SELECTABLE_GLOW, "Relations Toggle");
        }
        #endregion
        
        #region Logs Tab
        private void OnTopMostLogs() {
            Messenger.Broadcast(Signals.SHOW_SELECTABLE_GLOW, "Logs Toggle");
        }
        private void OnNoLongerTopMostLogs() {
            Messenger.Broadcast(Signals.HIDE_SELECTABLE_GLOW, "Logs Toggle");
        }
        #endregion
    }
}