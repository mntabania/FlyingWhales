using System;
using System.Collections.Generic;
using System.Linq;
using Ruinarch;
using UnityEngine;
using UtilityScripts;

public class AffattWinConditionTracker : WinconditionTracker {

    public const int MinimumHumans = 5;
    
    private System.Action<Character, int, int> _characterEliminatedAction;
    private System.Action<Character, int, int> _characterAddedAsTargetAction;

    public interface Listener {
        void OnCharacterEliminated(Character p_character, int p_elvenCount, int p_humanCount);
        void OnCharacterAddedAsTarget(Character p_character, int p_elvenCount, int p_humanCount);
    }

    public List<Character> elvenToEliminate { get; private set; }
    public List<Character> humans { get; private set; }
    public int totalHumansToProtect { get; private set; }
    public override Type serializedData => typeof(SaveDataAffattWinConditionTracker);
    
    public override void Initialize(List<Character> p_allCharacters) {
        base.Initialize(p_allCharacters);

        elvenToEliminate = new List<Character>();
        humans = new List<Character>();
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, CheckIfCharacterIsEliminated);
        Messenger.AddListener<Character>(FactionSignals.FACTION_SET, CheckIfCharacterIsEliminated);
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_BECOME_CULTIST, CheckIfCharacterIsEliminated);
        Messenger.AddListener<Character>(WorldEventSignals.NEW_VILLAGER_ARRIVED, OnNewVillagerArrived);
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_NO_LONGER_CULTIST, OnCharacterNoLongerCultist);
        Messenger.AddListener<Character, Faction>(FactionSignals.CHARACTER_REMOVED_FROM_FACTION, OnCharacterRemovedFromFaction);
        Messenger.AddListener<Character, Faction>(FactionSignals.CHARACTER_ADDED_TO_FACTION, OnCharacterAddedToFaction);

        List<Character> villagers = GetAllCharactersToBeEliminated(p_allCharacters);
        elvenToEliminate.Clear();
        for (int i = 0; i < villagers.Count; i++) {
            AddVillagerToEliminate(villagers[i]);
        }
    }

    #region Loading
    public override void LoadReferences(SaveDataWinConditionTracker data) {
        base.LoadReferences(data);
        SaveDataAffattWinConditionTracker saveDataAffattWinConditionTracker = data as SaveDataAffattWinConditionTracker;
        elvenToEliminate = SaveUtilities.ConvertIDListToCharacters(saveDataAffattWinConditionTracker.elvensToEliminate);
        humans = SaveUtilities.ConvertIDListToCharacters(saveDataAffattWinConditionTracker.humans);
        totalHumansToProtect = humans.Count;
    }
    #endregion

    #region List Maintenance
    private void EliminateVillager(Character p_character) {
        if (elvenToEliminate.Remove(p_character)) {
            RemoveCharacterFromTrackList(p_character);
            _characterEliminatedAction?.Invoke(p_character, elvenToEliminate.Count, humans.Count);
        }
        if (humans.Remove(p_character)) {
            RemoveCharacterFromTrackList(p_character);
            _characterEliminatedAction?.Invoke(p_character, elvenToEliminate.Count, humans.Count);
            CheckLoseCondition();
        }
    }
    private void AddVillagerToEliminate(Character p_character) {
        if (p_character.faction != null && !elvenToEliminate.Contains(p_character) && !p_character.isDead && p_character.faction.factionType.type == FACTION_TYPE.Elven_Kingdom) {
            elvenToEliminate.Add(p_character);
            AddCharacterToTrackList(p_character);
            _characterAddedAsTargetAction?.Invoke(p_character, elvenToEliminate.Count, humans.Count);
        } else if (p_character.faction != null && !humans.Contains(p_character) && p_character.faction.factionType.type == FACTION_TYPE.Human_Empire) {
            humans.Add(p_character);
            totalHumansToProtect = humans.Count;
        }
    }
    #endregion

    private void OnCharacterRemovedFromFaction(Character p_character, Faction p_faction) {
        if (p_faction.factionType.type == FACTION_TYPE.Elven_Kingdom) {
            if (elvenToEliminate.Contains(p_character)) {
                elvenToEliminate.Remove(p_character);
                RemoveCharacterFromTrackList(p_character);
                _characterEliminatedAction?.Invoke(p_character, elvenToEliminate.Count, humans.Count);
            }
        } else if(p_faction.factionType.type == FACTION_TYPE.Human_Empire){
            if (humans.Contains(p_character)) {
                humans.Remove(p_character);
                RemoveCharacterFromTrackList(p_character);
                _characterEliminatedAction?.Invoke(p_character, elvenToEliminate.Count, humans.Count);
            }
        }
        CheckLoseCondition();
    }
    private void CheckLoseCondition() {
        if (humans.Count < MinimumHumans) {
            PlayerUI.Instance.LoseGameOver("Humans out numbered. You failed");
        }
    }
    private void OnCharacterAddedToFaction(Character p_character, Faction p_faction) {
        if (p_faction.factionType.type == FACTION_TYPE.Elven_Kingdom) {
            if (!elvenToEliminate.Contains(p_character)) {
                elvenToEliminate.Add(p_character);
                AddCharacterToTrackList(p_character);
                _characterAddedAsTargetAction?.Invoke(p_character, elvenToEliminate.Count, humans.Count);
            }
        } else if (p_faction.factionType.type == FACTION_TYPE.Human_Empire) {
            if (!humans.Contains(p_character)) {
                humans.Add(p_character);
                AddCharacterToTrackList(p_character);
                _characterAddedAsTargetAction?.Invoke(p_character, elvenToEliminate.Count, humans.Count);
            }
        }
    }
    private void CheckIfCharacterIsEliminated(Character p_character) {
        if (ShouldConsiderCharacterAsEliminated(p_character)) {
            EliminateVillager(p_character);
            RemoveCharacterFromTrackList(p_character);
        }
    }
    private void OnNewVillagerArrived(Character newVillager) {
        AddVillagerToEliminate(newVillager);
    }
    private void OnCharacterNoLongerCultist(Character p_character) {
        AddVillagerToEliminate(p_character);
    }
    public Faction GetMainElvenFaction() {
        for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++) {
            Faction faction = FactionManager.Instance.allFactions[i];
            if (faction.factionType.type == FACTION_TYPE.Elven_Kingdom) {
                return faction;
            }
        }
        throw new Exception("No elven faction for Affatt Map!");
    }
    public Faction GetMainHumanFaction() {
        for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++) {
            Faction faction = FactionManager.Instance.allFactions[i];
            if (faction.factionType.type == FACTION_TYPE.Human_Empire) {
                return faction;
            }
        }
        throw new Exception("No elven faction for Affatt Map!");
    }
    
    public void Subscribe(Listener p_listener) {
        _characterEliminatedAction += p_listener.OnCharacterEliminated;
        _characterAddedAsTargetAction += p_listener.OnCharacterAddedAsTarget;
    }
    public void Unsubscribe(Listener p_listener) {
        _characterEliminatedAction -= p_listener.OnCharacterEliminated;
        _characterAddedAsTargetAction -= p_listener.OnCharacterAddedAsTarget;
    }
}

public class SaveDataAffattWinConditionTracker : SaveDataWinConditionTracker {
    public List<string> elvensToEliminate;
    public List<string> humans;
    public override void Save(WinconditionTracker data) {
        base.Save(data);
        AffattWinConditionTracker affattWinConditionTracker = data as AffattWinConditionTracker;
        elvensToEliminate = SaveUtilities.ConvertSavableListToIDs(affattWinConditionTracker.elvenToEliminate);
        humans = SaveUtilities.ConvertSavableListToIDs(affattWinConditionTracker.humans);
    }
}