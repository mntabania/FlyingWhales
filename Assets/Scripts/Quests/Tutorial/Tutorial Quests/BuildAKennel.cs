using System;
using System.Collections;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Quests;
using Quests.Steps;
using UnityEngine;
namespace Tutorial {
    public class BuildAKennel : TutorialQuest {

        public BuildAKennel() : base("Build A Kennel", TutorialManager.Tutorial.Build_A_Kennel) { }

        #region Criteria
        protected override void ConstructCriteria() {
            _activationCriteria = new List<TutorialQuestCriteria>() {
                new HasCompletedTutorialQuest(TutorialManager.Tutorial.Basic_Controls)
            };
            Messenger.AddListener<LocationStructure>(Signals.STRUCTURE_OBJECT_PLACED, OnAlreadyBuiltStructure);
        }
        protected override bool HasMetAllCriteria() {
            bool hasMetAllCriteria = base.HasMetAllCriteria();
            if (hasMetAllCriteria) {
                return PlayerSkillManager.Instance.GetDemonicStructureSkillData(SPELL_TYPE.THE_KENNEL).charges > 0;
            }
            return false;
        }
        #endregion
      
        protected override void ConstructSteps() {
            steps = new List<QuestStepCollection>() {
                new QuestStepCollection(
                    new ClickOnEmptyAreaStep(), 
                    new ObjectPickerShownStep("Click on Build Structure button", "Demonic Structure"), 
                    new StructureBuiltStep(STRUCTURE_TYPE.THE_KENNEL, "Build a Kennel")
                        .SetCompleteAction(OnKennelBuilt)),
                new QuestStepCollection(new DropCharacterAtStructureStep(STRUCTURE_TYPE.THE_KENNEL, typeof(Summon), "Seize a monster and drop it at the Kennel."))
            };
        }
        public override void Activate() {
            base.Activate();
            //stop listening for structure building, since another listener will be used to listen for step completion
            Messenger.RemoveListener<LocationStructure>(Signals.STRUCTURE_OBJECT_PLACED, OnAlreadyBuiltStructure);
        }
        public override void Deactivate() {
            base.Deactivate();
            //remove listener, this is for when the tutorial is completed without it being activated 
            Messenger.RemoveListener<LocationStructure>(Signals.STRUCTURE_OBJECT_PLACED, OnAlreadyBuiltStructure);
        }

        #region Availability Listeners
        private void OnAlreadyBuiltStructure(LocationStructure structure) {
            if (structure is DemonicStructure) {
                CompleteQuest(); //player already built a structure
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