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
            steps = new List<QuestStepCollection>();
            IcalawaWinConditionTracker winConditionTracker = QuestManager.Instance.winConditionTracker as IcalawaWinConditionTracker;
            if (winConditionTracker != null && winConditionTracker.psychoPath != null && !winConditionTracker.psychoPath.traitContainer.HasTrait("Psychopath")) {
                QuestStep turnVillagerToPsychopathStep = new TurnAvillagerToPsychopathStep(GetTurnVillagerToPsychopathDescription);
                steps.Add(new QuestStepCollection(turnVillagerToPsychopathStep));
            }
            _eliminateVillagerStep = new KillVillagersByPsychopathStep(GetEliminateAllVillagersDescription);
            IcalawaWinConditionTracker icalawaWinConditionTracker = QuestManager.Instance.winConditionTracker as IcalawaWinConditionTracker;
            _eliminateVillagerStep.SetObjectsToCenter(icalawaWinConditionTracker.villagersToEliminate.Count > 0
                ? icalawaWinConditionTracker.villagersToEliminate.Select(x => x as ISelectable).ToList()
                : new List<ISelectable>());
            steps.Add(new QuestStepCollection(_eliminateVillagerStep));
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