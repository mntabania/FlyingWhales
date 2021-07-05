using System;
using System.Collections.Generic;
using System.Linq;
using Quests.Steps;
using UnityEngine;
namespace Quests {
    public class EliminateAllVillagersOnGivenDate : ReactionQuest {
        
        #region getters
        public override Type serializedData => typeof(SaveEliminateAllVillagersOnGivenDate);
        #endregion

        public EliminateAllVillagersOnGivenDate() : base($"Eliminate All Villagers by Day {WipeOutAllUntilDayWinConditionTracker.DueDay.ToString()}") { }
        protected override void ConstructSteps() {
            var pangatLooWinConditionTracker = QuestManager.Instance.GetWinConditionTracker<WipeOutAllUntilDayWinConditionTracker>();
            var reachDayStep = new ReachDayStep(GetReachDayDescription, WipeOutAllUntilDayWinConditionTracker.DueDay);
            var eliminateVillagerStep = new EliminateAllVillagersOnGivenDateStep(GetEliminateAllVillagersDescription);
            eliminateVillagerStep.SetObjectsToCenter(pangatLooWinConditionTracker.villagersToEliminate.Count > 0
                ? pangatLooWinConditionTracker.villagersToEliminate.Select(x => x as ISelectable).ToList()
                : new List<ISelectable>());
            steps = new List<QuestStepCollection>() {
                new QuestStepCollection(reachDayStep, eliminateVillagerStep),
            };
        }

        #region Step Helpers
        private string GetEliminateAllVillagersDescription(List<Character> remainingTargets, int totalCharactersToEliminate) {
            return $"Remaining villagers : {totalCharactersToEliminate.ToString()}" ;
        }
        private string GetReachDayDescription(int p_targetDay) {
            return $"Days until the Undead invasion: {Mathf.Max(0, p_targetDay - GameManager.Instance.continuousDays).ToString()}";
        } 
        #endregion
    }

    public class SaveEliminateAllVillagersOnGivenDate : SaveDataReactionQuest {
        public override ReactionQuest Load() {
            return new EliminateAllVillagersOnGivenDate();
        }
    }
}