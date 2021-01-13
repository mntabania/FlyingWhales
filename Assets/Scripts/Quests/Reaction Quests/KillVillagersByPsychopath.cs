using System;
using System.Collections.Generic;
using System.Linq;
using Quests.Steps;
using UnityEngine;
namespace Quests {
    public class KillVillagersByPsychopath : ReactionQuest {


        private KillVillagersByPsychopathStep _eliminateVillagerStep;

        #region getters
        public override Type serializedData => typeof(SaveDataEliminateAllVillagers);
        #endregion

        public KillVillagersByPsychopath() : base($"Eliminate 5 Villagers using a psychopath") { }
        protected override void ConstructSteps() {
            _eliminateVillagerStep = new KillVillagersByPsychopathStep(GetEliminateAllVillagersDescription);
            _eliminateVillagerStep.SetObjectsToCenter((QuestManager.Instance.winConditionTracker as IcalawaWinConditionTracker).villagersToEliminate.Count > 0
                ? (QuestManager.Instance.winConditionTracker as IcalawaWinConditionTracker).villagersToEliminate.Select(x => x as ISelectable).ToList()
                : new List<ISelectable>());
            steps = new List<QuestStepCollection>() {
                new QuestStepCollection(_eliminateVillagerStep),
            };
        }

        #region Step Helpers
        private string GetEliminateAllVillagersDescription(int totalCharactersToEliminate) {
            return $"Villagers to be killed by psycho: {totalCharactersToEliminate}"; // /{totalCharactersToEliminate.ToString()}
        }
        #endregion
    }

    public class SaveDataEliminateNumberVillagersUsingPsychoPath : SaveDataReactionQuest {
        public override ReactionQuest Load() {
            return new KillVillagersByPsychopath();
        }
    }
}