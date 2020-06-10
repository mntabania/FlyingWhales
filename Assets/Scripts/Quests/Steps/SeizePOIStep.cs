using System;
using JetBrains.Annotations;
namespace Quests.Steps {
    public class SeizePOIStep : QuestStep {
        
        private readonly Func<IPointOfInterest, bool> _validityChecker;
        
        public SeizePOIStep(string stepDescription, [NotNull]System.Func<IPointOfInterest, bool> validityChecker) : base(stepDescription) {
            _validityChecker = validityChecker;
        }
        
        protected override void SubscribeListeners() {
            Messenger.AddListener<IPointOfInterest>(Signals.ON_SEIZE_POI, CheckForCompletion);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<IPointOfInterest>(Signals.ON_SEIZE_POI, CheckForCompletion);
        }

        #region Completion
        private void CheckForCompletion(IPointOfInterest poi) {
            if (_validityChecker.Invoke(poi)) {
                Complete();
            }
        }
        #endregion
    }
}