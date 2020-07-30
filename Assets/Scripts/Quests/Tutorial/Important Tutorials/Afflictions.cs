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
                new HasCompletedTutorialQuest(TutorialManager.Tutorial.Prison),
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
                    new ButtonClickedStep("Afflict", "Click on Afflict button")
                        .SetHoverOverAction(OnHoverAfflictButtonStep)
                        .SetHoverOutAction(UIManager.Instance.HideSmallInfo)
                        .SetOnTopmostActions(OnTopMostAfflict, OnNoLongerTopMostAfflict)
                        .SetCompleteAction(OnCompleteExecuteAffliction),
                    new ExecuteAfflictionStep("Apply Vampirism", SPELL_TYPE.VAMPIRISM)
                        .SetOnTopmostActions(OnTopMostVampirism, OnNoLongerTopMostVampirism)
                ),
                new QuestStepCollection(
                    new ButtonClickedStep("Trigger Flaw", "Click on Trigger Flaw button")
                        .SetOnTopmostActions(OnTopMostTriggerFlawButton, OnNoLongerTopMostTriggerFlawButton)
                        .SetCompleteAction(OnCompleteTriggerFlaw),
                    new FlawTriggeredStep("Select Vampirism", "Vampiric")
                        .SetOnTopmostActions(OnTopMostTriggerVampiric, OnNoLongerTopMostTriggerVampiric)
                )
                
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
            PlayerUI.Instance.ShowGeneralConfirmation("Afflictions",
                $"Afflictions are {UtilityScripts.Utilities.ColorizeAction("negative Traits")} that you may apply to a world's " +
                $"\nVillager that will affect their behavior. " +
                "Afflictions do not have any\n Mana Cost but they have a limited number of Charges.\n\n" +
                "There are a vast number of different types of Afflictions you may experiment with. " +
                "You can turn someone into a Psychopath or a Vampire, or you can afflict one with a Zombie Virus."
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
        private void OnCompleteTriggerFlaw() {
            PlayerUI.Instance.ShowGeneralConfirmation("Trigger Flaw",
                $"Trigger Flaw is a special ability that allows you to force a Villager to perform actions " +
                $"related to one of their negative Traits (aka Flaws). Not all Flaws have associated Trigger Flaw effects but most do."
            );
        }
        #endregion

        #region Trigger Flaw Button
        private void OnTopMostTriggerFlawButton() {
            Messenger.Broadcast(Signals.SHOW_SELECTABLE_GLOW, "Trigger Flaw");
        }
        private void OnNoLongerTopMostTriggerFlawButton() {
            Messenger.Broadcast(Signals.HIDE_SELECTABLE_GLOW, "Trigger Flaw");
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
        
        #region Vampiric Trigger Flaw
        private void OnTopMostTriggerVampiric() {
            Messenger.Broadcast(Signals.SHOW_SELECTABLE_GLOW, "Vampiric");
        }
        private void OnNoLongerTopMostTriggerVampiric() {
            Messenger.Broadcast(Signals.HIDE_SELECTABLE_GLOW, "Vampiric");
        }
        #endregion
    }
}