using System;
using System.Collections.Generic;
using System.Linq;
using Quests.Steps;
using UnityEngine;
namespace Quests {
    public class KillVillagersByPsychopath : ReactionQuest {


        private KillVillagersByPsychopathStep _eliminateVillagerStep;

        #region getters
        public override Type serializedData => typeof(SaveDataKillVillagersByPsychopath);
        #endregion

        public KillVillagersByPsychopath() : base($"Eliminate 5 Villagers using a psychopath") { }
        protected override void ConstructSteps() {
            QuestStep turnVillagerToPsychopathStep = new TurnAvillagerToPsychopathStep(GetTurnVillagerToPsychopathDescription);
            _eliminateVillagerStep = new KillVillagersByPsychopathStep(GetEliminateAllVillagersDescription);
            _eliminateVillagerStep.SetObjectsToCenter((QuestManager.Instance.winConditionTracker as IcalawaWinConditionTracker).villagersToEliminate.Count > 0
                ? (QuestManager.Instance.winConditionTracker as IcalawaWinConditionTracker).villagersToEliminate.Select(x => x as ISelectable).ToList()
                : new List<ISelectable>());
            steps = new List<QuestStepCollection>() {
                new QuestStepCollection(turnVillagerToPsychopathStep),
                new QuestStepCollection(_eliminateVillagerStep),
            };
        }

        #region Step Helpers
        private string GetEliminateAllVillagersDescription(int p_totalCharactersToEliminate) {
            return $"Villagers to be killed by psychopath: {p_totalCharactersToEliminate}"; // /{totalCharactersToEliminate.ToString()}
        }

        private string GetTurnVillagerToPsychopathDescription(string p_villagerName) {
            return $"Turn {p_villagerName} to a psychopath "; // /{totalCharactersToEliminate.ToString()}
        }
        #endregion
    }

    public class SaveDataKillVillagersByPsychopath : SaveDataReactionQuest {
        public override ReactionQuest Load() {
            return new KillVillagersByPsychopath();
        }
    }
}