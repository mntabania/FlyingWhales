using System.Collections.Generic;
namespace Tutorial {
    public class ThreatIncreased : TutorialQuestCriteria {
        public override void Enable() {
            Messenger.AddListener(Signals.THREAT_INCREASED, OnThreatIncreased);
        }
        public override void Disable() {
            Messenger.RemoveListener(Signals.THREAT_INCREASED, OnThreatIncreased);
        }
        
        private void OnThreatIncreased() {
            SetCriteriaAsMet();
        }
    }
}