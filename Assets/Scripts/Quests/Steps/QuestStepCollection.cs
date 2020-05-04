using System.Collections.Generic;
using System.Linq;
namespace Quests.Steps {
    public class QuestStepCollection {
        public List<QuestStep> steps { get; }
        public bool isComplete { get; private set; }

        public QuestStepCollection(params QuestStep[] _steps) {
            steps = new List<QuestStep>(_steps);
            isComplete = false;
        }

        public void Activate() {
            Messenger.AddListener<QuestStep>(Signals.QUEST_STEP_COMPLETED, OnTutorialStepCompleted);
            for (int i = 0; i < steps.Count; i++) {
                QuestStep step = steps[i];
                step.Activate();
            }
        }
        public void Deactivate() {
            Messenger.RemoveListener<QuestStep>(Signals.QUEST_STEP_COMPLETED, OnTutorialStepCompleted);
            for (int i = 0; i < steps.Count; i++) {
                QuestStep step = steps[i];
                step.Cleanup();
            }
        }

        #region Listeners
        private void OnTutorialStepCompleted(QuestStep questStep) {
            if (steps.Contains(questStep)) {
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
            Messenger.Broadcast(Signals.STEP_COLLECTION_COMPLETED, this);
        }
        #endregion
    }
}