namespace Locations.Settlements.Settlement_Events {
    public class WerewolfHunt : SettlementEvent {
        public override SETTLEMENT_EVENT eventType => SETTLEMENT_EVENT.Werewolf_Hunt;
        
        private GameDate _endDate;
        private string _currentScheduleKey;

        #region getters
        public GameDate endDate => _endDate;
        #endregion
        
        public WerewolfHunt(NPCSettlement location) : base(location) { }
        public WerewolfHunt(SaveDataWerewolfHunt data) : base(data) {
            LoadEnd(data.endDate);
            SubscribeListeners();
        }
        
        private void SubscribeListeners() {
            Messenger.AddListener<Character, CRIME_TYPE, Character>(CharacterSignals.CHARACTER_ACCUSED_OF_CRIME, OnCharacterAccusedOfCrime);
            Messenger.AddListener<Faction>(FactionSignals.FACTION_CRIMES_CHANGED, OnFactionCrimesChanged);
        }
        private void UnsubscribeListeners() {
            Messenger.RemoveListener<Character, CRIME_TYPE, Character>(CharacterSignals.CHARACTER_ACCUSED_OF_CRIME, OnCharacterAccusedOfCrime);
            Messenger.RemoveListener<Faction>(FactionSignals.FACTION_CRIMES_CHANGED, OnFactionCrimesChanged);
        }
        
        #region Overrides
        public override void ActivateEvent(NPCSettlement p_settlement) {
            for (int i = 0; i < p_settlement.residents.Count; i++) {
                Character resident = p_settlement.residents[i];
                AddInterestingItems(resident);
            }
            p_settlement.AddNeededItems(TILE_OBJECT_TYPE.PHYLACTERY);
            ScheduleEnd();
            SubscribeListeners();
            //p_settlement.settlementClassTracker.AddNeededClass("Shaman");
            
            Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Settlement Event", "Werewolf Hunt", "started", null, LOG_TAG.Major);
            log.AddToFillers(p_settlement, p_settlement.name, LOG_IDENTIFIER.LANDMARK_1);
            if (p_settlement.owner != null) {
                log.AddInvolvedObjectManual(p_settlement.owner.persistentID);    
            }
            log.AddLogToDatabase();
            PlayerManager.Instance.player.ShowNotificationFromPlayer(log, true);
        }
        public override void DeactivateEvent(NPCSettlement p_settlement) {
            for (int i = 0; i < p_settlement.residents.Count; i++) {
                Character resident = p_settlement.residents[i];
                RemoveInterestingItems(resident);
            }
            p_settlement.RemoveNeededItems(TILE_OBJECT_TYPE.PHYLACTERY);
            UnsubscribeListeners();
            //p_settlement.settlementClassTracker.RemoveNeededClass("Shaman");

            Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Settlement Event", "Werewolf Hunt", "ended", null, LOG_TAG.Major);
            log.AddToFillers(p_settlement, p_settlement.name, LOG_IDENTIFIER.LANDMARK_1);
            if (p_settlement.owner != null) {
                //in case when this event ends and a faction no longer owns the settlement that has this event
                log.AddInvolvedObjectManual(p_settlement.owner.persistentID);    
            }
            log.AddLogToDatabase();
            PlayerManager.Instance.player.ShowNotificationFromPlayer(log, true);
            
            if (!string.IsNullOrEmpty(_currentScheduleKey)) {
                SchedulingManager.Instance.RemoveSpecificEntry(_currentScheduleKey);
                _currentScheduleKey = string.Empty;
            }
        }
        public override void ProcessNewVillager(Character newVillager) {
            base.ProcessNewVillager(newVillager);
            AddInterestingItems(newVillager);
        }
        public override void ProcessRemovedVillager(Character removedVillager) {
            base.ProcessRemovedVillager(removedVillager);
            RemoveInterestingItems(removedVillager);
        }
        #endregion

        #region Interesting Items
        private void AddInterestingItems(Character character) {
            if (!character.traitContainer.HasTrait("Lycanphiliac", "Lycanthrope")) {
                character.AddItemAsInteresting("Phylactery");    
            }
        }
        private void RemoveInterestingItems(Character character) {
            if (!character.traitContainer.HasTrait("Lycanphiliac", "Lycanthrope")) {
                character.RemoveItemAsInteresting("Phylactery");    
            }
        }
        #endregion

        #region Timer
        private void ScheduleEnd() {
            _endDate = GameManager.Instance.Today();
            _endDate.AddDays(3);
            // _endDate.AddTicks(5);
            _currentScheduleKey = SchedulingManager.Instance.AddEntry(_endDate, () => location.eventManager.DeactivateEvent(this), location);
        }
        private void RescheduleEnd() {
            SchedulingManager.Instance.RemoveSpecificEntry(_currentScheduleKey);
            ScheduleEnd();
        }
        private void LoadEnd(GameDate date) {
            _endDate = date;
            _currentScheduleKey = SchedulingManager.Instance.AddEntry(date, () => location.eventManager.DeactivateEvent(this), location);
        }
        #endregion

        #region Listeners
        private void OnCharacterAccusedOfCrime(Character criminal, CRIME_TYPE crimeType, Character accuser) {
            if ((criminal.currentSettlement == location || criminal.homeSettlement == location) && crimeType == CRIME_TYPE.Werewolf) {
                RescheduleEnd(); //reset timer anytime a resident or a character inside the settlement is accused of being a werewolf
            }
        }
        private void OnFactionCrimesChanged(Faction faction) {
            if (location.owner == faction) {
                CRIME_SEVERITY severity = faction.factionType.GetCrimeSeverity(CRIME_TYPE.Werewolf);
                if (severity == CRIME_SEVERITY.None || severity == CRIME_SEVERITY.Unapplicable) {
                    //Werewolves became legal. end this event.
                    location.eventManager.DeactivateEvent(this);
                }
            }
        }
        #endregion

        #region Saving
        public override SaveDataSettlementEvent Save() {
            SaveDataWerewolfHunt saveData = new SaveDataWerewolfHunt();
            saveData.Save(this);
            return saveData;
        }
        #endregion
    }
    
    public class SaveDataWerewolfHunt : SaveDataSettlementEvent {
        public GameDate endDate;
        public override void Save(SettlementEvent data) {
            base.Save(data);
            WerewolfHunt hunt = data as WerewolfHunt;
            endDate = hunt.endDate;
        }
        public override SettlementEvent Load() {
            return new WerewolfHunt(this);
        }
    }
}