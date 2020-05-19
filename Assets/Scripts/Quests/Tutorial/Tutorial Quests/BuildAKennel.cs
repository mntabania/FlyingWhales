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
            _activationCriteria = new List<QuestCriteria>() {
                new HasCompletedTutorialQuest(TutorialManager.Tutorial.Elemental_Interactions)
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
                    new StructureBuiltStep(STRUCTURE_TYPE.THE_KENNEL, "Choose the Kennel")
                ),
                new QuestStepCollection(
                    new DropCharacterAtStructureStep(STRUCTURE_TYPE.THE_KENNEL, typeof(Summon), "Drop a monster at the Kennel."),
                    new ClickOnCharacterStep("Click on the monster", IsCharacterValid),
                    new ExecutedPlayerActionStep(SPELL_TYPE.BREED_MONSTER, "Breed it.")
                        .SetHoverOverAction(OnHoverBreed)
                        .SetHoverOutAction(UIManager.Instance.HideSmallInfo)
                )
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
            if (structure is Inner_Maps.Location_Structures.TheKennel) {
                CompleteQuest(); //player already built a kennel
            }
        }
        #endregion

        #region Step Completion Actions
        private bool IsCharacterValid(Character character) {
            return character is Summon && character.currentStructure is Inner_Maps.Location_Structures.TheKennel;
        }
        private void OnHoverBreed(QuestStepItem stepItem) {
            UIManager.Instance.ShowSmallInfo(
                "Breeding a monster inside the Kennel gives you 1 Summon Charge of that monster type. " +
                "You can use this charge for various actions - defend Structures, invade Villages, kill Villagers.",
                TutorialManager.Instance.breedVideoClip, "Breeding", stepItem.hoverPosition
            );
        }
        #endregion
    }
}