using System;
namespace Quests.Steps {
    public class LogHistoryItemClicked : QuestStep {
        private readonly Func<object, Log, IPointOfInterest, bool> _validityChecker;
        public LogHistoryItemClicked(string stepDescription, System.Func<object, Log, IPointOfInterest, bool> validityChecker) : base(stepDescription) {
            _validityChecker = validityChecker;
        }
        protected override void SubscribeListeners() {
            Messenger.AddListener<object, Log, IPointOfInterest>(Signals.LOG_HISTORY_OBJECT_CLICKED, CheckForCompletion);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<object, Log, IPointOfInterest>(Signals.LOG_HISTORY_OBJECT_CLICKED, CheckForCompletion);
        }

        #region Listeners
        private void CheckForCompletion(object o, Log log, IPointOfInterest poi) {
            if (_validityChecker.Invoke(o, log, poi)) {
                Complete();
            }
        }
        #endregion
    }
}