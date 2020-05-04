using System.Collections;
using UnityEngine;
namespace Tutorial {
    public class GameHasBeenRunningForSeconds : TutorialQuestCriteria {
        private readonly float _neededSeconds;
        public GameHasBeenRunningForSeconds(float neededSeconds) {
            _neededSeconds = neededSeconds;
        }
        public override void Enable() {
            TutorialManager.Instance.StartCoroutine(WaitForSeconds());
        }
        public override void Disable() { } 
        private IEnumerator WaitForSeconds() {
            yield return new WaitForSecondsRealtime(_neededSeconds);
            SetCriteriaAsMet();
        }
    }
}