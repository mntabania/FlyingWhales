namespace Quests {
    public class ThreatMaxedOut : QuestCriteria {
        public override void Enable() {
            Messenger.AddListener(PlayerSignals.THREAT_MAXED_OUT, OnThreatMaxedOut);
        }
        public override void Disable() {
            Messenger.RemoveListener(PlayerSignals.THREAT_MAXED_OUT, OnThreatMaxedOut);
        }
        
        private void OnThreatMaxedOut() {
            SetCriteriaAsMet();
        }
    }
}