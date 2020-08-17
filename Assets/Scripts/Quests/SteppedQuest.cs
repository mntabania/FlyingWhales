using System.Collections.Generic;
using System.Linq;
using Quests.Steps;
using UnityEngine.Assertions;
namespace Quests {
    /// <summary>
    /// Base class for quests that have steps.
    /// </summary>
    public abstract class SteppedQuest : Quest {
        
        public List<QuestStepCollection> steps { get; protected set; }
        public QuestStepCollection activeStepCollection { get; private set; }
        public QuestItem questItem { get; private set; }
        
        protected SteppedQuest(string _questName) : base(_questName) { }
        
        #region Steps
        /// <summary>
        /// Construct this quests' steps.
        /// </summary>
        protected abstract void ConstructSteps();
        #endregion

        #region Availability
        protected override void MakeAvailable() {
            base.MakeAvailable();
            ConstructSteps();
        }
        #endregion

        #region Activation
        public override void Activate() {
            Assert.IsNotNull(steps, $"{questName} was activated but its steps are not yet constructed!");
            Assert.IsTrue(steps.Count > 0, $"{questName} is activated but it has no steps to complete!");
            Messenger.AddListener<QuestStepCollection>(Signals.STEP_COLLECTION_COMPLETED, OnStepCollectionCompleted);
            base.Activate();
            //activate first collection
            QuestStepCollection stepCollection = steps[0];
            stepCollection.Activate();
            activeStepCollection = stepCollection;
        }
        public override void Deactivate() {
            //cleanup steps
            activeStepCollection?.Deactivate();
            base.Deactivate();
        }
        private void OnStepCollectionCompleted(QuestStepCollection completedCollection) {
            if (steps.Contains(completedCollection)) {
                completedCollection.Deactivate();
                if (CheckForCompletion() == false) {
                    //activate next step
                    int nextIndex = steps.IndexOf(completedCollection) + 1;
                    Assert.IsTrue(nextIndex < steps.Count, $"Not all steps of tutorial {questName} has been completed, but no more steps are left!");
                    QuestStepCollection stepCollection = steps[nextIndex];
                    stepCollection.Activate();
                    activeStepCollection = stepCollection;
                    //only re layout if completed collection has a different number of steps compared to the new active one. 
                    questItem.UpdateStepsDelayed(completedCollection.steps.Count != activeStepCollection.steps.Count);
                }
            }
        }
        private bool CheckForCompletion() {
            if (steps.Any(s => s.isComplete == false) == false) {
                //check if any steps are not yet completed, if there are none, then this tutorial has been completed
                CompleteQuest();
                return true;
            }
            return false;
        }
        #endregion

        #region Failure
        protected override void FailQuest() {
            base.FailQuest();
            if (activeStepCollection != null && activeStepCollection.steps != null) {
                //fail uncompleted steps.
                for (int i = 0; i < activeStepCollection.steps.Count; i++) {
                    QuestStep step = activeStepCollection.steps[i];
                    if (step.isCompleted == false) {
                        step.FailStep();
                    }
                }    
            }
        }
        #endregion
        
        #region UI
        public void SetQuestItem(QuestItem questItem) {
            this.questItem = questItem;
        }
        #endregion
    }
}