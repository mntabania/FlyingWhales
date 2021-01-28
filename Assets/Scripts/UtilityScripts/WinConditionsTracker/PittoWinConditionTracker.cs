using System;
using System.Collections.Generic;
using Traits;

public class PittoWinConditionTracker : WinconditionTracker {

    private System.Action<Faction> _onfactionCreated;
    private System.Action<Character> _CharacterChangeTrait;
    private System.Action<Character> _CharacterDied;

    public List<Character> cultists = new List<Character>();

    public Faction createdFaction;
    public override Type serializedData => typeof(SaveDataPittoWinConditionTracker);

    #region IListener
    public interface IListenerFactionEvents {
        void OnFactionCreated(Faction p_createdFaction);
    }

    public interface IListenerChangeTraits {
        void OnCharacterChangeTrait(Character p_character);
        void OnCharacterDied(Character p_character);
    }
    #endregion

    public override void Initialize(List<Character> p_allCharacters) {
        base.Initialize(p_allCharacters);
                
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, CheckIfCharacterIsEliminated);
        Messenger.AddListener<Character>(FactionSignals.FACTION_SET, CheckIfCharacterIsEliminated);
        Messenger.AddListener<Character, Faction>(FactionSignals.CHARACTER_ADDED_TO_FACTION, CharacterJoinFaction);
        Messenger.AddListener<Character, Faction>(FactionSignals.CHARACTER_REMOVED_FROM_FACTION, CharacterRemoveFaction);
        Messenger.AddListener<Character>(WorldEventSignals.NEW_VILLAGER_ARRIVED, OnNewVillagerArrived);
        Messenger.AddListener<Faction>(FactionSignals.FACTION_CREATED, OnFactionCreated);
        Messenger.AddListener<Faction>(FactionSignals.FACTION_DISBANDED, OnFactionDisbanded);

        List<Character> charactersToTrack = GetQuestCharacters(p_allCharacters);
        
        for (int i = 0; i < charactersToTrack.Count; i++) {
            AddVillagerToEliminate(charactersToTrack[i]);
        }
    }
    
    #region Loading
    public override void LoadReferences(SaveDataWinConditionTracker data) {
        base.LoadReferences(data);
        SaveDataPittoWinConditionTracker tracker = data as SaveDataPittoWinConditionTracker;
        cultists = SaveUtilities.ConvertIDListToCharacters(tracker.cultists);
        createdFaction = DatabaseManager.Instance.factionDatabase.GetFactionBasedOnPersistentID(tracker.createdFaction);
    }
    #endregion

    private void OnFactionCreated(Faction p_createdFaction) {
        if (p_createdFaction.factionType.type == FACTION_TYPE.Demon_Cult && createdFaction == null) {
            createdFaction = p_createdFaction;
            _onfactionCreated?.Invoke(p_createdFaction);
        }
    }

    private void OnFactionDisbanded(Faction p_disbandedFaction) {
        if (createdFaction != null && createdFaction == p_disbandedFaction) {
            PlayerUI.Instance.LoseGameOver("Your demon cult has been wiped out. Mission Failed");
        }
    }

    #region List Maintenance
    private void EliminateVillager(Character p_character) {
        if (cultists.Contains(p_character)) {
            cultists.Remove(p_character);
        }
        _CharacterDied?.Invoke(p_character);
    }
    private void AddVillagerToEliminate(Character p_character) {
       
    }
    #endregion

    public List<Character> GetQuestCharacters(List<Character> p_allCharacters) {
        List<Character> characters = new List<Character>();
        for (int i = 0; i < p_allCharacters.Count; i++) {
            Character character = p_allCharacters[i];
            if (character.isNormalCharacter && character.race.IsSapient()) {
                characters.Add(character);
            }
        }
        return characters;
    }

    private int GetFactionCount() {
        int count = 0;
        FactionManager.Instance.allFactions.ForEach((eachFaction) => {
            if (eachFaction.isMajorNonPlayer && eachFaction.characters.Count > 0) {
                count++;
            }
        });
        return count;
    }

    private void CharacterRemoveFaction(Character p_newMember, Faction p_faction) {
        if (createdFaction != null) {
            if (createdFaction == p_faction && !cultists.Contains(p_newMember)) {
                cultists.Remove(p_newMember);
                _CharacterChangeTrait?.Invoke(p_newMember);
            }
        }
    }

    private void CharacterJoinFaction(Character p_newMember, Faction p_faction) {
        if (createdFaction != null) {
            if (createdFaction == p_faction && !cultists.Contains(p_newMember)) {
                cultists.Add(p_newMember);
                _CharacterChangeTrait?.Invoke(p_newMember);
            }
        }
    }

    private void CheckIfCharacterIsEliminated(Character p_character) {
        if (ShouldConsiderCharacterAsEliminated(p_character)) {
            EliminateVillager(p_character);
            RemoveCharacterFromTrackList(p_character);
        }
        if (GetFactionCount() <= 0) {
            PlayerUI.Instance.LoseGameOver("You fail to recruit 15 cultists. Mission Failed");
        }
    }
    private void OnNewVillagerArrived(Character newVillager) {
        
    }

    public void SubscribeToFactionEvents(PittoWinConditionTracker.IListenerFactionEvents p_listener) {
        _onfactionCreated += p_listener.OnFactionCreated;
    }
    public void UnsubscribeToFactionEvents(PittoWinConditionTracker.IListenerFactionEvents p_listener) {
        _onfactionCreated -= p_listener.OnFactionCreated;
    }

    public void SubscribeToChangeTraitEvents(PittoWinConditionTracker.IListenerChangeTraits p_listener) {
        _CharacterChangeTrait += p_listener.OnCharacterChangeTrait;
        _CharacterDied += p_listener.OnCharacterDied;
    }
    public void UnsubscribeToChangeTraitEvents(PittoWinConditionTracker.IListenerChangeTraits p_listener) {
        _CharacterChangeTrait -= p_listener.OnCharacterChangeTrait;
        _CharacterDied -= p_listener.OnCharacterDied;
    }
}

public class SaveDataPittoWinConditionTracker : SaveDataWinConditionTracker {
    public List<string> cultists;
    public string createdFaction;
    public override void Save(WinconditionTracker data) {
        base.Save(data);
        PittoWinConditionTracker tracker = data as PittoWinConditionTracker;
        cultists = SaveUtilities.ConvertSavableListToIDs(tracker.cultists);
        createdFaction = tracker.createdFaction.persistentID;
    }
}