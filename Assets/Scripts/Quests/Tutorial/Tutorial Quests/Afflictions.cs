using System.Collections.Generic;
using Quests;
using Quests.Steps;
using UnityEngine;
namespace Tutorial {
    public class Afflictions : TutorialQuest {
        
        public Afflictions() : base("Afflictions", TutorialManager.Tutorial.Afflictions) { }

        #region Criteria
        protected override void ConstructCriteria() {
            _activationCriteria = new List<QuestCriteria>() {
                new HasCompletedTutorialQuest(TutorialManager.Tutorial.Torture_Chambers),
            };
        }
        protected override bool HasMetAllCriteria() {
            bool hasMetAllCriteria = base.HasMetAllCriteria();
            if (hasMetAllCriteria) {
                return PlayerManager.Instance.player.playerSkillComponent.HasAnyAvailableAffliction();
            }
            return false;
        }
        #endregion
        
        protected override void ConstructSteps() {
            steps = new List<QuestStepCollection>() {
                new QuestStepCollection(
                    new ClickOnCharacterStep("Click on a Villager", IsCharacterValid)
                        .SetHoverOverAction(OnHoverSelectCharacterStep)
                        .SetHoverOutAction(UIManager.Instance.HideSmallInfo)
                    ),
                new QuestStepCollection(
                    new ObjectPickerShownStep("Click on Afflict button", "Intervention Ability")
                        .SetHoverOverAction(OnHoverAfflictButtonStep)
                        .SetHoverOutAction(UIManager.Instance.HideSmallInfo),
                    new ExecuteAfflictionStep("Choose an Affliction to apply").SetCompleteAction(OnCompleteExecuteAffliction)
                ),
                new QuestStepCollection(
                    new FlawClickedStep("Click on the added Affliction")
                        .SetHoverOverAction(OnHoverAfflictDetails)
                        .SetHoverOutAction(UIManager.Instance.HideSmallInfo),
                    new FlawTriggeredStep("Trigger it")
                )
                
            };
        }

        #region Step Helpers
        private bool IsCharacterValid(Character character) {
            return character.isNormalCharacter && character.traitContainer.HasTrait("Blessed") == false;
        }
        private void OnHoverSelectCharacterStep(QuestStepItem item) {
            UIManager.Instance.ShowSmallInfo("There are some characters that are <color=\"green\">Blessed</color>. " +
                                             "These characters cannot be directly affected by your spells. " +
                                             "You will need to find other ways to deal with them.",
                TutorialManager.Instance.blessedVideoClip, "Blessed Characters", item.hoverPosition);
        }
        private void OnCompleteExecuteAffliction() {
            UIManager.Instance.generalConfirmationWithVisual.ShowGeneralConfirmation("Afflictions",
                "These are negative Traits that you may apply to a world's Resident that will affect their behavior. " +
                "Afflictions do not have any Mana Cost but they have a limited number of Charges.\n\n" +
                "There are a vast number of different types of Afflictions you may experiment with. " +
                "You can turn someone into a Psychopath or a Vampire, or you can afflict one with a Zombie Virus.",
                TutorialManager.Instance.afflictionsVideoClip);
        }
        private void OnHoverAfflictButtonStep(QuestStepItem item) {
            UIManager.Instance.ShowSmallInfo("The afflict button can be seen beside the selected character's nameplate",
                TutorialManager.Instance.afflictButtonVideoClip, "How to Afflict", item.hoverPosition);
        }
        private void OnHoverAfflictDetails(QuestStepItem item) {
            UIManager.Instance.ShowSmallInfo("Open the Villager's Info Menu and click on the recently added Affliction.",
                TutorialManager.Instance.afflictionDetailsVideoClip, "Affliction Details", item.hoverPosition);
        }
        #endregion
    }
}