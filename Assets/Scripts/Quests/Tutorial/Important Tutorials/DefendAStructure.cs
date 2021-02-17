using System;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Quests;
using Quests.Steps;
namespace Tutorial {
    public class DefendAStructure : BonusTutorial {
        public DefendAStructure() : base("Defend A Structure", TutorialManager.Tutorial.Defend_A_Structure) { }

        #region Criteria
        protected override void ConstructCriteria() {
            _activationCriteria = new List<QuestCriteria>() {
                // new HasCompletedTutorialQuest(TutorialManager.Tutorial.Prison) 
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
        
        protected override void ConstructSteps() {
            steps = new List<QuestStepCollection>() {
                new QuestStepCollection(
                    new ToggleTurnedOnStep("Demons Tab", "Open Demon Tab")
                        .SetOnTopmostActions(OnTopMostDemonTab, OnNoLongerTopMostDemonTab),
                    new ChooseSpellStep(IsChosenSpellValid, "Select a Defender-type Demon"),
                    new MinionSummonedStep(IsSummonedMinionValid, "Spawn on a Demonic Structure")
                )
            };
        }
        
        #region Step Helpers
        private bool IsChosenSpellValid(SkillData spellData) {
            if (spellData is MinionPlayerSkill minionPlayerSkill) {
                CharacterClass characterClass = CharacterManager.Instance.GetCharacterClass(minionPlayerSkill.className);
                return characterClass.traitNameOnTamedByPlayer == "Defender";
            }
            return false;
        }
        private bool IsSummonedMinionValid(Minion minion) {
            return minion.character.traitContainer.HasTrait("Defender") &&
                   (minion.character.currentStructure is DemonicStructure || minion.character.gridTileLocation.corruptionComponent.isCorrupted);
        }
        #endregion
        
        #region Demons Tab
        private void OnTopMostDemonTab() {
            Messenger.Broadcast(UISignals.SHOW_SELECTABLE_GLOW, "Demons Tab");
        }
        private void OnNoLongerTopMostDemonTab() {
            Messenger.Broadcast(UISignals.HIDE_SELECTABLE_GLOW, "Demons Tab");
        }
        #endregion
    }
}