using System;
using System.Collections.Generic;

namespace Quests.Steps {
    public class EliminateAllVillagersOnGivenDateStep : QuestStep, PangatLooWinConditionTracker.Listener {
        private readonly Func<List<Character>, int, string> _descriptionGetter;

        public EliminateAllVillagersOnGivenDateStep(Func<List<Character>, int, string> descriptionGetter) : base(string.Empty) {
            _descriptionGetter = descriptionGetter;
        }

        protected override void SubscribeListeners() {
            QuestManager.Instance.GetWinConditionTracker<PangatLooWinConditionTracker>().Subscribe(this);
        }
        protected override void UnSubscribeListeners() {
            QuestManager.Instance.GetWinConditionTracker<PangatLooWinConditionTracker>().Unsubscribe(this);
        }

        private void CheckForCompletion(int p_currentDay, int p_villagersCount) {
            if (p_currentDay > PangatLooWinConditionTracker.DueDay && p_villagersCount <= 0) {
                Complete();
                Messenger.Broadcast(PlayerSignals.WIN_GAME, $"You've successfully wiped out all villagers before Day {PangatLooWinConditionTracker.DueDay.ToString()}. Congratulations!");
            }
        }

        #region Listeners
        public void OnCharacterEliminated(Character p_character) {
            objectsToCenter?.Remove(p_character);
            Messenger.Broadcast(UISignals.UPDATE_QUEST_STEP_ITEM, this as QuestStep);
        }
        public void OnCharacterAddedAsTarget(Character p_character) {
            objectsToCenter?.Add(p_character);
            Messenger.Broadcast(UISignals.UPDATE_QUEST_STEP_ITEM, this as QuestStep);
        }

        public void OnDayChangedAction(int p_currentDay, int p_villagersCount) {
            CheckForCompletion(p_currentDay, p_villagersCount);
        }
       #endregion

        #region Description
        protected override string GetStepDescription() {
            if (_descriptionGetter != null) {
                return _descriptionGetter.Invoke(QuestManager.Instance.GetWinConditionTracker<PangatLooWinConditionTracker>().villagersToEliminate, QuestManager.Instance.GetWinConditionTracker<PangatLooWinConditionTracker>().totalCharactersToEliminate);
            }
            return base.GetStepDescription();
        }
        #endregion
    }
}