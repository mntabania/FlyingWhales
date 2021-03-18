namespace UtilityScripts {
    
    /// <summary>
    /// Base class for anything that can be interpreted into a progress bar.
    /// Mainly it has a minimum value, a max value and current value which can also be called the progress.
    /// </summary>
    [System.Serializable]
    public abstract class RuinarchProgressable : IBookmarkable {

        #region IListener
        public interface IListener {
            void OnCurrentProgressChanged(RuinarchProgressable p_progressable);
        }
        #endregion
        
        private int m_minValue;
        private int m_maxValue;
        protected int currentValue;
        protected int totalValue;
        private System.Action<RuinarchProgressable> _onCurrentProgressChanged;
        private System.Action _onSelectProgressable;
        
        public string persistentID { get; }
        public abstract string name { get; }
        public abstract BOOKMARK_TYPE bookmarkType { get; } 
        public BookmarkableEventDispatcher bookmarkEventDispatcher { get; }

        #region getters
        public string bookmarkName => name;
        #endregion

        protected RuinarchProgressable() {
            persistentID = UtilityScripts.Utilities.GetNewUniqueID();
            bookmarkEventDispatcher = new BookmarkableEventDispatcher();
        }
        
        protected void Setup(int p_minValue, int p_maxValue) {
            m_minValue = p_minValue;
            m_maxValue = p_maxValue;
            totalValue = p_maxValue - p_minValue;
            currentValue = m_minValue;
        }
        protected void Reset() {
            m_minValue = 0;
            m_maxValue = 0;
            currentValue = 0;
            totalValue = 0;
        }
        protected void IncreaseProgress(int p_amount) {
            currentValue += p_amount;
            ExecuteOnProgressChangedEvent();
        }
        public float GetCurrentProgressPercent() {
            float currentTimerProgressPercent = (float) currentValue / totalValue;
            if (float.IsNaN(currentTimerProgressPercent)) {
                currentTimerProgressPercent = 0f;
            }
            return currentTimerProgressPercent;
        }
        public bool IsComplete() {
            return currentValue >= m_maxValue;
        }

        #region Listeners
        public void ListenToProgress(IListener p_listener) {
            _onCurrentProgressChanged += p_listener.OnCurrentProgressChanged;
        }
        public void StopListeningToProgress(IListener p_listener) {
            _onCurrentProgressChanged -= p_listener.OnCurrentProgressChanged;
        }
        private void ExecuteOnProgressChangedEvent() {
            _onCurrentProgressChanged?.Invoke(this);
        }
        #endregion

        #region Interaction
        public void SetOnSelectAction(System.Action p_action) {
            _onSelectProgressable = p_action;
        }
        public void OnSelect() {
            _onSelectProgressable?.Invoke();
        }
        public void OnSelectBookmark() {
            OnSelect();
        }
        public void RemoveBookmark() {
            PlayerManager.Instance.player.bookmarkComponent.RemoveBookmark(this);
        }
        #endregion
    }
}