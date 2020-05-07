using System.Collections.Generic;
using System.Linq;
using Quests;
using UnityEngine.Assertions;

namespace Tutorial {
    public abstract class TutorialQuest : Quest {
        public TutorialManager.Tutorial tutorialType { get; }
        public virtual int priority => 0; //priority of this tutorial
        protected TutorialQuest(string _questName, TutorialManager.Tutorial _tutorialType) : base(_questName) {
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
        protected override void MakeAvailable() {
            base.MakeAvailable();
            TutorialManager.Instance.AddTutorialToWaitList(this);
        }
        protected override void MakeUnavailable() {
            base.MakeUnavailable();
            TutorialManager.Instance.RemoveTutorialFromWaitList(this);
        }
        #endregion

        #region Completion
        protected override void CompleteQuest() {
            TutorialManager.Instance.CompleteTutorialQuest(this);
        }
        #endregion
        
        #region Activation
        public override void Activate() {
            StopCheckingCriteria();
            base.Activate();
        }
        public override void Deactivate() {
            base.Deactivate();
            if (isAvailable == false) {
                //only stop waiting for availability only if tutorial has not yet been made available but has been deactivated.  
                StopCheckingCriteria();    
            }
        }
        #endregion
        
    }
}