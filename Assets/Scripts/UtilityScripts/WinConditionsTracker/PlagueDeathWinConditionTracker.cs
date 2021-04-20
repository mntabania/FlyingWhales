using System;
using System.Collections.Generic;

public class PlagueDeathWinConditionTracker : WinConditionTracker {

    public const int Elimination_Requirement = 14;

    public List<Character> villagersToEliminate { get; private set; }
    public int totalCharactersToEliminate { get; private set; }
    public override Type serializedData => typeof(SaveDataPlagueDeathWinConditionTracker);
    
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
            AddVillagerToEliminate(villagers[i]);
        }
        totalCharactersToEliminate = Elimination_Requirement;
    }

    #region Loading
    public override void LoadReferences(SaveDataWinConditionTracker data) {
        base.LoadReferences(data);
        SaveDataPlagueDeathWinConditionTracker tracker = data as SaveDataPlagueDeathWinConditionTracker;
        villagersToEliminate = SaveUtilities.ConvertIDListToCharacters(tracker.villagersToEliminate);
        totalCharactersToEliminate = tracker.totalCharactersToEliminate;
    }
    #endregion
    
    #region List Maintenance
    private void EliminateVillager(Character p_character) {
        if (villagersToEliminate.Remove(p_character)) {
            if (p_character.causeOfDeath == INTERACTION_TYPE.PLAGUE_FATALITY) {
                totalCharactersToEliminate--;
                OnVillagerEliminatedViaPlagueDeath();
            }
        }
        if (totalCharactersToEliminate > villagersToEliminate.Count) {
            if (PlayerManager.Instance.player.hasAlreadyWon) {
                return;
            }
            PlayerUI.Instance.LoseGameOver($"You were not able to plague {Elimination_Requirement.ToString()} villagers. You failed");
        }
    }
    private void AddVillagerToEliminate(Character p_character) {
        if (!villagersToEliminate.Contains(p_character)) {
            villagersToEliminate.Add(p_character);
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
    private void OnVillagerEliminatedViaPlagueDeath() {
        UpdateStepsChangedNameEvent();
        if (totalCharactersToEliminate <= 0) {
            Messenger.Broadcast(PlayerSignals.WIN_GAME, $"You managed to wipe out {Elimination_Requirement.ToString()} Villagers using Plague. Congratulations!");
        }
    }

    #region Win Conditions Steps
    protected override IBookmarkable[] CreateWinConditionSteps() {
        GenericTextBookmarkable plagueFatality = new GenericTextBookmarkable(GetPlagueFatalityText, () => BOOKMARK_TYPE.Text, null, null);
        IBookmarkable[] bookmarkables = new[] {
            plagueFatality
        };
        return bookmarkables;
    }
    private string GetPlagueFatalityText() {
        return $"Plague Fatality deaths: {(Elimination_Requirement - totalCharactersToEliminate).ToString()}/{Elimination_Requirement.ToString()}";
    }
    #endregion
}

public class SaveDataPlagueDeathWinConditionTracker : SaveDataWinConditionTracker {
    public List<string> villagersToEliminate;
    public int totalCharactersToEliminate;
    public override void Save(WinConditionTracker data) {
        base.Save(data);
        PlagueDeathWinConditionTracker tracker = data as PlagueDeathWinConditionTracker;
        villagersToEliminate = SaveUtilities.ConvertSavableListToIDs(tracker.villagersToEliminate);
        totalCharactersToEliminate = tracker.totalCharactersToEliminate;
    }
}