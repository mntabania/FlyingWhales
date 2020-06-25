using System.Collections.Generic;
using Quests;
using Quests.Steps;
using UnityEngine;
namespace Tutorial {
    public class Afflictions : ImportantTutorial {
        
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
                    new ClickOnCharacterStep($"Click on a Villager", IsCharacterValid)
                        .SetHoverOverAction(OnHoverSelectCharacterStep)
                        .SetHoverOutAction(UIManager.Instance.HideSmallInfo),
                    new ObjectPickerShownStep("Click on Afflict button", "Intervention Ability")
                        .SetHoverOverAction(OnHoverAfflictButtonStep)
                        .SetHoverOutAction(UIManager.Instance.HideSmallInfo)
                        .SetOnTopmostActions(OnTopMostAfflict, OnNoLongerTopMostAfflict),
                    new ExecuteAfflictionStep("Apply Vampirism", SPELL_TYPE.VAMPIRISM)
                        .SetCompleteAction(OnCompleteExecuteAffliction)
                        .SetOnTopmostActions(OnTopMostVampirism, OnNoLongerTopMostVampirism)
                ),
                // new QuestStepCollection(
                //     new FlawClickedStep("Click on the added Affliction", "Vampiric")
                //         .SetHoverOverAction(OnHoverAfflictDetails)
                //         .SetHoverOutAction(UIManager.Instance.HideSmallInfo),
                //     new FlawTriggeredStep("Trigger it")
                // )
                
            };
        }

        #region Step Helpers
        private bool IsCharacterValid(Character character) {
            return character.isNormalCharacter;
        }
        private void OnHoverSelectCharacterStep(QuestStepItem item) {
            UIManager.Instance.ShowSmallInfo("There are some characters that are <color=\"green\">Blessed</color>. " +
                                             "These characters cannot be directly affected by your spells. " +
                                             "You will need to find other ways to deal with them.",
                TutorialManager.Instance.blessedVideoClip, "Blessed Characters", item.hoverPosition
            );
        }
        private void OnCompleteExecuteAffliction() {
            UIManager.Instance.generalConfirmationWithVisual.ShowGeneralConfirmation("Afflictions",
                $"These are negative Traits that you may apply to a world's " +
                $"\nVillager that will affect their behavior. " +
                "Afflictions do not have any\n Mana Cost but they have a limited number of Charges.\n\n" +
                "There are a vast number of different types of Afflictions you may experiment with. " +
                "You can turn someone into a Psychopath or a Vampire, or you can afflict one with a Zombie Virus.",
                TutorialManager.Instance.afflictionsVideoClip
            );
        }
        private void OnHoverAfflictButtonStep(QuestStepItem item) {
            UIManager.Instance.ShowSmallInfo("The afflict button can be seen beside the selected character's nameplate",
                TutorialManager.Instance.afflictButtonVideoClip, "How to Afflict", item.hoverPosition
            );
        }
        private void OnHoverAfflictDetails(QuestStepItem item) {
            UIManager.Instance.ShowSmallInfo($"Open the Villager's Info Menu and click on the recently added Affliction.",
                TutorialManager.Instance.afflictionDetailsVideoClip, "Affliction Details", item.hoverPosition
            );
        }
        #endregion

        #region Affliction Button
        private void OnTopMostAfflict() {
            Messenger.Broadcast(Signals.SHOW_SELECTABLE_GLOW, "Afflict");
        }
        private void OnNoLongerTopMostAfflict() {
            Messenger.Broadcast(Signals.HIDE_SELECTABLE_GLOW, "Afflict");
        }
        #endregion
        
        #region Vampirism Button
        private void OnTopMostVampirism() {
            Messenger.Broadcast(Signals.SHOW_SELECTABLE_GLOW, "Vampirism");
        }
        private void OnNoLongerTopMostVampirism() {
            Messenger.Broadcast(Signals.HIDE_SELECTABLE_GLOW, "Vampirism");
        }
        #endregion
    }
}