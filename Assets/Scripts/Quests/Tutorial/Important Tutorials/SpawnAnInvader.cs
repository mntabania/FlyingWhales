using System;
using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Quests;
using Quests.Steps;
using UnityEngine;
namespace Tutorial {
    public class SpawnAnInvader : ImportantTutorial {
        
        public SpawnAnInvader() : base("Spawn an Invader", TutorialManager.Tutorial.Spawn_An_Invader) { }

        #region Criteria
        protected override void ConstructCriteria() {
            _activationCriteria = new List<QuestCriteria>() {
                new HasCompletedTutorialQuest(TutorialManager.Tutorial.Build_A_Kennel)
            };
        }
        protected override bool HasMetAllCriteria() {
            bool hasMetAllCriteria = base.HasMetAllCriteria();
            if (hasMetAllCriteria) {
                return PlayerManager.Instance.player.playerSkillComponent.minionsSkills.Count > 0;
            }
            return false;
        }
        #endregion

        #region Overrides
        protected override void ConstructSteps() {
            steps = new List<QuestStepCollection>() {
                new QuestStepCollection(
                    new ToggleTurnedOnStep("Demons Tab", "Open Demon Tab")
                        .SetOnTopmostActions(OnTopMostDemonTab, OnNoLongerTopMostDemonTab),
                    new ChooseSpellStep(IsChosenSpellValid, "Select an Invader-type Demon")
                        .SetHoverOverAction(OnHoverDemon)
                        .SetHoverOutAction(UIManager.Instance.HideSmallInfo),
                    new ExecuteSpellStep(IsChosenSpellValid, "Spawn anywhere")
                        .SetCompleteAction(OnSpawnAction)
                ),
            };
        }
        #endregion

        #region Step Helpers
        private bool IsChosenSpellValid(SpellData spellData) {
            if (spellData is MinionPlayerSkill minionPlayerSkill) {
                CharacterClass characterClass =
                    CharacterManager.Instance.GetCharacterClass(minionPlayerSkill.className);
                return characterClass.traitNameOnTamedByPlayer == "Invader";
            }
            return false;
        }
        private void OnHoverDemon(QuestStepItem stepItem) {
            UIManager.Instance.ShowSmallInfo(
                "You have a handful of lesser demons at your command. You can summon them to perform certain actions for you. " +
                "They will de-spawn if they lose all HP and will have to fully recuperate in the nether realms before you can summon again.",
                stepItem.hoverPosition, "Lesser Demons");
        }
        private void OnSpawnAction() {
            PlayerUI.Instance.ShowGeneralConfirmation("Minion Behaviors", 
                "Each demon type has a fixed behavior. For example, an Invader-type such as " +
                "Pride and Wrath Demons will assault the nearest Villager settlement.");
        }
        #endregion

        #region Demons Tab
        private void OnTopMostDemonTab() {
            Messenger.Broadcast(Signals.SHOW_SELECTABLE_GLOW, "Demons Tab");
        }
        private void OnNoLongerTopMostDemonTab() {
            Messenger.Broadcast(Signals.HIDE_SELECTABLE_GLOW, "Demons Tab");
        }
        #endregion
    }
}