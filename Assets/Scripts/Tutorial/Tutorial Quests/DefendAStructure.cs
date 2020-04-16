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
        public override void ConstructSteps() {
            steps = new List<TutorialQuestStep>() {
                new ClickOnStructureStep("Select a Demonic Structure", "Demonic"),
                new ExecutedPlayerActionStep(SPELL_TYPE.DEFEND, "Click on Defend and Choose at least one defender")
            };
        }

        #region Listeners
        private void OnStructurePlaced(LocationStructure structure) {
            if (structure is DemonicStructure) {
                MakeAvailable();
            }
        }
        #endregion
    }
}