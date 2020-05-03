using System;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
namespace Tutorial {
    public class DefendAStructure : TutorialQuest {
        public DefendAStructure() : base("Defend A Structure", TutorialManager.Tutorial.Defend_A_Structure) { }

        #region Criteria
        protected override void ConstructCriteria() {
            _activationCriteria = new List<TutorialQuestCriteria>() {
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
            steps = new List<TutorialQuestStepCollection>() {
                new TutorialQuestStepCollection(new ClickOnStructureStep("Select a Demonic Structure", "Demonic")),
                new TutorialQuestStepCollection(
                    new ActivateDefendStep(),
                    new ExecutedPlayerActionStep(SPELL_TYPE.DEFEND, "Summon at least one defender")
                )
            };
        }
    }
}