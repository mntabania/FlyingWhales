using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;
namespace Tutorial {
    public abstract class TutorialQuest {

        public string questName { get; }
        public TutorialManager.Tutorial tutorialType { get; }
        public virtual int priority => 0; //priority of this tutorial
        public TutorialQuestItem tutorialQuestItem { get; private set; }
        public bool isAvailable { get; private set; }
        /// <summary>
        /// Is this tutorial activated? In other words, is this tutorial currently being shown to the player.
        /// </summary>
        public bool isActivated { get; private set; }
        public List<TutorialQuestStepCollection> steps { get; protected set; }
        public TutorialQuestStepCollection activeStepCollection { get; private set; }
        protected List<TutorialQuestCriteria> _activationCriteria;

        protected TutorialQuest(string _questName, TutorialManager.Tutorial _tutorialType) {
            questName = _questName;
            tutorialType = _tutorialType;
            Initialize();
        }

        #region Initialization
        private void Initialize() {
            ConstructCriteria();
            StartCheckingCriteria();
        }
        #endregion

        #region Criteria
        /// <summary>
        /// Construct the list of criteria that this quest needs to be activated.
        /// </summary>
        protected abstract void ConstructCriteria();
        /// <summary>
        /// Make this quest start checking for it's criteria
        /// </summary>
        private void StartCheckingCriteria() {
            Messenger.AddListener<TutorialQuestCriteria>(Signals.TUTORIAL_QUEST_CRITERIA_MET, OnCriteriaMet);
            Messenger.AddListener<TutorialQuestCriteria>(Signals.TUTORIAL_QUEST_CRITERIA_UNMET, OnCriteriaUnMet);
            for (int i = 0; i < _activationCriteria.Count; i++) {
                TutorialQuestCriteria criteria = _activationCriteria[i];
                criteria.Enable();
            }
        }
        private void StopCheckingCriteria() {
            Messenger.RemoveListener<TutorialQuestCriteria>(Signals.TUTORIAL_QUEST_CRITERIA_MET, OnCriteriaMet);
            Messenger.RemoveListener<TutorialQuestCriteria>(Signals.TUTORIAL_QUEST_CRITERIA_UNMET, OnCriteriaUnMet);
            for (int i = 0; i < _activationCriteria.Count; i++) {
                TutorialQuestCriteria criteria = _activationCriteria[i];
                criteria.Disable();
            }
        }
        private void OnCriteriaMet(TutorialQuestCriteria criteria) {
            if (isAvailable) { return; } //do not check criteria completion if tutorial has already been made available
            if (_activationCriteria.Contains(criteria)) {
                //check if all criteria has been met
                if (HasMetAllCriteria()) {
                    MakeAvailable();
                }
            }
        }
        private void OnCriteriaUnMet(TutorialQuestCriteria criteria) {
            if (_activationCriteria.Contains(criteria)) {
                if (isAvailable) {
                    MakeUnavailable();
                }
            }
        }
        protected virtual bool HasMetAllCriteria() {
            bool hasMetAllCriteria = true;
            for (int i = 0; i < _activationCriteria.Count; i++) {
                TutorialQuestCriteria c = _activationCriteria[i];
                if (c.hasCriteriaBeenMet == false) {
                    hasMetAllCriteria = false;
                    break;
                }
            }
            return hasMetAllCriteria;
        }
        #endregion
        
        #region Availability
        /// <summary>
        /// Make this quest available, this means that this quest is put on the list of available tutorials that the
        /// player can undertake. Usually this is preceded by this quests' criteria being met.  
        /// </summary>
        protected virtual void MakeAvailable() {
            isAvailable = true;
            ConstructSteps();
            TutorialManager.Instance.AddTutorialToWaitList(this);
        }
        /// <summary>
        /// Make this tutorial unavailable again. This assumes that this tutorial is currently on wait list.
        /// </summary>
        protected virtual void MakeUnavailable() {
            isAvailable = false;
            Assert.IsTrue(TutorialManager.Instance.IsInWaitList(this), $"{questName} is being made unavailable even though it is not the the current tutorial wait list.");
            TutorialManager.Instance.RemoveTutorialFromWaitList(this);
        }
        #endregion

        #region Activation
        /// <summary>
        /// Activate this tutorial, meaning this quest should be listening for whether its steps are completed.
        /// </summary>
        public virtual void Activate() {
            StopCheckingCriteria();
            Messenger.AddListener<TutorialQuestStepCollection>(Signals.TUTORIAL_STEP_COLLECTION_COMPLETED, OnTutorialStepCollectionCompleted);
            isActivated = true;
            //activate first collection
            TutorialQuestStepCollection stepCollection = steps[0];
            stepCollection.Activate();
            activeStepCollection = stepCollection;
        }
        public virtual void Deactivate() {
            isActivated = false;
            if (isAvailable == false) {
                //only stop waiting for availability only if tutorial has not yet been made available but has been deactivated.  
                StopCheckingCriteria();    
            }
            //cleanup steps
            activeStepCollection?.Deactivate();
        }
        #endregion

        #region Steps
        /// <summary>
        /// Construct this quests' steps.
        /// </summary>
        protected abstract void ConstructSteps();
        #endregion
        
        #region Listeners
        private void OnTutorialStepCollectionCompleted(TutorialQuestStepCollection completedCollection) {
            if (steps.Contains(completedCollection)) {
                completedCollection.Deactivate();
                if (CheckForCompletion() == false) {
                    //activate next step
                    int nextIndex = steps.IndexOf(completedCollection) + 1;
                    Assert.IsTrue(nextIndex < steps.Count, $"Not all steps of tutorial {questName} has been completed, but no more steps are left!");
                    TutorialQuestStepCollection stepCollection = steps[nextIndex];
                    stepCollection.Activate();
                    activeStepCollection = stepCollection;
                    //only re layout if completed collection has a different number of steps compared to the new active one. 
                    tutorialQuestItem.UpdateStepsDelayed(completedCollection.steps.Count != activeStepCollection.steps.Count);
                }
            }
        }
        #endregion

        #region Completion
        private bool CheckForCompletion() {
            if (steps.Any(s => s.isComplete == false) == false) {
                //check if any steps are not yet completed, if there are none, then this tutorial has been completed
                CompleteTutorial();
                return true;
            }
            return false;
        }
        protected void CompleteTutorial() {
            TutorialManager.Instance.CompleteTutorialQuest(this);
        }
        #endregion

        #region UI
        public void SetTutorialQuestItem(TutorialQuestItem tutorialQuestItem) {
            this.tutorialQuestItem = tutorialQuestItem;
        }
        #endregion

        #region Utilities
        public virtual void PerFrameActions() {}
        #endregion
    }
}