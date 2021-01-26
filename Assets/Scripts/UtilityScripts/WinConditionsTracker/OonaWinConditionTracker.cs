﻿using System;
using System.Collections.Generic;
using System.Linq;
using Ruinarch;
using UnityEngine;
using UtilityScripts;

public class OonaWinConditionTracker : WinconditionTracker {

    private System.Action<Character, int> _characterEliminatedAction;
    private System.Action<Character> _characterAddedAsTargetAction;

    public interface Listener {
        void OnCharacterEliminated(Character p_character, int p_villagerCount);
        void OnCharacterAddedAsTarget(Character p_character);
    }

    public List<Character> villagersToEliminate { get; private set; }
    public int totalCharactersToEliminate { get; private set; }
    public override Type serializedData => typeof(SaveDataOonaWinConditionTracker);
    
    public override void Initialize(List<Character> p_allCharacters) {
        base.Initialize(p_allCharacters);

        villagersToEliminate = new List<Character>();
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, CheckIfCharacterIsEliminated);
        Messenger.AddListener<Character>(FactionSignals.FACTION_SET, CheckIfCharacterIsEliminated);
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_BECOME_CULTIST, CheckIfCharacterIsEliminated);
        Messenger.AddListener<Character>(WorldEventSignals.NEW_VILLAGER_ARRIVED, OnNewVillagerArrived);
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_NO_LONGER_CULTIST, OnCharacterNoLongerCultist);

        List<Character> villagers = GetAllCharactersToBeEliminated(p_allCharacters);
        villagersToEliminate.Clear();
        for (int i = 0; i < villagers.Count; i++) {
            Character villager = villagers[i];
            if (!ShouldConsiderCharacterAsEliminated(villager)) {
                AddVillagerToEliminate(villager);    
            }
        }
        totalCharactersToEliminate = villagersToEliminate.Count;
    }

    #region Loading
    public override void LoadReferences(SaveDataWinConditionTracker data) {
        base.LoadReferences(data);
        SaveDataOonaWinConditionTracker tracker = data as SaveDataOonaWinConditionTracker;
        villagersToEliminate = SaveUtilities.ConvertIDListToCharacters(tracker.villagersToEliminate);
        totalCharactersToEliminate = tracker.totalCharactersToEliminate;
    }
    #endregion
    
    #region List Maintenance
    private void EliminateVillager(Character p_character) {
        if (villagersToEliminate.Remove(p_character)) {
            totalCharactersToEliminate--;
            RemoveCharacterFromTrackList(p_character);
            _characterEliminatedAction?.Invoke(p_character, villagersToEliminate.Count);
        }
    }
    private void AddVillagerToEliminate(Character p_character) {
        if (!villagersToEliminate.Contains(p_character)) {
            villagersToEliminate.Add(p_character);
            AddCharacterToTrackList(p_character);
            totalCharactersToEliminate++;
            _characterAddedAsTargetAction?.Invoke(p_character);
        }
    }
    #endregion

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

    public void Subscribe(OonaWinConditionTracker.Listener p_listener) {
        _characterEliminatedAction += p_listener.OnCharacterEliminated;
        _characterAddedAsTargetAction += p_listener.OnCharacterAddedAsTarget;
    }
    public void Unsubscribe(OonaWinConditionTracker.Listener p_listener) {
        _characterEliminatedAction -= p_listener.OnCharacterEliminated;
        _characterAddedAsTargetAction -= p_listener.OnCharacterAddedAsTarget;
    }
}

public class SaveDataOonaWinConditionTracker : SaveDataWinConditionTracker {
    public List<string> villagersToEliminate;
    public int totalCharactersToEliminate;
    public override void Save(WinconditionTracker data) {
        base.Save(data);
        OonaWinConditionTracker tracker = data as OonaWinConditionTracker;
        villagersToEliminate = SaveUtilities.ConvertSavableListToIDs(tracker.villagersToEliminate);
        totalCharactersToEliminate = tracker.totalCharactersToEliminate;
    }
}