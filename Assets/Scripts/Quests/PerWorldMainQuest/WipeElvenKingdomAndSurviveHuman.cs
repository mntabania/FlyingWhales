using System;
using System.Collections.Generic;
using System.Linq;
using Quests.Steps;
using UnityEngine;
namespace Quests {
    public class WipeElvenKingdomAndSurviveHuman : ReactionQuest {

        private WipeElvenKingdomAndSurviveHumanStep _eliminateElvensStep;

        #region getters
        public override Type serializedData => typeof(WipeElvenKingdomAndSurviveHuman);
        #endregion

        public WipeElvenKingdomAndSurviveHuman() : base($"Wipe Elven kingdom and survive atleast 5 humans") { }
        protected override void ConstructSteps() {
            _eliminateElvensStep = new WipeElvenKingdomAndSurviveHumanStep(GetEliminateAllVillagersDescription);
            _eliminateElvensStep.SetObjectsToCenter((QuestManager.Instance.winConditionTracker as AffatWinConditionTracker).elvensToEliminate.Count > 0
                ? (QuestManager.Instance.winConditionTracker as AffatWinConditionTracker).elvensToEliminate.Select(x => x as ISelectable).ToList()
                : new List<ISelectable>());
            steps = new List<QuestStepCollection>() {
                new QuestStepCollection(_eliminateElvensStep),
            };
        }

        #region Step Helpers
        private string GetEliminateAllVillagersDescription(List<Character> remainingTargets, int totalCharactersToEliminate) {
            return $"wipe elven kingdom and survive 5 humans"; // /{totalCharactersToEliminate.ToString()}
        }
        #endregion
    }

    public class SaveWipeElvenKingdomAndSurviveHuman : SaveDataReactionQuest {
        public override ReactionQuest Load() {
            return new WipeElvenKingdomAndSurviveHuman();
        }
    }
}