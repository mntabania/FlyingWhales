namespace Locations.Settlements.Components {
    public class NPCSettlementEventDispatcher {
        
        #region IListener
        public interface IListener {
            void OnSettlementRulerChanged(Character p_newLeader, NPCSettlement p_settlement);
            void OnFactionOwnerChanged(Faction p_previousOwner, Faction p_newOwner, NPCSettlement p_settlement);
        }
        #endregion

        private System.Action<Character, NPCSettlement> _settlementRulerChanged;
        private System.Action<Faction, Faction, NPCSettlement> _factionOwnerChanged;
        
        public void SubscribeToSettlementRulerChangedEvent(IListener p_listener) {
            _settlementRulerChanged += p_listener.OnSettlementRulerChanged;
        }
        public void UnsubscribeToSettlementRulerChangedEvent(IListener p_listener) {
            _settlementRulerChanged -= p_listener.OnSettlementRulerChanged;
        }
        public void ExecuteSettlementRulerChangedEvent(Character p_newRuler, NPCSettlement p_settlement) {
            _settlementRulerChanged?.Invoke(p_newRuler, p_settlement);
        }
        
        public void SubscribeToFactionOwnerChangedEvent(IListener p_listener) {
            _factionOwnerChanged += p_listener.OnFactionOwnerChanged;
        }
        public void UnsubscribeToFactionOwnerChangedEvent(IListener p_listener) {
            _factionOwnerChanged -= p_listener.OnFactionOwnerChanged;
        }
        public void ExecuteFactionOwnerChangedEvent(Faction p_previousOwner, Faction p_newOwner, NPCSettlement p_settlement) {
            _factionOwnerChanged?.Invoke(p_previousOwner, p_newOwner, p_settlement);
        }
    }
}