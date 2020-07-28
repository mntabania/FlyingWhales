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
                return PlayerManager.Instance.player.playerSkillComponent.minionsSkills.Count > 0 
                       || PlayerManager.Instance.player.playerSkillComponent.summonsSkills.Count > 0;
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
                    new ChooseSpellStep(IsChosenSpellValid, "Select an Invader-type Demon"),
                    new ExecuteSpellStep(IsChosenSpellValid, "Spawn anywhere")
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