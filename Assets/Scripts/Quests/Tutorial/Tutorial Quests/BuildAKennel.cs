using System;
using System.Collections;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Quests;
using Quests.Steps;
using UnityEngine;
namespace Tutorial {
    public class BuildAKennel : TutorialQuest {

        public BuildAKennel() : base("Kennel", TutorialManager.Tutorial.Build_A_Kennel) { }

        #region Criteria
        protected override void ConstructCriteria() {
            _activationCriteria = new List<TutorialQuestCriteria>() {
                new HasCompletedTutorialQuest(TutorialManager.Tutorial.Torture_Chambers)
            };
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
                ),
                new QuestStepCollection(new DropCharacterAtStructureStep(STRUCTURE_TYPE.THE_KENNEL, typeof(Summon), "Seize a monster and drop it at the Kennel."))
            };
        }
    }
}