namespace Locations.Settlements.Settlement_Events {
    public abstract class SettlementEvent {

        public abstract SETTLEMENT_EVENT eventType { get; }
        
        protected NPCSettlement _location;

        #region getters
        public NPCSettlement location => _location;
        #endregion
        
        public SettlementEvent(NPCSettlement location) {
            _location = location;
        }
        public SettlementEvent(SaveDataSettlementEvent data) {
            _location = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentID(data.settlementID) as NPCSettlement;
        }

        public abstract void ActivateEvent(NPCSettlement settlement);
        public abstract void DeactivateEvent(NPCSettlement settlement);
        public virtual void ProcessNewVillager(Character newVillager) { }
        public virtual void ProcessRemovedVillager(Character removedVillager) { }

        #region Saving
        public abstract SaveDataSettlementEvent Save();
        #endregion
    }

    public abstract class SaveDataSettlementEvent : SaveData<SettlementEvent> {
        public string settlementID;
        public override void Save(SettlementEvent data) {
            base.Save(data);
            settlementID = data.location.persistentID;
        }
    }
}