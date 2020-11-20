namespace Factions.Faction_Components {
    public class FactionEventDispatcher {

        #region IListener
        public interface IListener {
            void OnFactionLeaderChanged(ILeader p_newLeader);
        }
        #endregion
        
        private System.Action<ILeader> _factionLeaderChanged;

        public void SubscribeToFactionLeaderChangedEvent(IListener p_listener) {
            _factionLeaderChanged += p_listener.OnFactionLeaderChanged;
        }
        public void UnsubscribeToFactionLeaderChangedEvent(IListener p_listener) {
            _factionLeaderChanged -= p_listener.OnFactionLeaderChanged;
        }
        public void ExecuteFactionLeaderChangedEvent(ILeader p_newLeader) {
            _factionLeaderChanged?.Invoke(p_newLeader);
        }
    }
}