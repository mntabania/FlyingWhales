namespace Quests {
    public abstract class QuestCriteria {
        
        public bool hasCriteriaBeenMet { get; private set; }
        private System.Action<QuestCriteria> _onCriteriaMetAction;
        
        /// <summary>
        /// Enable this criteria. Meaning, wait for this criteria to be met.
        /// </summary>
        public abstract void Enable();
        /// <summary>
        /// Disable this criteria. Doing this will prevent this criteria from being met/unmet.
        /// </summary>
        public abstract void Disable();

        public QuestCriteria SetOnMeetAction(System.Action<QuestCriteria> onCriteriaMetAction) {
            _onCriteriaMetAction = onCriteriaMetAction;
            return this;
        }
        
        protected virtual void SetCriteriaAsMet() {
            hasCriteriaBeenMet = true;
            _onCriteriaMetAction?.Invoke(this);
            Messenger.Broadcast(Signals.QUEST_CRITERIA_MET, this);
        }
        protected virtual void SetCriteriaAsUnMet() {
            hasCriteriaBeenMet = false;
            Messenger.Broadcast(Signals.QUEST_CRITERIA_UNMET, this);
        }

    }
}