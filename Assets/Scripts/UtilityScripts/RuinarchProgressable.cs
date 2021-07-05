using UnityEngine;
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
        
        public int minValue;
        public int maxValue;
        public int currentValue;
        public int totalValue;
        private System.Action<RuinarchProgressable> _onCurrentProgressChanged;
        private System.Action _onSelectProgressable;
        private System.Action<UIHoverPosition> _onHoverOverProgressable;
        private System.Action _onHoverOutProgressable;
        
        public abstract string progressableName { get; }
        public abstract BOOKMARK_TYPE bookmarkType { get; } 
        public BookmarkableEventDispatcher bookmarkEventDispatcher { get; private set; }

        #region getters
        public string bookmarkName => progressableName;
        #endregion

        protected RuinarchProgressable() {
            bookmarkEventDispatcher = new BookmarkableEventDispatcher();
        }
        public void Load() {
            bookmarkEventDispatcher = new BookmarkableEventDispatcher();
        }
        
        protected void Setup(int p_minValue, int p_maxValue) {
            minValue = p_minValue;
            maxValue = p_maxValue;
            totalValue = p_maxValue - p_minValue;
            currentValue = minValue;
        }
        protected void Reset() {
            minValue = 0;
            maxValue = 0;
            currentValue = 0;
            totalValue = 0;
        }
        public void IncreaseProgress(int p_amount) {
            currentValue += p_amount;
            currentValue = Mathf.Clamp(currentValue, 0, maxValue);
            ExecuteOnProgressChangedEvent();
        }
        public void SetProgress(int p_amount) {
            currentValue = p_amount;
            currentValue = Mathf.Clamp(currentValue, 0, maxValue);
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
            return currentValue >= maxValue;
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
        public void SetOnHoverOverAction(System.Action<UIHoverPosition> p_action) {
            _onHoverOverProgressable = p_action;
        }
        public void SetOnHoverOutAction(System.Action p_action) {
            _onHoverOutProgressable = p_action;
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
        public void OnHoverOverBookmarkItem(UIHoverPosition pos) {
            _onHoverOverProgressable?.Invoke(pos);
        }
        public void OnHoverOutBookmarkItem() {
            _onHoverOutProgressable?.Invoke();
        }
        #endregion
    }
}