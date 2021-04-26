using System;
using System.Collections.Generic;
using Ruinarch;
using UtilityScripts;

public class WipeOutAllVillagersWinConditionTracker : WinConditionTracker {

    public List<Character> villagersToEliminate { get; private set; }
    public int totalCharactersToEliminate { get; private set; }
    public override Type serializedData => typeof(SaveDataWipeOutAllVillagersWinConditionTracker);
    
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
        SaveDataWipeOutAllVillagersWinConditionTracker tracker = data as SaveDataWipeOutAllVillagersWinConditionTracker;
        villagersToEliminate = SaveUtilities.ConvertIDListToCharacters(tracker.villagersToEliminate);
        totalCharactersToEliminate = tracker.totalCharactersToEliminate;
    }
    #endregion
    
    #region List Maintenance
    private void EliminateVillager(Character p_character) {
        if (villagersToEliminate.Remove(p_character)) {
            totalCharactersToEliminate--;
            OnCharacterEliminated();
        }
    }
    private void AddVillagerToEliminate(Character p_character) {
        if (!villagersToEliminate.Contains(p_character)) {
            villagersToEliminate.Add(p_character);
            totalCharactersToEliminate++;
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
    private void OnCharacterEliminated() {
        UpdateStepsChangedNameEvent();
        if (totalCharactersToEliminate <= 0) {
            Messenger.Broadcast(PlayerSignals.WIN_GAME, "You managed to wipe out all Villagers. Congratulations!");
        }
    }

    #region Win Conditions Steps
    protected override IBookmarkable[] CreateWinConditionSteps() {
        GenericTextBookmarkable eliminateVillagers = new GenericTextBookmarkable(GetEliminateVillagersText, () => BOOKMARK_TYPE.Text, 
            CharacterCenterCycle, null, OnHoverOverAction, OnHoverOutAction);
        IBookmarkable[] bookmarkables = new[] {
            eliminateVillagers
        };
        return bookmarkables;
    }
    private string GetEliminateVillagersText() {
        return $"Eliminate Villagers: {villagersToEliminate.Count.ToString()}";
    }
    private void OnHoverOverAction(UIHoverPosition position) {
        UIManager.Instance.ShowSmallInfo("To eliminate a Villager, either kill it or turn it into a Cultist by brainwashing it in your Prison.", 
            pos: position);
    }
    private void OnHoverOutAction() {
        UIManager.Instance.HideSmallInfo();
    }
    #endregion
    
    #region Center Cycle
    private void CharacterCenterCycle() {
        if (villagersToEliminate != null && villagersToEliminate.Count > 0) {
            //normal objects to center
            ISelectable objToSelect = GetNextCharacterToCenter(villagersToEliminate);
            if (objToSelect != null) {
                InputManager.Instance.Select(objToSelect);
            }
        }
    }
    private Character GetNextCharacterToCenter(List<Character> selectables) {
        Character objToSelect = null;
        for (int i = 0; i < selectables.Count; i++) {
            Character currentSelectable = selectables[i];
            if (currentSelectable.IsCurrentlySelected()) {
                //set next selectable in list to be selected.
                objToSelect = CollectionUtilities.GetNextElementCyclic(selectables, i);
                break;
            }
        }
        if (objToSelect == null) {
            objToSelect = selectables[0];
        }
        return objToSelect;
    }
    #endregion
}

public class SaveDataWipeOutAllVillagersWinConditionTracker : SaveDataWinConditionTracker {
    public List<string> villagersToEliminate;
    public int totalCharactersToEliminate;
    public override void Save(WinConditionTracker data) {
        base.Save(data);
        WipeOutAllVillagersWinConditionTracker tracker = data as WipeOutAllVillagersWinConditionTracker;
        villagersToEliminate = SaveUtilities.ConvertSavableListToIDs(tracker.villagersToEliminate);
        totalCharactersToEliminate = tracker.totalCharactersToEliminate;
    }
}