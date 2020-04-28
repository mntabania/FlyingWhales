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
            //in case basic controls tutorial has been completed in a previous play through
            if (TutorialManager.Instance.HasTutorialBeenCompleted(TutorialManager.Tutorial.Basic_Controls)) {
                TryMakeAvailable();
            }
        }
        protected override void ConstructSteps() {
            steps = new List<TutorialQuestStepCollection>() {
                new TutorialQuestStepCollection(
                    new ClickOnEmptyAreaStep(), 
                    new ObjectPickerShownStep("Click on Build Structure button", "Demonic Structure"), 
                    new StructureBuiltStep(STRUCTURE_TYPE.THE_KENNEL, "Build a Kennel")
                        .SetCompleteAction(OnKennelBuilt)),
                new TutorialQuestStepCollection(new DropCharacterAtStructureStep(STRUCTURE_TYPE.THE_KENNEL, typeof(Summon), "Seize a monster and drop it at the Kennel."))
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
        private void TryMakeAvailable() {
            if (PlayerSkillManager.Instance.GetDemonicStructureSkillData(SPELL_TYPE.THE_KENNEL).charges > 0) {
                MakeAvailable();
            }
        }

        #region Availability Listeners
        private void OnTutorialQuestCompleted(TutorialQuest tutorialQuest) {
            if (tutorialQuest.tutorialType == TutorialManager.Tutorial.Basic_Controls) {
                Messenger.RemoveListener<TutorialQuest>(Signals.TUTORIAL_QUEST_COMPLETED, OnTutorialQuestCompleted);
                TryMakeAvailable();
            }
        }
        private void OnAlreadyBuiltStructure(LocationStructure structure) {
            if (structure is DemonicStructure) {
                CompleteTutorial(); //player already built a structure
            }
        }
        #endregion

        #region Step Completion Actions
        private void OnKennelBuilt() {
            UIManager.Instance.generalConfirmationWithVisual.ShowGeneralConfirmation("Demonic Structures",
                "These are unique demonic structures that you can build on unoccupied Areas. " +
                "Each structure type has a unique use that may aid you in your invasion. For example, " +
                "the Kennel allows you to take any monster you manage to keep within it, " +
                "and retain it for future use in a different playthrough.",
                TutorialManager.Instance.demonicStructureVideoClip);
        }
        #endregion
    }
}