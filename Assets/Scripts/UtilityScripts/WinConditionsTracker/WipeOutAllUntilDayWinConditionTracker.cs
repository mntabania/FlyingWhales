using System;
using System.Collections.Generic;
using System.Linq;
using Ruinarch;
using UnityEngine;
using UtilityScripts;

public class WipeOutAllUntilDayWinConditionTracker : WinConditionTracker {

    public const int DueDay = 10;

    public List<Character> villagersToEliminate { get; private set; }
    public override Type serializedData => typeof(SaveDataWipeOutUntilDayWinConditionTracker);
    
    public override void Initialize(List<Character> p_allCharacters) {
        base.Initialize(p_allCharacters);

        villagersToEliminate = new List<Character>();
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, CheckIfCharacterIsEliminated);
        Messenger.AddListener<Character>(FactionSignals.FACTION_SET, CheckIfCharacterIsEliminated);
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_BECOME_CULTIST, CheckIfCharacterIsEliminated);
        Messenger.AddListener<Character>(WorldEventSignals.NEW_VILLAGER_ARRIVED, OnNewVillagerArrived);
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_NO_LONGER_CULTIST, OnCharacterNoLongerCultist);
        Messenger.AddListener(Signals.DAY_STARTED, OnDayChange);

        List<Character> villagers = GetAllCharactersToBeEliminated(p_allCharacters);
        villagersToEliminate.Clear();
        for (int i = 0; i < villagers.Count; i++) {
            AddVillagerToEliminate(villagers[i]);
        }
    }
    
    #region Loading
    public override void LoadReferences(SaveDataWinConditionTracker data) {
        base.LoadReferences(data);
        SaveDataWipeOutUntilDayWinConditionTracker tracker = data as SaveDataWipeOutUntilDayWinConditionTracker;
        villagersToEliminate = SaveUtilities.ConvertIDListToCharacters(tracker.villagersToEliminate);
    }
    #endregion

    #region List Maintenance
    private void EliminateVillager(Character p_character) {
        if (villagersToEliminate.Remove(p_character)) {
            OnCharacterEliminated(p_character);
        }
    }
    private void AddVillagerToEliminate(Character p_character) {
        if (!villagersToEliminate.Contains(p_character) && !p_character.isDead) {
            villagersToEliminate.Add(p_character);
            UpdateStepsChangedNameEvent();
        }
    }
    #endregion

    private void CheckIfCharacterIsEliminated(Character p_character) {
        if (ShouldConsiderCharacterAsEliminated(p_character)) {
            EliminateVillager(p_character);
        }
    }
    private void OnNewVillagerArrived(Character newVillager) {
        AddVillagerToEliminate(newVillager);
    }
    private void OnCharacterNoLongerCultist(Character p_character) {
        AddVillagerToEliminate(p_character);
    }
    private void OnDayChange() {
        int p_currentDay = GameManager.Instance.continuousDays; 
        OnDayChanged(p_currentDay);
    }
    private void OnCharacterEliminated(Character p_character) {
        UpdateStepsChangedNameEvent();
        if (villagersToEliminate.Count <= 0) {
            Messenger.Broadcast(PlayerSignals.WIN_GAME, $"You've successfully wiped out all villagers before Day {(DueDay + 1).ToString()}. Congratulations!");
        }
    }
    private void OnDayChanged(int p_currentDay) {
        UpdateStepsChangedNameEvent();
        if (p_currentDay > DueDay && villagersToEliminate.Count > 0) {
            if (PlayerManager.Instance.player.hasAlreadyWon) {
                return;
            }
            PlayerUI.Instance.LoseGameOver("You failed to eliminate all the villagers!");
        }
    }

    #region Win Conditions Steps
    protected override IBookmarkable[] CreateWinConditionSteps() {
        GenericTextBookmarkable undeadInvasion = new GenericTextBookmarkable(GetUndeadInvasionText, () => BOOKMARK_TYPE.Text, null, null, null, null);
        GenericTextBookmarkable remainingVillagers = new GenericTextBookmarkable(GetRemainingVillagers, () => BOOKMARK_TYPE.Text, null, null, null, null);
        IBookmarkable[] bookmarkables = new[] {
            undeadInvasion, remainingVillagers
        };
        return bookmarkables;
    }
    private string GetUndeadInvasionText() {
        return $"Days until the Undead invasion: {Mathf.Max(0, DueDay - GameManager.Instance.continuousDays).ToString()}";
    }
    private string GetRemainingVillagers() {
        return $"Remaining villagers : {villagersToEliminate.Count.ToString()}";
    }
    #endregion
}

public class SaveDataWipeOutUntilDayWinConditionTracker : SaveDataWinConditionTracker {
    public List<string> villagersToEliminate;
    public override void Save(WinConditionTracker data) {
        base.Save(data);
        WipeOutAllUntilDayWinConditionTracker tracker = data as WipeOutAllUntilDayWinConditionTracker;
        villagersToEliminate = SaveUtilities.ConvertSavableListToIDs(tracker.villagersToEliminate);
    }
}