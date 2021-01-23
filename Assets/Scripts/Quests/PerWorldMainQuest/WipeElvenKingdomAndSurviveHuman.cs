using System;
using System.Collections.Generic;
using System.Linq;
using Quests.Steps;
using UnityEngine;
namespace Quests {
    public class WipeElvenKingdomAndSurviveHuman : ReactionQuest {

        private Faction _mainElvenFaction;
        private Faction _mainHumanFaction;
        
        #region getters
        public override Type serializedData => typeof(SaveWipeElvenKingdomAndSurviveHuman);
        #endregion

        public WipeElvenKingdomAndSurviveHuman() : base(string.Empty) {
            _mainElvenFaction = QuestManager.Instance.GetWinConditionTracker<AffattWinConditionTracker>().GetMainElvenFaction();
            _mainHumanFaction = QuestManager.Instance.GetWinConditionTracker<AffattWinConditionTracker>().GetMainHumanFaction();
            ChangeQuestName($"Wipe out {_mainElvenFaction.nameWithColor}");

        }
        protected override void ConstructSteps() {
            var eliminateElvenStep = new WipeElvenKingdomAndSurviveHumanStep(GetEliminateAllVillagersDescription);
            var protectHumanStep = new ProtectHumansStep(GetProtectHumanDescription);
            steps = new List<QuestStepCollection>() {
                new QuestStepCollection(eliminateElvenStep, protectHumanStep),
            };
        }

        #region Step Helpers
        private string GetEliminateAllVillagersDescription(List<Character> remainingTargets, int totalCharactersToEliminate) {
            return $"Wipe {_mainElvenFaction.nameWithColor}. Remaining {remainingTargets.Count.ToString()}";
        }
        private string GetProtectHumanDescription(List<Character> remainingTargets, int totalCharactersToEliminate) {
            return $"Protect the humans. Remaining {remainingTargets.Count.ToString()}/{AffattWinConditionTracker.MinimumHumans.ToString()}";
        }
        #endregion
    }

    public class SaveWipeElvenKingdomAndSurviveHuman : SaveDataReactionQuest {
        public override ReactionQuest Load() {
            return new WipeElvenKingdomAndSurviveHuman();
        }
    }
}