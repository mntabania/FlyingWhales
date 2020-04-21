using System.Collections.Generic;
using System.Linq;
namespace Tutorial {
    public class TutorialQuestStepCollection {
        public List<TutorialQuestStep> steps { get; }
        public bool isComplete { get; private set; }

        public TutorialQuestStepCollection(params TutorialQuestStep[] _steps) {
            steps = new List<TutorialQuestStep>(_steps);
            isComplete = false;
        }

        public void Activate() {
            Messenger.AddListener<TutorialQuestStep>(Signals.TUTORIAL_STEP_COMPLETED, OnTutorialStepCompleted);
            for (int i = 0; i < steps.Count; i++) {
                TutorialQuestStep step = steps[i];
                step.Activate();
            }
        }
        public void Deactivate() {
            Messenger.RemoveListener<TutorialQuestStep>(Signals.TUTORIAL_STEP_COMPLETED, OnTutorialStepCompleted);
            for (int i = 0; i < steps.Count; i++) {
                TutorialQuestStep step = steps[i];
                step.Cleanup();
            }
        }

        #region Listeners
        private void OnTutorialStepCompleted(TutorialQuestStep tutorialQuestStep) {
            if (steps.Contains(tutorialQuestStep)) {
                CheckForCompletion();
            }
        }
        #endregion

        #region Completion
        private void CheckForCompletion() {
            if (steps.Any(s => s.isCompleted == false) == false) {
                CompleteStepCollection();
            }
        }
        private void CompleteStepCollection() {
            isComplete = true;
            Messenger.Broadcast(Signals.TUTORIAL_STEP_COLLECTION_COMPLETED, this);
        }
        #endregion
    }
}