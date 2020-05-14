namespace Quests {
    public class ThreatMaxedOut : QuestCriteria {
        public override void Enable() {
            Messenger.AddListener(Signals.THREAT_MAXED_OUT, OnThreatMaxedOut);
        }
        public override void Disable() {
            Messenger.RemoveListener(Signals.THREAT_MAXED_OUT, OnThreatMaxedOut);
        }
        
        private void OnThreatMaxedOut() {
            SetCriteriaAsMet();
        }
    }
}