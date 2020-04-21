using System;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
namespace Tutorial {
    public class DefendAStructure : TutorialQuest {
        public DefendAStructure() : base("Defend A Structure", TutorialManager.Tutorial.Defend_A_Structure) { }
        public override void WaitForAvailability() {
            Messenger.AddListener<LocationStructure>(Signals.STRUCTURE_OBJECT_PLACED, OnStructurePlaced);
        }
        protected override void StopWaitingForAvailability() {
            Messenger.RemoveListener<LocationStructure>(Signals.STRUCTURE_OBJECT_PLACED, OnStructurePlaced);
        }
        protected override void ConstructSteps() {
            steps = new List<TutorialQuestStepCollection>() {
                new TutorialQuestStepCollection(new ClickOnStructureStep("Select a Demonic Structure", "Demonic")),
                new TutorialQuestStepCollection(new ExecutedPlayerActionStep(SPELL_TYPE.DEFEND, "Click on Defend and Choose at least one defender"))
            };
        }

        #region Listeners
        private void OnStructurePlaced(LocationStructure structure) {
            if (structure is DemonicStructure 
                && PlayerManager.Instance.player.playerSkillComponent.CanDoPlayerAction(SPELL_TYPE.DEFEND)) {
                MakeAvailable();
            }
        }
        #endregion
    }
}