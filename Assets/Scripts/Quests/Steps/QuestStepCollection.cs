using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;
namespace Quests.Steps {
    public class QuestStepCollection {
        public List<QuestStep> steps { get; }
        public bool isComplete { get; private set; }

        private QuestStep _topMostIncompleteStep;

        public QuestStepCollection(params QuestStep[] _steps) {
            steps = new List<QuestStep>(_steps);
            isComplete = false;
        }

        public void Activate() {
            Messenger.AddListener<QuestStep>(Signals.QUEST_STEP_COMPLETED, OnTutorialStepCompleted);
            for (int i = 0; i < steps.Count; i++) {
                QuestStep step = steps[i];
                step.Activate();
                if (i == 0) {
                    SetTopMostIncompleteStep(step);        
                }
            }
        }
        public void Deactivate() {
            Messenger.RemoveListener<QuestStep>(Signals.QUEST_STEP_COMPLETED, OnTutorialStepCompleted);
            DeactivateTopMostIncompleteStep();
            for (int i = 0; i < steps.Count; i++) {
                QuestStep step = steps[i];
                step.Deactivate();
            }
        }

        #region Listeners
        private void OnTutorialStepCompleted(QuestStep questStep) {
            if (steps.Contains(questStep)) {
                if (CheckForCompletion() == false) {
                    //collection is not yet completed, update top most incomplete step
                    CheckTopMostIncompleteStep();
                }
            }
        }
        #endregion

        #region Completion
        private bool CheckForCompletion() {
            if (steps.Any(s => s.isCompleted == false) == false) {
                CompleteStepCollection();
                return true;
            }
            return false;
        }
        private void CompleteStepCollection() {
            isComplete = true;
            Messenger.Broadcast(Signals.STEP_COLLECTION_COMPLETED, this);
        }
        #endregion

        #region Misc
        private void SetTopMostIncompleteStep(QuestStep step) {
            if (step == _topMostIncompleteStep) { return; } //ignore change
            
            Assert.IsTrue(steps.Contains(step),
                $"Passed step {step.stepDescription} in step collection is not part of this collection's steps");
            //deactivate previous step as top most incomplete step
            DeactivateTopMostIncompleteStep();
            
            _topMostIncompleteStep = step;
            step.SetAsTopMostIncompleteStep();
        }
        private void DeactivateTopMostIncompleteStep() {
            _topMostIncompleteStep?.NoLongerTopMostIncompleteStep();
        }
        private void CheckTopMostIncompleteStep() {
            for (int i = 0; i < steps.Count; i++) {
                QuestStep step = steps[i];
                if (step.isCompleted == false) {
                    SetTopMostIncompleteStep(step);
                    break;
                }
            }
        }
        #endregion
    }
}