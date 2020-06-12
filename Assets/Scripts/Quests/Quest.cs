using System.Collections.Generic;
using System.Linq;
using Quests.Steps;
using Tutorial;
using UnityEngine.Assertions;
namespace Quests {
    public abstract class Quest {

        #region Properties
        protected List<QuestCriteria> _activationCriteria;
        public string questName { get; }
        public bool isAvailable { get; protected set; }
        /// <summary>
        /// Is this quest activated? In other words, is this quest currently being shown to the player.
        /// </summary>
        public bool isActivated { get; private set; }
        public List<QuestStepCollection> steps { get; protected set; }
        public QuestStepCollection activeStepCollection { get; private set; }
        public QuestItem questItem { get; private set; }
        #endregion

        protected Quest(string _questName) {
            questName = _questName;
        }
        
        #region Steps
        /// <summary>
        /// Construct this quests' steps.
        /// </summary>
        protected abstract void ConstructSteps();
        #endregion

        #region Availability
        /// <summary>
        /// Make this quest available, this means that this quest is put on the list of available tutorials that the
        /// player can undertake. Usually this is preceded by this quests' criteria being met.  
        /// </summary>
        protected virtual void MakeAvailable() {
            isAvailable = true;
            ConstructSteps();
        }
        /// <summary>
        /// Make this tutorial unavailable again. This assumes that this tutorial is currently on wait list.
        /// </summary>
        protected virtual void MakeUnavailable() {
            isAvailable = false;
        }
        #endregion

        #region Activation
        /// <summary>
        /// Activate this tutorial, meaning this quest should be listening for whether its steps are completed.
        /// </summary>
        public virtual void Activate() {
            Assert.IsNotNull(steps, $"{questName} was activated but its steps are not yet constructed!");
            Assert.IsTrue(steps.Count > 0, $"{questName} is activated but it has no steps to complete!");
            Messenger.AddListener<QuestStepCollection>(Signals.STEP_COLLECTION_COMPLETED, OnStepCollectionCompleted);
            isActivated = true;
            //activate first collection
            QuestStepCollection stepCollection = steps[0];
            stepCollection.Activate();
            activeStepCollection = stepCollection;
        }
        public virtual void Deactivate() {
            isActivated = false;
            //cleanup steps
            activeStepCollection?.Deactivate();
            Messenger.Broadcast(Signals.QUEST_DEACTIVATED, this);
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
        #endregion

        #region Completion
        private bool CheckForCompletion() {
            if (steps.Any(s => s.isComplete == false) == false) {
                //check if any steps are not yet completed, if there are none, then this tutorial has been completed
                CompleteQuest();
                return true;
            }
            return false;
        }
        protected abstract void CompleteQuest();
        #endregion

        #region UI
        public void SetQuestItem(QuestItem questItem) {
            this.questItem = questItem;
        }
        #endregion
        
        #region Failure
        protected virtual void FailQuest() {
            //fail uncompleted steps.
            for (int i = 0; i < activeStepCollection.steps.Count; i++) {
                QuestStep step = activeStepCollection.steps[i];
                if (step.isCompleted == false) {
                    step.FailStep();
                }
            }
        }
        #endregion
    }
}