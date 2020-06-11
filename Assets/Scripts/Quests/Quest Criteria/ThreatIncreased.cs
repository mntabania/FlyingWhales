namespace Quests {
    public class ThreatIncreased : QuestCriteria {
        public override void Enable() {
            Messenger.AddListener<int>(Signals.THREAT_INCREASED, OnThreatIncreased);
        }
        public override void Disable() {
            Messenger.RemoveListener<int>(Signals.THREAT_INCREASED, OnThreatIncreased);
        }
        
        private void OnThreatIncreased(int amount) {
            SetCriteriaAsMet();
        }
    }
}