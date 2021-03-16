using UnityEngine;
namespace UtilityScripts {
    /// <summary>
    /// Utility class for creating a timer that has an effect at the end.  
    /// </summary>
    [System.Serializable]
    public class RuinarchTimer {

        public string timerName;
        public GameDate timerStart;
        public GameDate timerEnd;
        public int totalTicksInTimer;
        public int currentTimerProgress;

        private System.Action _onTimerEndAction;

        public RuinarchTimer(string p_name) {
            timerName = p_name;
        }
        public void LoadStart(System.Action p_endAction) {
            _onTimerEndAction = p_endAction;
            Messenger.AddListener(Signals.TICK_ENDED, TimerTick);
        }
        public void Start(GameDate p_start, GameDate p_end, System.Action p_endAction) {
            timerStart = p_start;
            timerEnd = p_end;
            totalTicksInTimer = p_start.GetTickDifference(p_end);
            currentTimerProgress = 0;
            _onTimerEndAction = p_endAction;
            Messenger.AddListener(Signals.TICK_ENDED, TimerTick);
            Debug.Log($"Started {timerName}. ETA is {p_end.ToString()}");
        }
        private void TimerTick() {
            currentTimerProgress++;
            if (GameManager.Instance.Today().IsSameDate(timerEnd)) {
                TimerHasReachedEnd();
            }
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
        public void Stop() {
            timerStart = default;
            timerEnd = default;
            _onTimerEndAction = null;
            currentTimerProgress = 0;
            totalTicksInTimer = 0;
            Messenger.RemoveListener(Signals.TICK_ENDED, TimerTick);
            Debug.Log($"Stopped {timerName}");
        }
    }
}