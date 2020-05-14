using System.Collections;
using Tutorial;
using UnityEngine;
namespace Quests {
    public class PlayerHasNotCompletedTutorialInSeconds : QuestCriteria {
        
        private readonly float _neededSeconds;
        private Coroutine _timerCoroutine;
        public PlayerHasNotCompletedTutorialInSeconds(float neededSeconds) {
            _neededSeconds = neededSeconds;
        }
        
        public override void Enable() {
            Messenger.AddListener<TutorialQuest>(Signals.TUTORIAL_QUEST_COMPLETED, OnTutorialQuestCompleted);
        }
        public override void Disable() {
            Messenger.RemoveListener<TutorialQuest>(Signals.TUTORIAL_QUEST_COMPLETED, OnTutorialQuestCompleted);
        }
        
        private void OnTutorialQuestCompleted(TutorialQuest tutorialQuest) {
            RestartTimer();
            SetCriteriaAsUnMet();
        }
        
        #region Timer
        private void StartTimer() {
            _timerCoroutine = TutorialManager.Instance.StartCoroutine(WaitForSeconds());
        }
        private void StopTimer() {
            if (_timerCoroutine != null) {
                TutorialManager.Instance.StopCoroutine(_timerCoroutine);
                _timerCoroutine = null;
            }
        }
        private void RestartTimer() {
            StopTimer();
            StartTimer();
        }
        private IEnumerator WaitForSeconds() {
            yield return new WaitForSecondsRealtime(_neededSeconds);
            SetCriteriaAsMet();
        }
        #endregion
    }
}