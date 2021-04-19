using System;
using System.Collections.Generic;
using System.Linq;
using Quests.Steps;
using UnityEngine;
namespace Quests {
    public class EliminateAllVillagers : ReactionQuest {


        private EliminateVillagerStep _eliminateVillagerStep;

        #region getters
        public override Type serializedData => typeof(SaveDataEliminateAllVillagers);
        #endregion
        
        public EliminateAllVillagers() : base($"Eliminate All Villagers") { }
        protected override void ConstructSteps() {
            _eliminateVillagerStep = new EliminateVillagerStep(GetEliminateAllVillagersDescription);
            _eliminateVillagerStep.SetObjectsToCenter((QuestManager.Instance.winConditionTracker as WipeOutAllVillagersWinConditionTracker).villagersToEliminate.Count > 0
                ? (QuestManager.Instance.winConditionTracker as WipeOutAllVillagersWinConditionTracker).villagersToEliminate.Select(x => x as ISelectable).ToList()
                : new List<ISelectable>());
            steps = new List<QuestStepCollection>() {
                new QuestStepCollection(_eliminateVillagerStep),
            };
        }

        #region Step Helpers
        private string GetEliminateAllVillagersDescription(List<Character> remainingTargets, int totalCharactersToEliminate) {
            return $"Villagers Remaining: {remainingTargets.Count.ToString()}"; // /{totalCharactersToEliminate.ToString()}
        }
        #endregion
    }
    
    public class SaveDataEliminateAllVillagers : SaveDataReactionQuest {
        public override ReactionQuest Load() {
            return new EliminateAllVillagers();
        }
    }
}