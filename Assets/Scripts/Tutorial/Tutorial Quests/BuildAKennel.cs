using System;
using System.Collections;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UnityEngine;
namespace Tutorial {
    public class BuildAKennel : TutorialQuest {

        public BuildAKennel() : base("Build A Kennel", TutorialManager.Tutorial.Build_A_Kennel) { }
        public override void WaitForAvailability() {
            Messenger.AddListener<TutorialQuest>(Signals.TUTORIAL_QUEST_COMPLETED, OnTutorialQuestCompleted);
            Messenger.AddListener<LocationStructure>(Signals.STRUCTURE_OBJECT_PLACED, OnAlreadyBuiltStructure);
        }
        public override void ConstructSteps() {
            steps = new List<TutorialQuestStep>() {
                new ClickOnEmptyAreaStep(), 
                new ObjectPickerShownStep("Click on Build Structure button", "Demonic Structure"), 
                new StructureBuiltStep(STRUCTURE_TYPE.THE_KENNEL, "Build a Kennel"), 
                new DropCharacterAtStructureStep(STRUCTURE_TYPE.THE_KENNEL, typeof(Summon), "Seize a monster and drop it at the Kennel.")
            };
        }
        public override void Activate() {
            base.Activate();
            //stop listening for structure building, since another listener will be used to listen for step completion
            Messenger.RemoveListener<LocationStructure>(Signals.STRUCTURE_OBJECT_PLACED, OnAlreadyBuiltStructure);
        }
        protected override void StopWaitingForAvailability() {
            base.StopWaitingForAvailability();
            Messenger.RemoveListener<TutorialQuest>(Signals.TUTORIAL_QUEST_COMPLETED, OnTutorialQuestCompleted);
        }

        #region Availability Listeners
        private void OnTutorialQuestCompleted(TutorialQuest tutorialQuest) {
            if (tutorialQuest.tutorialType == TutorialManager.Tutorial.Basic_Controls) {
                Messenger.RemoveListener<TutorialQuest>(Signals.TUTORIAL_QUEST_COMPLETED, OnTutorialQuestCompleted);
                // if (PlayerSkillManager.Instance.GetPlayerSkillData(SPELL_TYPE.BUILD_DEMONIC_STRUCTURE).charges >= 1) {
                    MakeAvailable();
                // }
            }
        }
        private void OnAlreadyBuiltStructure(LocationStructure structure) {
            if (structure is DemonicStructure) {
                CompleteTutorial(); //player already built a structure
            }
        }
        #endregion
    }
}