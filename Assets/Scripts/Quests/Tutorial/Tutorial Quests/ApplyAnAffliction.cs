using System.Collections.Generic;
using Quests;
using Quests.Steps;
using UnityEngine;
namespace Tutorial {
    public class ApplyAnAffliction : TutorialQuest {
        public override int priority => 40;
        
        public ApplyAnAffliction() : base("Apply an Affliction", TutorialManager.Tutorial.Apply_An_Affliction) { }


        #region Criteria
        protected override void ConstructCriteria() {
            _activationCriteria = new List<TutorialQuestCriteria>() {
                new PlayerHasNotCastedForSeconds(15f),
                new PlayerHasNotCompletedTutorialInSeconds(15f)
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
                )
            };
        }

        #region Step Helpers
        private bool IsCharacterValid(Character character) {
            return character.IsNormalCharacter() && character.traitContainer.HasTrait("Blessed") == false;
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
        #endregion
    }
}