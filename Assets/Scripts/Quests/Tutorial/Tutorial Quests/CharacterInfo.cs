using System.Collections;
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
                    new ClickOnCharacterStep("Click on a Villager", validityChecker: IsSelectedCharacterValid),
                    new ToggleTurnedOnStep("CharacterInfo_Info", "Open its Info tab"),
                    new ToggleTurnedOnStep("CharacterInfo_Mood", "Open its Mood tab"),
                    new ToggleTurnedOnStep("CharacterInfo_Relations", "Open its Relations tab"),
                    new ToggleTurnedOnStep("CharacterInfo_Logs", "Open its Logs tab")
                )
            };
        }
        #endregion

        #region Step Helpers
        private bool IsSelectedCharacterValid(Character character) {
            return character.isNormalCharacter;
        }
        #endregion
    }
}