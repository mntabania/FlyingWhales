using System;
using System.Collections.Generic;

namespace Quests.Steps {
    public class EliminateAllVillagersOnGivenDateStep : QuestStep, PangatlooWinConditionTracker.Listener {
        private readonly Func<List<Character>, int, string> _descriptionGetter;

        public EliminateAllVillagersOnGivenDateStep(Func<List<Character>, int, string> descriptionGetter) : base(string.Empty) {
            _descriptionGetter = descriptionGetter;
        }

        protected override void SubscribeListeners() {
            (QuestManager.Instance.winConditionTracker as PangatlooWinConditionTracker).Subscribe(this);
        }
        protected override void UnSubscribeListeners() {
            (QuestManager.Instance.winConditionTracker as PangatlooWinConditionTracker).Unsubscribe(this);
        }

        private void CheckForCompletion(int p_currentDay, int p_villagersCount) {
            if (p_currentDay > 8 && p_villagersCount <= 0) {
                Complete();
                Messenger.Broadcast(PlayerSignals.WIN_GAME);
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
                return _descriptionGetter.Invoke((QuestManager.Instance.winConditionTracker as PangatlooWinConditionTracker).villagersToEliminate, (QuestManager.Instance.winConditionTracker as PangatlooWinConditionTracker).totalCharactersToEliminate);
            }
            return base.GetStepDescription();
        }
        #endregion
    }
}