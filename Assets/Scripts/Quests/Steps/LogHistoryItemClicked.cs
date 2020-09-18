using System;
namespace Quests.Steps {
    public class LogHistoryItemClicked : QuestStep {
        private readonly Func<object, string, IPointOfInterest, bool> _validityChecker;
        public LogHistoryItemClicked(string stepDescription, System.Func<object, string, IPointOfInterest, bool> validityChecker) : base(stepDescription) {
            _validityChecker = validityChecker;
        }
        protected override void SubscribeListeners() {
            Messenger.AddListener<object, string, IPointOfInterest>(Signals.LOG_HISTORY_OBJECT_CLICKED, CheckForCompletion);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<object, string, IPointOfInterest>(Signals.LOG_HISTORY_OBJECT_CLICKED, CheckForCompletion);
        }

        #region Listeners
        private void CheckForCompletion(object o, string log, IPointOfInterest poi) {
            if (_validityChecker.Invoke(o, log, poi)) {
                Complete();
            }
        }
        #endregion
    }
}