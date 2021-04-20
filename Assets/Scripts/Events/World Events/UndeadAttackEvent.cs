using Object_Pools;
namespace Events.World_Events {
    public class UndeadAttackEvent : WorldEvent {

        public UndeadAttackEvent() { }
        public UndeadAttackEvent(SaveUndeadAttackEvent data) { }

        public override void InitializeEvent() {
            Messenger.AddListener(Signals.DAY_STARTED, OnDayStarted);
            Messenger.AddListener<Character, Faction>(FactionSignals.CHARACTER_ADDED_TO_FACTION, OnCharacterAddedFaction);
        }
        private void StartUndeadAttack() {
            for (int i = 0; i < FactionManager.Instance.undeadFaction.characters.Count; i++) {
                Character character = FactionManager.Instance.undeadFaction.characters[i];
                if (!character.isDead) {
                    AddInvaderBehaviourToCharacter(character);
                }
            }
            for (int i = 0; i < PlayerManager.Instance.player.playerFaction.characters.Count; i++) {
                Character character = PlayerManager.Instance.player.playerFaction.characters[i];
                if (!character.isDead && IsUndead(character)) {
                    AddInvaderBehaviourToCharacter(character);
                }
            }
            Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Tile Object", "DesertRose", "activated_village", providedTags: LOG_TAG.Player);
            log.AddLogToDatabase();
            PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
            LogPool.Release(log);
        }

        #region Listeners
        private void OnDayStarted() {
            int p_currentDay = GameManager.Instance.continuousDays;
            if (p_currentDay == WipeOutAllUntilDayWinConditionTracker.DueDay) {
                StartUndeadAttack();
            }
        }
        private void OnCharacterAddedFaction(Character p_character, Faction p_faction) {
            int p_currentDay = GameManager.Instance.continuousDays;
            if (p_currentDay >= WipeOutAllUntilDayWinConditionTracker.DueDay) {
                bool shouldBeInvader = false;
                if (p_faction.factionType.type == FACTION_TYPE.Undead) {
                    shouldBeInvader = true;
                } else if (p_faction.isPlayerFaction && IsUndead(p_character)) {
                    shouldBeInvader = true;
                }
                if (shouldBeInvader) {
                    AddInvaderBehaviourToCharacter(p_character);
                }
            }
        }
        private bool IsUndead(Character p_character) {
            if (p_character.characterClass.IsZombie() || (p_character is Summon summon && 
                (summon.summonType == SUMMON_TYPE.Ghost || summon.summonType == SUMMON_TYPE.Skeleton || summon.summonType == SUMMON_TYPE.Vengeful_Ghost || summon.summonType == SUMMON_TYPE.Revenant))) {
                return true;
            }
            return false;
        }
        #endregion

        #region Undead Attack
        private void AddInvaderBehaviourToCharacter(Character p_character) {
            p_character.behaviourComponent.AddBehaviourComponent(typeof(PangatLooVillageInvaderBehaviour));
            p_character.jobQueue.CancelAllJobs();
        }
        #endregion

        #region Saving
        public override SaveDataWorldEvent Save() {
            SaveUndeadAttackEvent leaderEvent = new SaveUndeadAttackEvent();
            leaderEvent.Save(this);
            return leaderEvent;
        }
        #endregion
    }

    public class SaveUndeadAttackEvent : SaveDataWorldEvent {
        public override void Save(WorldEvent data) {
            base.Save(data);
            UndeadAttackEvent undeadAttackEvent = data as UndeadAttackEvent;
        }
        public override WorldEvent Load() {
            return new UndeadAttackEvent(this);
        }
    }
}