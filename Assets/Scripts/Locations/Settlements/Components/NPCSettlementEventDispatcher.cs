namespace Locations.Settlements.Components {
    public class NPCSettlementEventDispatcher {
        
        #region IListener
        public interface IListener {
            void OnSettlementRulerChanged(Character p_newLeader, NPCSettlement p_settlement);
            void OnFactionOwnerChanged(Faction p_previousOwner, Faction p_newOwner, NPCSettlement p_settlement);
        }
        public interface ITimeListener {
            void OnHourStarted(NPCSettlement p_settlement);
        }
        public interface ITileListener {
            void OnSettlementAreaRemoved(Area p_area, NPCSettlement p_settlement);
        }
        #endregion

        private System.Action<Character, NPCSettlement> _settlementRulerChanged;
        private System.Action<Faction, Faction, NPCSettlement> _factionOwnerChanged;
        private System.Action<NPCSettlement> _hourStarted;
        private System.Action<Area, NPCSettlement> _settlementAreaRemoved;

        #region Settlement Ruler
        public void SubscribeToSettlementRulerChangedEvent(IListener p_listener) {
            _settlementRulerChanged += p_listener.OnSettlementRulerChanged;
        }
        public void UnsubscribeToSettlementRulerChangedEvent(IListener p_listener) {
            _settlementRulerChanged -= p_listener.OnSettlementRulerChanged;
        }
        public void ExecuteSettlementRulerChangedEvent(Character p_newRuler, NPCSettlement p_settlement) {
            _settlementRulerChanged?.Invoke(p_newRuler, p_settlement);
        }
        #endregion

        #region Faction Owner
        public void SubscribeToFactionOwnerChangedEvent(IListener p_listener) {
            _factionOwnerChanged += p_listener.OnFactionOwnerChanged;
        }
        public void UnsubscribeToFactionOwnerChangedEvent(IListener p_listener) {
            _factionOwnerChanged -= p_listener.OnFactionOwnerChanged;
        }
        public void ExecuteFactionOwnerChangedEvent(Faction p_previousOwner, Faction p_newOwner, NPCSettlement p_settlement) {
            _factionOwnerChanged?.Invoke(p_previousOwner, p_newOwner, p_settlement);
        }
        #endregion

        #region Hour Started
        public void SubscribeToHourStartedEvent(ITimeListener p_listener) {
            _hourStarted += p_listener.OnHourStarted;
        }
        public void UnsubscribeToHourStartedEvent(ITimeListener p_listener) {
            _hourStarted -= p_listener.OnHourStarted;
        }
        public void ExecuteHourStartedEvent(NPCSettlement p_settlement) {
            _hourStarted?.Invoke(p_settlement);
        }
        #endregion

        #region Settlement Tiles
        public void SubscribeToTileRemovedEvent(ITileListener p_listener) {
            _settlementAreaRemoved += p_listener.OnSettlementAreaRemoved;
        }
        public void UnsubscribeToTileRemovedEvent(ITileListener p_listener) {
            _settlementAreaRemoved -= p_listener.OnSettlementAreaRemoved;
        }
        public void ExecuteTileRemovedEvent(Area p_area, NPCSettlement p_settlement) {
            _settlementAreaRemoved?.Invoke(p_area, p_settlement);
        }
        #endregion
    }
}