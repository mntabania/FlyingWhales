using System;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Quests;
using Quests.Steps;
namespace Tutorial {
    public class DefendAStructure : TutorialQuest {
        public DefendAStructure() : base("Defend A Structure", TutorialManager.Tutorial.Defend_A_Structure) { }

        #region Criteria
        protected override void ConstructCriteria() {
            _activationCriteria = new List<QuestCriteria>() {
                new DemonicStructurePlaced()
            };
        }
        protected override bool HasMetAllCriteria() {
            bool hasMetAllCriteria = base.HasMetAllCriteria();
            if (hasMetAllCriteria) {
                return PlayerManager.Instance.player.playerSkillComponent.CanDoPlayerAction(SPELL_TYPE.DEFEND);
            }
            return false;
        }
        #endregion
        
        protected override void ConstructSteps() {
            steps = new List<QuestStepCollection>() {
                new QuestStepCollection(new ClickOnStructureStep("Select a Demonic Structure", "Demonic")),
                new QuestStepCollection(
                    new ActivateDefendStep(),
                    new ExecutedPlayerActionStep(SPELL_TYPE.DEFEND, "Summon at least one defender")
                )
            };
        }
    }
}