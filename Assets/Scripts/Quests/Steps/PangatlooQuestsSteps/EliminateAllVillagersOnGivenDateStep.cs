using System;
using System.Collections.Generic;

namespace Quests.Steps {
    public class EliminateAllVillagersOnGivenDateStep : QuestStep {
        private readonly Func<List<Character>, int, string> _descriptionGetter;

        public EliminateAllVillagersOnGivenDateStep(Func<List<Character>, int, string> descriptionGetter) : base(string.Empty) {
            _descriptionGetter = descriptionGetter;
        }

        protected override void SubscribeListeners() {
            // QuestManager.Instance.GetWinConditionTracker<WipeOutAllUntilDayWinConditionTracker>().Subscribe(this);
        }
        protected override void UnSubscribeListeners() {
            // QuestManager.Instance.GetWinConditionTracker<WipeOutAllUntilDayWinConditionTracker>().Unsubscribe(this);
        }

        private void CheckForCompletion(int p_villagersCount) {
            if (p_villagersCount <= 0) {
                Complete();
                Messenger.Broadcast(PlayerSignals.WIN_GAME, $"You've successfully wiped out all villagers before Day {(WipeOutAllUntilDayWinConditionTracker.DueDay + 1).ToString()}. Congratulations!");
            }
        }

        #region Listeners
        public void OnCharacterEliminated(Character p_character, int p_villagersCount) {
            objectsToCenter?.Remove(p_character);
            Messenger.Broadcast(UISignals.UPDATE_QUEST_STEP_ITEM, this as QuestStep);
            CheckForCompletion(p_villagersCount);
        }
        public void OnCharacterAddedAsTarget(Character p_character, int p_villagersCount) {
            objectsToCenter?.Add(p_character);
            Messenger.Broadcast(UISignals.UPDATE_QUEST_STEP_ITEM, this as QuestStep);
        }

        public void OnDayChangedAction(int p_currentDay, int p_villagersCount) {
            CheckForCompletion(p_villagersCount);
        }
       #endregion

        #region Description
        // protected override string GetStepDescription() {
        //     if (_descriptionGetter != null) {
        //         return _descriptionGetter.Invoke(QuestManager.Instance.GetWinConditionTracker<WipeOutAllUntilDayWinConditionTracker>().villagersToEliminate, QuestManager.Instance.GetWinConditionTracker<WipeOutAllUntilDayWinConditionTracker>().totalCharactersToEliminate);
        //     }
        //     return base.GetStepDescription();
        // }
        #endregion
    }
}