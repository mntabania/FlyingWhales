using System;
using System.Collections;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Quests;
using Quests.Steps;
using UnityEngine;
namespace Tutorial {
    public class BuildAKennel : ImportantTutorial {

        public BuildAKennel() : base("Kennel", TutorialManager.Tutorial.Build_A_Kennel) { }

        #region Criteria
        protected override void ConstructCriteria() {
            _activationCriteria = new List<QuestCriteria>() {
                new HasCompletedTutorialQuest(TutorialManager.Tutorial.Elemental_Interactions)
            };
        }
        #endregion
      
        protected override void ConstructSteps() {
            steps = new List<QuestStepCollection>() {
                new QuestStepCollection(
                    new ToggleTurnedOnStep("Build Tab", "Open Build Menu")
                        .SetOnTopmostActions(OnTopMostBuildTab, OnNoLongerTopMostBuildTab),
                    new ToggleTurnedOnStep("Kennel", "Choose the Kennel")
                        .SetOnTopmostActions(OnTopMostKennel, OnNoLongerTopMostKennel),
                    new StructureBuiltStep(STRUCTURE_TYPE.KENNEL, "Place on an unoccupied Area")
                ),
                new QuestStepCollection(
                    new ExecutedPlayerActionStep(SPELL_TYPE.SEIZE_MONSTER, $"Seize a {UtilityScripts.Utilities.MonsterIcon()}monster")
                        .SetOnTopmostActions(OnTopMostSeizeMonster, OnNoLongerTopMostSeizeMonster),
                    new DropPOIAtStructureStep((structure, pointOfInterest) => structure.structureType == STRUCTURE_TYPE.KENNEL,
                        poi => poi is Summon, "Drop at the Kennel."),
                    new ClickOnCharacterStep($"Click on the monster", IsCharacterValid),
                    new ExecutedPlayerActionStep(SPELL_TYPE.BREED_MONSTER, "Breed it.")
                        .SetHoverOverAction(OnHoverBreed)
                        .SetHoverOutAction(UIManager.Instance.HideSmallInfo)
                        .SetOnTopmostActions(OnTopMostBreedMonster, OnNoLongerTopMostBreedMonster)
                )
            };
        }
        public override void Activate() {
            base.Activate();
            Messenger.Broadcast(UISignals.UPDATE_BUILD_LIST);
        }

        #region Step Completion Actions
        private bool IsCharacterValid(Character character) {
            return character is Summon && character.currentStructure is Inner_Maps.Location_Structures.Kennel;
        }
        #endregion

        #region Step Helpers
        private void OnHoverBreed(QuestStepItem stepItem) {
            UIManager.Instance.ShowSmallInfo(
                $"Breeding a monster inside the Kennel gives you {UtilityScripts.Utilities.ColorizeAction("1 Summon Charge")} of that monster type. " +
                $"You can use summoned monsters for different purposes - their behavior will depend on their monster type. " +
                $"For example, if you breed a {UtilityScripts.Utilities.ColorizeAction("Wolf")}, it would be an {UtilityScripts.Utilities.ColorizeAction("Invader")} and will assault the nearest settlement.",
                TutorialManager.Instance.breedVideoClip, "Breeding", stepItem.hoverPosition
            );
        }
        private void OnHoverClickEmptyTile(QuestStepItem stepItem) {
            UIManager.Instance.ShowSmallInfo("Suggestion: choose an empty area far away from the Village", 
                stepItem.hoverPosition, "Choosing where to Build");
        }
        #endregion
        
        #region Build Structure Button
        private void OnTopMostBuildStructure() {
            Messenger.Broadcast(UISignals.SHOW_SELECTABLE_GLOW, "Build Structure");
        }
        private void OnNoLongerTopMostBuildStructure() {
            Messenger.Broadcast(UISignals.HIDE_SELECTABLE_GLOW, "Build Structure");
        }
        #endregion
        
        #region Choose Kennel
        private void OnTopMostKennel() {
            Messenger.Broadcast(UISignals.SHOW_SELECTABLE_GLOW, "Kennel");
        }
        private void OnNoLongerTopMostKennel() {
            Messenger.Broadcast(UISignals.HIDE_SELECTABLE_GLOW, "Kennel");
        }
        #endregion
        
        #region Seize Montser
        private void OnTopMostSeizeMonster() {
            Messenger.Broadcast(UISignals.SHOW_SELECTABLE_GLOW, "Seize Monster");
        }
        private void OnNoLongerTopMostSeizeMonster() {
            Messenger.Broadcast(UISignals.HIDE_SELECTABLE_GLOW, "Seize Monster");
        }
        #endregion
        
        #region Breed Monster
        private void OnTopMostBreedMonster() {
            Messenger.Broadcast(UISignals.SHOW_SELECTABLE_GLOW, "Breed Monster");
        }
        private void OnNoLongerTopMostBreedMonster() {
            Messenger.Broadcast(UISignals.HIDE_SELECTABLE_GLOW, "Breed Monster");
        }
        #endregion
        
        #region Build Tab
        private void OnTopMostBuildTab() {
            Messenger.Broadcast(UISignals.SHOW_SELECTABLE_GLOW, "Build Tab");
        }
        private void OnNoLongerTopMostBuildTab() {
            Messenger.Broadcast(UISignals.HIDE_SELECTABLE_GLOW, "Build Tab");
        }
        #endregion
    }
}