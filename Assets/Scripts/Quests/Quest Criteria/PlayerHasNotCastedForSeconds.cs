using System.Collections;
using Tutorial;
using UnityEngine;
namespace Quests {
    public class PlayerHasNotCastedForSeconds : QuestCriteria {
        private readonly float _neededSeconds;
        private Coroutine _timerCoroutine;
        
        public PlayerHasNotCastedForSeconds(float neededSeconds) {
            _neededSeconds = neededSeconds;
        }
        public override void Enable() {
            Messenger.AddListener<SpellData>(Signals.ON_EXECUTE_SPELL, OnSpellExecuted);
            Messenger.AddListener<SpellData>(Signals.ON_EXECUTE_AFFLICTION, OnSpellExecuted);
            Messenger.AddListener<PlayerAction>(Signals.ON_EXECUTE_PLAYER_ACTION, OnSpellExecuted);
        }
        public override void Disable() {
            Messenger.RemoveListener<SpellData>(Signals.ON_EXECUTE_SPELL, OnSpellExecuted);
            Messenger.RemoveListener<SpellData>(Signals.ON_EXECUTE_AFFLICTION, OnSpellExecuted);
            Messenger.RemoveListener<PlayerAction>(Signals.ON_EXECUTE_PLAYER_ACTION, OnSpellExecuted);
            StopTimer();
        }

        #region Listeners
        private void OnSpellExecuted(SpellData spell) {
            RestartTimer();
            SetCriteriaAsUnMet();
        }
        #endregion

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