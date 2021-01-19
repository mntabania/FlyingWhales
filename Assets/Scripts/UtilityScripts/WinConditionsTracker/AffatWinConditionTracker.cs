using System;
using System.Collections.Generic;
using System.Linq;
using Ruinarch;
using UnityEngine;
using UtilityScripts;

public class AffatWinConditionTracker : WinconditionTracker {

    private System.Action<Character, int, int> _characterEliminatedAction;
    private System.Action<Character, int, int> _characterAddedAsTargetAction;

    public interface Listener {
        void OnCharacterEliminated(Character p_character, int p_elvenCount, int p_humanCount);
        void OnCharacterAddedAsTarget(Character p_character, int p_elvenCount, int p_humanCount);
    }

    public List<Character> elvensToEliminate { get; private set; }
    public List<Character> humans { get; private set; }
    public override Type serializedData => typeof(SaveDataAffattWinConditionTracker);
    
    public override void Initialize(List<Character> p_allCharacters) {
        base.Initialize(p_allCharacters);

        elvensToEliminate = new List<Character>();
        humans = new List<Character>();
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, CheckIfCharacterIsEliminated);
        Messenger.AddListener<Character>(FactionSignals.FACTION_SET, CheckIfCharacterIsEliminated);
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_BECOME_CULTIST, CheckIfCharacterIsEliminated);
        Messenger.AddListener<Character>(WorldEventSignals.NEW_VILLAGER_ARRIVED, OnNewVillagerArrived);
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_NO_LONGER_CULTIST, OnCharacterNoLongerCultist);
        Messenger.AddListener<Character, Faction>(FactionSignals.CHARACTER_REMOVED_FROM_FACTION, OnCharacterRemovedFromFaction);
        Messenger.AddListener<Character, Faction>(FactionSignals.CHARACTER_ADDED_TO_FACTION, OnCharacterAddedToFaction);

        List<Character> villagers = GetAllCharactersToBeEliminated(p_allCharacters);
        elvensToEliminate.Clear();
        for (int i = 0; i < villagers.Count; i++) {
            AddVillagerToEliminate(villagers[i]);
        }
    }

    #region Loading
    public override void LoadReferences(SaveDataWinConditionTracker data) {
        base.LoadReferences(data);
        SaveDataAffattWinConditionTracker saveDataAffattWinConditionTracker = data as SaveDataAffattWinConditionTracker;
        elvensToEliminate = SaveUtilities.ConvertIDListToCharacters(saveDataAffattWinConditionTracker.elvensToEliminate);
        humans = SaveUtilities.ConvertIDListToCharacters(saveDataAffattWinConditionTracker.humans);
    }
    #endregion

    #region List Maintenance
    private void EliminateVillager(Character p_character) {
        if (elvensToEliminate.Remove(p_character)) {
            RemoveCharacterFromTrackList(p_character);
            _characterEliminatedAction?.Invoke(p_character, elvensToEliminate.Count, humans.Count);
        }
    }
    private void AddVillagerToEliminate(Character p_character) {
        if (p_character.faction != null && !elvensToEliminate.Contains(p_character) && !p_character.isDead && p_character.faction.factionType.type == FACTION_TYPE.Elven_Kingdom) {
            elvensToEliminate.Add(p_character);
            AddCharacterToTrackList(p_character);
            _characterAddedAsTargetAction?.Invoke(p_character, elvensToEliminate.Count, humans.Count);
        } else if (p_character.faction != null && !humans.Contains(p_character) && p_character.faction.factionType.type == FACTION_TYPE.Human_Empire) {
            humans.Add(p_character);
        }
    }
    #endregion

    public void OnCharacterRemovedFromFaction(Character p_character, Faction p_faction) {
        if (p_faction.factionType.type == FACTION_TYPE.Elven_Kingdom) {
            if (elvensToEliminate.Contains(p_character)) {
                elvensToEliminate.Remove(p_character);
                RemoveCharacterFromTrackList(p_character);
                _characterEliminatedAction?.Invoke(p_character, elvensToEliminate.Count, humans.Count);
            }
        } else if(p_faction.factionType.type == FACTION_TYPE.Human_Empire){
            if (humans.Contains(p_character)) {
                humans.Remove(p_character);
                RemoveCharacterFromTrackList(p_character);
                _characterEliminatedAction?.Invoke(p_character, elvensToEliminate.Count, humans.Count);
            }
        }
        if (humans.Count <= 0) {
            PlayerUI.Instance.LoseGameOver("Humans out numbered. You failed");
        }
    }

    public void OnCharacterAddedToFaction(Character p_character, Faction p_faction) {
        if (p_faction.factionType.type == FACTION_TYPE.Elven_Kingdom) {
            if (!elvensToEliminate.Contains(p_character)) {
                elvensToEliminate.Add(p_character);
                AddCharacterToTrackList(p_character);
                _characterAddedAsTargetAction?.Invoke(p_character, elvensToEliminate.Count, humans.Count);
            }
        } else if (p_faction.factionType.type == FACTION_TYPE.Human_Empire) {
            if (!humans.Contains(p_character)) {
                humans.Add(p_character);
                AddCharacterToTrackList(p_character);
                _characterAddedAsTargetAction?.Invoke(p_character, elvensToEliminate.Count, humans.Count);
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
        AffatWinConditionTracker affatWinConditionTracker = data as AffatWinConditionTracker;
        elvensToEliminate = SaveUtilities.ConvertSavableListToIDs(affatWinConditionTracker.elvensToEliminate);
        humans = SaveUtilities.ConvertSavableListToIDs(affatWinConditionTracker.humans);
    }
}