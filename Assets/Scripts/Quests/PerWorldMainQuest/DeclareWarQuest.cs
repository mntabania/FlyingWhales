using System;
using System.Collections.Generic;
using System.Linq;
using Quests.Steps;
using UnityEngine;
namespace Quests {
    public class DeclareWarQuest : ReactionQuest {
        
        #region getters
        public override Type serializedData => typeof(SaveDeclareWarQuest);
        #endregion

        public DeclareWarQuest() : base($"Trigger 3 Concurrent Wars") { }
        protected override void ConstructSteps() {
            steps = new List<QuestStepCollection>() {
                new QuestStepCollection(new DeclareWarQuestStep(GetEliminateAllVillagersDescription)),
            };
        }

        #region Step Helpers
        private string GetEliminateAllVillagersDescription(int p_warCount) {
            return $"Major Active Wars: {p_warCount.ToString()}/{ZenkoWinConditionTracker.ActiveWarRequirement.ToString()}";
        }
        #endregion
    }

    public class SaveDeclareWarQuest : SaveDataReactionQuest {
        public override ReactionQuest Load() {
            return new DeclareWarQuest();
        }
    }
}