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
            _mainElvenFaction = QuestManager.Instance.GetWinConditionTracker<HumansSurviveAndElvesWipedOutWinConditionTracker>().GetMainElvenFaction();
            _mainHumanFaction = QuestManager.Instance.GetWinConditionTracker<HumansSurviveAndElvesWipedOutWinConditionTracker>().GetMainHumanFaction();
            ChangeQuestName($"Wipe out {_mainElvenFaction.nameWithColor}");

        }
        protected override void ConstructSteps() {
            var eliminateElvenStep = new WipeElvenKingdomAndSurviveHumanStep(GetEliminateAllVillagersDescription);
            eliminateElvenStep.SetObjectsToCenter(QuestManager.Instance.GetWinConditionTracker<HumansSurviveAndElvesWipedOutWinConditionTracker>().elvenToEliminate.Count > 0
                ? QuestManager.Instance.GetWinConditionTracker<HumansSurviveAndElvesWipedOutWinConditionTracker>().elvenToEliminate.Select(x => x as ISelectable).ToList()
                : new List<ISelectable>());
            var protectHumanStep = new ProtectHumansStep(GetProtectHumanDescription).SetHoverOverAction(OnHoverProtectHumans).SetHoverOutAction(UIManager.Instance.HideSmallInfo);
            steps = new List<QuestStepCollection>() {
                new QuestStepCollection(eliminateElvenStep, protectHumanStep),
            };
        }

        #region Step Helpers
        private string GetEliminateAllVillagersDescription(List<Character> remainingTargets, int totalCharactersToEliminate) {
            return $"Wipe {_mainElvenFaction.nameWithColor}. Remaining {remainingTargets.Count.ToString()}";
        }
        private string GetProtectHumanDescription(List<Character> remainingTargets, int totalCharactersToEliminate) {
            return $"Protect the humans. Remaining {remainingTargets.Count.ToString()}/{HumansSurviveAndElvesWipedOutWinConditionTracker.MinimumHumans.ToString()}";
        }
        #endregion

        #region Tooltips
        private void OnHoverProtectHumans(QuestStepItem item) {
            UIManager.Instance.ShowSmallInfo(
                $"Keep at least {HumansSurviveAndElvesWipedOutWinConditionTracker.MinimumHumans.ToString()} humans alive and part of {_mainHumanFaction.nameWithColor}.\n" +
                $"Important Notes:\n " +
                $"\t- Human vagrants do not count!\n" +
                $"\t- Human Villagers cannot be replenished, while Elven Villagers can.\n" +
                $"\t- Elven vagrants are considered as eliminated.", 
                item.hoverPosition);
        }
        #endregion
    }

    public class SaveWipeElvenKingdomAndSurviveHuman : SaveDataReactionQuest {
        public override ReactionQuest Load() {
            return new WipeElvenKingdomAndSurviveHuman();
        }
    }
}