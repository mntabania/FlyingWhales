namespace Tutorial {
    public abstract class TutorialQuestCriteria {
        
        public bool hasCriteriaBeenMet { get; private set; }

        /// <summary>
        /// Enable this criteria. Meaning, wait for this criteria to be met.
        /// </summary>
        public abstract void Enable();
        /// <summary>
        /// Disable this criteria. Doing this will prevent this criteria from being met/unmet.
        /// </summary>
        public abstract void Disable();
        
        protected virtual void SetCriteriaAsMet() {
            hasCriteriaBeenMet = true;
            Messenger.Broadcast(Signals.TUTORIAL_QUEST_CRITERIA_MET, this);
        }
        protected virtual void SetCriteriaAsUnMet() {
            hasCriteriaBeenMet = false;
            Messenger.Broadcast(Signals.TUTORIAL_QUEST_CRITERIA_UNMET, this);
        }

    }
}