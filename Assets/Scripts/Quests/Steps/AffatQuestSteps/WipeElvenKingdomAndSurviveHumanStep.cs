using System;
using System.Collections.Generic;

namespace Quests.Steps {
    public class WipeElvenKingdomAndSurviveHumanStep : QuestStep, AffattWinConditionTracker.Listener {
        private readonly Func<List<Character>, int, string> _descriptionGetter;

        public WipeElvenKingdomAndSurviveHumanStep(Func<List<Character>, int, string> descriptionGetter) : base(string.Empty) {
            _descriptionGetter = descriptionGetter;
        }

        protected override void SubscribeListeners() {
            QuestManager.Instance.GetWinConditionTracker<AffattWinConditionTracker>().Subscribe(this);
        }
        protected override void UnSubscribeListeners() {
            QuestManager.Instance.GetWinConditionTracker<AffattWinConditionTracker>().Unsubscribe(this);
        }

        private void CheckForCompletion(int p_elvenCount, int p_humanCount) {
            if (p_elvenCount <= 0) {
                Complete();
                Messenger.Broadcast(PlayerSignals.WIN_GAME, $"You managed to wipe out {QuestManager.Instance.GetWinConditionTracker<AffattWinConditionTracker>().GetMainElvenFaction().name}. Congratulations!");
            }
        }

        #region Listeners
        public void OnCharacterEliminated(Character p_character, int p_elvenCount, int p_humanCount) {
            objectsToCenter?.Remove(p_character);
            Messenger.Broadcast(UISignals.UPDATE_QUEST_STEP_ITEM, this as QuestStep);
            CheckForCompletion(p_elvenCount, p_humanCount);
        }
        public void OnCharacterAddedAsTarget(Character p_character, int p_elvenCount, int p_humanCount) {
            objectsToCenter?.Add(p_character);
            Messenger.Broadcast(UISignals.UPDATE_QUEST_STEP_ITEM, this as QuestStep);
            CheckForCompletion(p_elvenCount, p_humanCount);
        }

        #endregion

        #region Description
        protected override string GetStepDescription() {
            if (_descriptionGetter != null) {
                return _descriptionGetter.Invoke((QuestManager.Instance.winConditionTracker as AffattWinConditionTracker).elvenToEliminate, (QuestManager.Instance.winConditionTracker as AffattWinConditionTracker).elvenToEliminate.Count);
            }
            return base.GetStepDescription();
        }
        #endregion
    }
}