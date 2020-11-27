using Tutorial;
namespace Quests.Special_Popups {
    public abstract class SpecialPopup : Quest {
        public QuestManager.Special_Popup specialPopupType { get; }
        public bool isRepeatable { get; protected set; }
        
        public SpecialPopup(string _questName, QuestManager.Special_Popup popupType) : base(_questName) {
            specialPopupType = popupType;
            isRepeatable = false;
        }

        #region Initialization
        public void Initialize() {
            ConstructCriteria();
            StartCheckingCriteria();
        }
        #endregion

        #region Completion
        protected override void CompleteQuest() {
            QuestManager.Instance.CompleteQuest(this);
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
            Messenger.AddListener<QuestCriteria>(PlayerQuestSignals.QUEST_CRITERIA_MET, OnCriteriaMet);
            Messenger.AddListener<QuestCriteria>(PlayerQuestSignals.QUEST_CRITERIA_UNMET, OnCriteriaUnMet);
            for (int i = 0; i < _activationCriteria.Count; i++) {
                QuestCriteria criteria = _activationCriteria[i];
                criteria.Enable();
            }
        }
        protected void StopCheckingCriteria() {
            Messenger.RemoveListener<QuestCriteria>(PlayerQuestSignals.QUEST_CRITERIA_MET, OnCriteriaMet);
            Messenger.RemoveListener<QuestCriteria>(PlayerQuestSignals.QUEST_CRITERIA_UNMET, OnCriteriaUnMet);
            for (int i = 0; i < _activationCriteria.Count; i++) {
                QuestCriteria criteria = _activationCriteria[i];
                criteria.Disable();
            }
        }
        private void OnCriteriaMet(QuestCriteria criteria) {
            if (isAvailable) { return; } //do not check criteria completion if tutorial has already been made available
            if (_activationCriteria.Contains(criteria)) {
                TryMakeAvailable();
            }
        }
        private void OnCriteriaUnMet(QuestCriteria criteria) {
            if (_activationCriteria.Contains(criteria)) {
                if (isAvailable) {
                    MakeUnavailable();
                }
            }
        }
        /// <summary>
        /// Try and make this quest available, this will check if all criteria has been met. If it has
        /// then make it available.
        /// </summary>
        private void TryMakeAvailable() {
            //check if all criteria has been met
            if (HasMetAllCriteria()) {
                MakeAvailable();
            }
        }
        protected virtual bool HasMetAllCriteria() {
            bool hasMetAllCriteria = true;
            for (int i = 0; i < _activationCriteria.Count; i++) {
                QuestCriteria c = _activationCriteria[i];
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
            QuestManager.Instance.ActivateQuest(this);
        }
        #endregion
    }
}