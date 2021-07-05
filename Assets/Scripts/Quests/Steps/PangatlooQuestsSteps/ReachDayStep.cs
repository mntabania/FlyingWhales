using System;
namespace Quests.Steps {
    public class ReachDayStep : QuestStep {
        private readonly Func<int, string> _descriptionGetter;
        private readonly int _targetDay;
        
        public ReachDayStep(Func<int, string> p_descriptionGetter, int p_day) : base(string.Empty) {
            _descriptionGetter = p_descriptionGetter;
            _targetDay = p_day;
        }
        protected override void SubscribeListeners() {
            // QuestManager.Instance.GetWinConditionTracker<WipeOutAllUntilDayWinConditionTracker>().Subscribe(this);
        }
        protected override void UnSubscribeListeners() {
            // QuestManager.Instance.GetWinConditionTracker<WipeOutAllUntilDayWinConditionTracker>().Unsubscribe(this);
        }
        protected override bool CheckIfStepIsAlreadyCompleted() {
            return GameManager.Instance.continuousDays >= _targetDay;
        }
        public void OnCharacterEliminated(Character p_character, int p_villagersCount) { }
        public void OnCharacterAddedAsTarget(Character p_character, int p_villagersCount) { }
        public void OnDayChangedAction(int p_currentDay, int p_villagersCount) {
            if (_targetDay == p_currentDay) {
                Complete();
            }
            Messenger.Broadcast(UISignals.UPDATE_QUEST_STEP_ITEM, this as QuestStep);
        }
        protected override string GetStepDescription() {
            if (_descriptionGetter != null) {
                return _descriptionGetter.Invoke(_targetDay);
            }
            return base.GetStepDescription();
        }
    }
}