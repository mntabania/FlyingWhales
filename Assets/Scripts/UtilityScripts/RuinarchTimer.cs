using UnityEngine;
namespace UtilityScripts {
    /// <summary>
    /// Utility class for creating a timer that has an effect at the end.  
    /// </summary>
    [System.Serializable]
    public class RuinarchTimer : RuinarchProgressable {

        public string timerName;
        public GameDate timerStart;
        public GameDate timerEnd;
        
        private System.Action _onTimerEndAction;

        #region getters
        public override string progressableName => timerName;
        public int totalTicksInTimer => totalValue;
        public int currentTimerProgress => currentValue;
        public override BOOKMARK_TYPE bookmarkType => BOOKMARK_TYPE.Progress_Bar;
        #endregion
        
        public RuinarchTimer(string p_name) : base() {
            timerName = p_name;
        }
        public void SetTimerName(string p_name) {
            timerName = p_name;
        }
        public void LoadStart(System.Action p_endAction = null) {
            Load();
            _onTimerEndAction = p_endAction;
            Messenger.AddListener(Signals.TICK_ENDED, TimerTick);
        }
        public void Start(GameDate p_start, GameDate p_end, System.Action p_endAction = null) {
            timerStart = p_start;
            timerEnd = p_end;
            int totalTicks = p_start.GetTickDifference(p_end);
            Setup(0, totalTicks);
            _onTimerEndAction = p_endAction;
            Messenger.AddListener(Signals.TICK_ENDED, TimerTick);
            Debug.Log($"Started {timerName}. ETA is {p_end.ToString()}");
        }
        public bool IsFinished() {
            return currentTimerProgress == totalTicksInTimer;
        }
        private void TimerTick() {
            if (IsComplete()) {
                TimerHasReachedEnd();
            }
            IncreaseProgress(1);
        }
        private void TimerHasReachedEnd() {
            _onTimerEndAction?.Invoke();
            Stop();
        }
        /// <summary>
        /// Return a float value representing this timers current progress.
        /// </summary>
        /// <returns>A percentage value divided by 100 (eg 1% = 0.01f, 25% = 0.25f, etc.)</returns>
        public float GetCurrentTimerProgressPercent() {
            float currentTimerProgressPercent = (float) currentTimerProgress / totalTicksInTimer;
            if (float.IsNaN(currentTimerProgressPercent)) {
                currentTimerProgressPercent = 0f;
            }
            return currentTimerProgressPercent;
        }
        public int GetRemainingTicks() {
            return totalTicksInTimer - currentTimerProgress;
        }
        public string GetRemainingTimeString() {
            int remainingTicks = GetRemainingTicks();
            return $"{GameManager.GetTimeAsWholeDuration(remainingTicks).ToString()} {GameManager.GetTimeIdentifierAsWholeDuration(remainingTicks)}";
        }
        public string GetTimerEndString() {
            return $"{timerEnd.ToString()}";
        }
        public void Stop() {
            Reset();
            timerStart = default;
            timerEnd = default;
            _onTimerEndAction = null;
            Messenger.RemoveListener(Signals.TICK_ENDED, TimerTick);
            Debug.Log($"Stopped {timerName}");
        }
    }
}