using System;
using System.Collections.Generic;
using System.Linq;
using Quests.Steps;
using UnityEngine;
namespace Quests {
    public class EliminateNumberOfVillagersUsingPlague : ReactionQuest {


        private EliminateNumberOfVillagersUsingPlagueStep _eliminateVillagerStep;

        #region getters
        public override Type serializedData => typeof(SaveDataEliminateNumberOfVillagersUsingPlague);
        #endregion

        public EliminateNumberOfVillagersUsingPlague() : base($"Eliminate 10 Villager using Plague") { }
        protected override void ConstructSteps() {
            _eliminateVillagerStep = new EliminateNumberOfVillagersUsingPlagueStep(GetEliminateAllVillagersDescription);
            _eliminateVillagerStep.SetObjectsToCenter((QuestManager.Instance.winConditionTracker as AneemWinConditionTracker).villagersToEliminate.Count > 0
                ? (QuestManager.Instance.winConditionTracker as AneemWinConditionTracker).villagersToEliminate.Select(x => x as ISelectable).ToList()
                : new List<ISelectable>());
            steps = new List<QuestStepCollection>() {
                new QuestStepCollection(_eliminateVillagerStep),
            };
        }

        #region Step Helpers
        private string GetEliminateAllVillagersDescription(List<Character> remainingTargets, int totalCharactersToEliminate) {
            return $"Remaining villagers to be plagued: {totalCharactersToEliminate.ToString()}"; // /{totalCharactersToEliminate.ToString()}
        }
        #endregion
    }

    public class SaveDataEliminateNumberOfVillagersUsingPlague : SaveDataReactionQuest {
        public override ReactionQuest Load() {
            return new EliminateNumberOfVillagersUsingPlague();
        }
    }
}