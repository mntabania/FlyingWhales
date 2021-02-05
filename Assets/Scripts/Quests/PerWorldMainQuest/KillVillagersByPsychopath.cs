using System;
using System.Collections.Generic;
using System.Linq;
using Quests.Steps;
using UnityEngine;
namespace Quests {
    public class KillVillagersByPsychopath : ReactionQuest {
        
        #region getters
        public override Type serializedData => typeof(SaveDataKillVillagersByPsychopath);
        #endregion

        public KillVillagersByPsychopath() : base("Use Psychopath to eliminate Villagers") { }
        protected override void ConstructSteps() {
            steps = new List<QuestStepCollection>();
            IcalawaWinConditionTracker winConditionTracker = QuestManager.Instance.GetWinConditionTracker<IcalawaWinConditionTracker>();
            QuestStep turnVillagerToPsychopathStep = new TurnAVillagerToPsychopathStep(GetTurnVillagerToPsychopathDescription);
            turnVillagerToPsychopathStep.SetObjectsToCenter(winConditionTracker.psychoPath);
            QuestStep eliminateVillagerStep = new KillVillagersByPsychopathStep(GetEliminateAllVillagersDescription);
            steps.Add(new QuestStepCollection(turnVillagerToPsychopathStep, eliminateVillagerStep));
        }
        public override void Activate() {
            base.Activate();
            Messenger.AddListener<string, string>(CharacterSignals.RENAME_CHARACTER, OnCharacterRenamed);
        }
        public override void Deactivate() {
            base.Deactivate();
            Messenger.RemoveListener<string, string>(CharacterSignals.RENAME_CHARACTER, OnCharacterRenamed);
        }
        
        private void OnCharacterRenamed(string p_characterPID, string p_newName) {
            if (QuestManager.Instance.GetWinConditionTracker<IcalawaWinConditionTracker>().psychoPath.persistentID == p_characterPID) {
                UpdateAllStepsUI();
            }
        }

        #region Step Helpers
        private string GetEliminateAllVillagersDescription(int p_totalCharactersToEliminate) {
            return $"Remaining targets: {p_totalCharactersToEliminate.ToString()}";
        }

        private string GetTurnVillagerToPsychopathDescription(string p_villagerName) {
            return $"Turn {p_villagerName} to a psychopath ";
        }
        #endregion
    }

    public class SaveDataKillVillagersByPsychopath : SaveDataReactionQuest {
        public override ReactionQuest Load() {
            return new KillVillagersByPsychopath();
        }
    }
}