using System;
using System.Collections.Generic;
using System.Linq;
using Quests.Steps;
using UnityEngine;
namespace Quests {
    public class EliminateAllVillagersOnGivenDate : ReactionQuest {


        private EliminateAllVillagersOnGivenDateStep _eliminateVillagerStep;

        #region getters
        public override Type serializedData => typeof(SaveEliminateAllVillagersOnGivenDate);
        #endregion

        public EliminateAllVillagersOnGivenDate() : base($"Eliminate All Villagers and Finish Day 8") { }
        protected override void ConstructSteps() {
            _eliminateVillagerStep = new EliminateAllVillagersOnGivenDateStep(GetEliminateAllVillagersDescription);
            Debug.LogError(_eliminateVillagerStep);
            _eliminateVillagerStep.SetObjectsToCenter((QuestManager.Instance.winConditionTracker as PangatlooWinConditionTracker).villagersToEliminate.Count > 0
                ? (QuestManager.Instance.winConditionTracker as PangatlooWinConditionTracker).villagersToEliminate.Select(x => x as ISelectable).ToList()
                : new List<ISelectable>());
            steps = new List<QuestStepCollection>() {
                new QuestStepCollection(_eliminateVillagerStep),
            };
        }

        #region Step Helpers
        private string GetEliminateAllVillagersDescription(List<Character> remainingTargets, int totalCharactersToEliminate) {
            return $"wiped the villagers until Day 9 : {totalCharactersToEliminate.ToString()} Remaining" ; // /{totalCharactersToEliminate.ToString()}
        }
        #endregion
    }

    public class SaveEliminateAllVillagersOnGivenDate : SaveDataReactionQuest {
        public override ReactionQuest Load() {
            return new EliminateAllVillagersOnGivenDate();
        }
    }
}