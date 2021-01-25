using System;
using System.Collections.Generic;
using Traits;

public class PittoWinConditionTracker : WinconditionTracker {

    private System.Action<Faction> _onfactionCreated;
    private System.Action<Character> _CharacterChangeTrait;

    public List<Character> cultists = new List<Character>();
    public override Type serializedData => typeof(SaveDataPittoWinConditionTracker);

    #region IListener
    public interface IListenerFactionEvents {
        void OnFactionCreated(Faction p_createdFaction);
    }

    public interface IListenerChangeTraits {
        void OnCharacterChangeTrait(Character p_character);
    }
    #endregion

    public override void Initialize(List<Character> p_allCharacters) {
        base.Initialize(p_allCharacters);
                
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, CheckIfCharacterIsEliminated);
        Messenger.AddListener<Character>(FactionSignals.FACTION_SET, CheckIfCharacterIsEliminated);
        Messenger.AddListener<Character, Trait>(CharacterSignals.CHARACTER_TRAIT_ADDED, OnCharactertraitGain);
        Messenger.AddListener<Character>(WorldEventSignals.NEW_VILLAGER_ARRIVED, OnNewVillagerArrived);
        Messenger.AddListener<Faction>(FactionSignals.FACTION_CREATED, OnFactionCreated);

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
    }
    #endregion

    private void OnFactionCreated(Faction p_createdFaction) {
        if (p_createdFaction.factionType.type == FACTION_TYPE.Demon_Cult) {
            _onfactionCreated?.Invoke(p_createdFaction);
        }
    }

    #region List Maintenance
    private void EliminateVillager(Character p_character) {
        
    }
    private void AddVillagerToEliminate(Character p_character) {
        AddCharacterToTrackList(p_character);
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

    private void OnCharactertraitGain(Character p_character, Trait p_trait) {
        if (p_character.traitContainer.HasTrait("Cultist")) {
            if (!cultists.Contains(p_character)) {
                cultists.Add(p_character);
                _CharacterChangeTrait?.Invoke(p_character);
            }
        }
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
        AddVillagerToEliminate(newVillager);
    }

    public void SubscribeToFactionEvents(PittoWinConditionTracker.IListenerFactionEvents p_listener) {
        _onfactionCreated += p_listener.OnFactionCreated;
    }
    public void UnsubscribeToFactionEvents(PittoWinConditionTracker.IListenerFactionEvents p_listener) {
        _onfactionCreated -= p_listener.OnFactionCreated;
    }

    public void SubscribeToChangeTraitEvents(PittoWinConditionTracker.IListenerChangeTraits p_listener) {
        _CharacterChangeTrait += p_listener.OnCharacterChangeTrait;
    }
    public void UnsubscribeToChangeTraitEvents(PittoWinConditionTracker.IListenerChangeTraits p_listener) {
        _CharacterChangeTrait -= p_listener.OnCharacterChangeTrait;
    }
}

public class SaveDataPittoWinConditionTracker : SaveDataWinConditionTracker {
    public List<string> cultists;
    public override void Save(WinconditionTracker data) {
        base.Save(data);
        PittoWinConditionTracker tracker = data as PittoWinConditionTracker;
        cultists = SaveUtilities.ConvertSavableListToIDs(tracker.cultists);
    }
}