using System;
using System.Collections.Generic;
using System.Linq;
using Quests.Steps;
using UnityEngine;
namespace Quests {
    public class EliminateNumberOfVillagersUsingPlague : ReactionQuest {
        
        #region getters
        public override Type serializedData => typeof(SaveDataEliminateNumberOfVillagersUsingPlague);
        #endregion

        public EliminateNumberOfVillagersUsingPlague() : base($"Kill {PlagueDeathWinConditionTracker.Elimination_Requirement.ToString()} Villagers through Plague") { }
        protected override void ConstructSteps() {
            var eliminateVillagerStep = new EliminateNumberOfVillagersUsingPlagueStep(GetEliminateAllVillagersDescription);
            var aneemWinConditionTracker = QuestManager.Instance.GetWinConditionTracker<PlagueDeathWinConditionTracker>();
            eliminateVillagerStep.SetObjectsToCenter(aneemWinConditionTracker.villagersToEliminate.Count > 0
                ? aneemWinConditionTracker.villagersToEliminate.Select(x => x as ISelectable).ToList()
                : new List<ISelectable>());
            steps = new List<QuestStepCollection>() {
                new QuestStepCollection(eliminateVillagerStep),
            };
        }

        #region Step Helpers
        private string GetEliminateAllVillagersDescription(List<Character> remainingTargets, int totalCharactersToEliminate) {
            return $"Plague Fatality deaths: {(PlagueDeathWinConditionTracker.Elimination_Requirement - totalCharactersToEliminate).ToString()}/{PlagueDeathWinConditionTracker.Elimination_Requirement.ToString()}";
        }
        #endregion
    }

    public class SaveDataEliminateNumberOfVillagersUsingPlague : SaveDataReactionQuest {
        public override ReactionQuest Load() {
            return new EliminateNumberOfVillagersUsingPlague();
        }
    }
}