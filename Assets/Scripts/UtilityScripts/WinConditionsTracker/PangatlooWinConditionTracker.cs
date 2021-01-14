using System.Collections.Generic;
using System.Linq;
using Ruinarch;
using UnityEngine;
using UtilityScripts;

public class PangatlooWinConditionTracker : WinconditionTracker {

    private System.Action<Character> _characterEliminatedAction;
    private System.Action<Character> _characterAddedAsTargetAction;
    private System.Action<int, int> _onDayChangedAction;

    public interface Listener {
        void OnCharacterEliminated(Character p_character);
        void OnCharacterAddedAsTarget(Character p_character);
        void OnDayChangedAction(int currentDay, int p_villagersCount);
    }

    public List<Character> villagersToEliminate { get; private set; }
    public int totalCharactersToEliminate { get; private set; }

    public override void Initialize(List<Character> p_allCharacters) {
        base.Initialize(p_allCharacters);

        villagersToEliminate = new List<Character>();
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, CheckIfCharacterIsEliminated);
        Messenger.AddListener<Character>(FactionSignals.FACTION_SET, CheckIfCharacterIsEliminated);
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_BECOME_CULTIST, CheckIfCharacterIsEliminated);
        Messenger.AddListener<Character>(WorldEventSignals.NEW_VILLAGER_ARRIVED, OnNewVillagerArrived);
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_NO_LONGER_CULTIST, OnCharacterNoLongerCultist);
        Messenger.AddListener<int>(Signals.DAY_STARTED, OnDayChange);

        List<Character> villagers = GetAllCharactersToBeEliminated(p_allCharacters);
        villagersToEliminate.Clear();
        for (int i = 0; i < villagers.Count; i++) {
            AddVillagerToEliminate(villagers[i]);
        }
        totalCharactersToEliminate = villagersToEliminate.Count;
    }

    #region List Maintenance
    private void EliminateVillager(Character p_character) {
        if (villagersToEliminate.Remove(p_character)) {
            totalCharactersToEliminate--;
            RemoveCharacterFromTrackList(p_character);
            _characterEliminatedAction?.Invoke(p_character);
        }
    }
    private void AddVillagerToEliminate(Character p_character) {
        if (!villagersToEliminate.Contains(p_character) && !p_character.isDead) {
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
    private void OnDayChange(int p_currentDay) {
        if (p_currentDay > 8 && villagersToEliminate.Count > 0) {
            PlayerUI.Instance.LoseGameOver("You failed to eliminate all the villagers!");
        } else {
            _onDayChangedAction?.Invoke(p_currentDay, villagersToEliminate.Count);
        }
    }

    public void Subscribe(Listener p_listener) {
        _characterEliminatedAction += p_listener.OnCharacterEliminated;
        _characterAddedAsTargetAction += p_listener.OnCharacterAddedAsTarget;
        _onDayChangedAction += p_listener.OnDayChangedAction;
    }
    public void Unsubscribe(Listener p_listener) {
        _characterEliminatedAction -= p_listener.OnCharacterEliminated;
        _characterAddedAsTargetAction -= p_listener.OnCharacterAddedAsTarget;
        _onDayChangedAction -= p_listener.OnDayChangedAction;
    }
}