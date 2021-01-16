using System;
using System.Collections.Generic;
using System.Linq;
using Quests.Steps;
using UnityEngine;
namespace Quests {
    public class DeclareWarQuest : ReactionQuest {

        private DeclareWarQuestStep _declareWarSteps;

        #region getters
        public override Type serializedData => typeof(DeclareWarQuest);
        #endregion

        public DeclareWarQuest() : base($"Daclare War 3 times (Must be different factions)") { }
        protected override void ConstructSteps() {
            _declareWarSteps = new DeclareWarQuestStep(GetEliminateAllVillagersDescription);
            steps = new List<QuestStepCollection>() {
                new QuestStepCollection(_declareWarSteps),
            };
        }

        #region Step Helpers
        private string GetEliminateAllVillagersDescription(int p_warCount) {
            return $"War Declaration remaining: {p_warCount.ToString()}"; // /{totalCharactersToEliminate.ToString()}
        }
        #endregion
    }

    public class SaveDeclareWarQuest : SaveDataReactionQuest {
        public override ReactionQuest Load() {
            return new DeclareWarQuest();
        }
    }
}