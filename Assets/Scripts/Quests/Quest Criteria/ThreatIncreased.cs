namespace Quests {
    public class ThreatIncreased : QuestCriteria {
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