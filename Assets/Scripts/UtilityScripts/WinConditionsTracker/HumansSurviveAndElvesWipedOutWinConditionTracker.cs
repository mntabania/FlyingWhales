using System;
using System.Collections.Generic;
using System.Linq;
using Ruinarch;
using UnityEngine;
using UtilityScripts;

public class HumansSurviveAndElvesWipedOutWinConditionTracker : WinConditionTracker {

    public const int MinimumHumans = 5;

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
            OnElvenCharacterEliminated(p_character);
        }
        if (humans.Remove(p_character)) {
            OnHumanCharacterEliminated(p_character);
            CheckLoseCondition();
        }
    }
    private void AddVillagerToEliminate(Character p_character) {
        if (p_character.faction != null && !elvenToEliminate.Contains(p_character) && !p_character.isDead && p_character.faction.factionType.type == FACTION_TYPE.Elven_Kingdom) {
            elvenToEliminate.Add(p_character);
        } else if (p_character.faction != null && !humans.Contains(p_character) && p_character.faction.factionType.type == FACTION_TYPE.Human_Empire) {
            humans.Add(p_character);
            totalHumansToProtect = humans.Count;
        }
        UpdateStepsChangedNameEvent();
    }
    #endregion

    private void OnHumanCharacterEliminated(Character p_character) {
        UpdateStepsChangedNameEvent();
        CheckLoseCondition();
    }
    private void OnElvenCharacterEliminated(Character p_character) {
        UpdateStepsChangedNameEvent();
        CheckWinCondition();
    }

    private void OnCharacterRemovedFromFaction(Character p_character, Faction p_faction) {
        if (p_faction.factionType.type == FACTION_TYPE.Elven_Kingdom) {
            if (elvenToEliminate.Contains(p_character)) {
                elvenToEliminate.Remove(p_character);
                OnElvenCharacterEliminated(p_character);
            }
        } else if(p_faction.factionType.type == FACTION_TYPE.Human_Empire){
            if (humans.Contains(p_character)) {
                humans.Remove(p_character);
                OnHumanCharacterEliminated(p_character);
            }
        }
    }
    private void CheckLoseCondition() {
        if (humans.Count < MinimumHumans) {
            if (PlayerManager.Instance.player.hasAlreadyWon) {
                return;
            }
            PlayerUI.Instance.LoseGameOver("Humans out numbered. You failed");
        }
    }
    private void CheckWinCondition() {
        if (elvenToEliminate.Count <= 0 && humans.Count >= MinimumHumans) {
            Messenger.Broadcast(PlayerSignals.WIN_GAME, $"You managed to wipe out {GetMainElvenFaction().name}. Congratulations!");
        }
    }
    private void OnCharacterAddedToFaction(Character p_character, Faction p_faction) {
        if (p_faction.factionType.type == FACTION_TYPE.Elven_Kingdom) {
            if (!elvenToEliminate.Contains(p_character)) {
                elvenToEliminate.Add(p_character);
            }
        } else if (p_faction.factionType.type == FACTION_TYPE.Human_Empire) {
            if (!humans.Contains(p_character)) {
                humans.Add(p_character);
            }
        }
        UpdateStepsChangedNameEvent();
    }
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

    #region Win Conditions Steps
    protected override IBookmarkable[] CreateWinConditionSteps() {
        GenericTextBookmarkable wipeOutBookmarkable = new GenericTextBookmarkable(GetWipeOutText, () => BOOKMARK_TYPE.Text, CenterCycleElves, 
            null, OnHoverEliminateElves, UIManager.Instance.HideSmallInfo);
        GenericTextBookmarkable protectHumansBookmarkable = new GenericTextBookmarkable(GetProtectHumansText, () => BOOKMARK_TYPE.Text, CenterCycleHumans, 
            null, OnHoverProtectHumans, UIManager.Instance.HideSmallInfo);
        IBookmarkable[] bookmarkables = new[] {
            wipeOutBookmarkable, protectHumansBookmarkable
        };
        return bookmarkables;
    }
    private string GetWipeOutText() {
        return $"Wipe Out {GetMainElvenFaction().nameWithColor}. Remaining {elvenToEliminate.Count.ToString()}";
    }
    private string GetProtectHumansText() {
        return $"Protect the humans. Remaining {humans.Count.ToString()}/{MinimumHumans.ToString()}";
    }
    #endregion

    #region Tooltips
    private void OnHoverProtectHumans(UIHoverPosition position) {
        UIManager.Instance.ShowSmallInfo(
            $"Keep at least {MinimumHumans.ToString()} humans alive and part of {GetMainHumanFaction().nameWithColor}.\n" +
            $"Important Notes:\n " +
            $"\t- Human vagrants do not count!\n" +
            $"\t- Human Villagers cannot be replenished, while Elven Villagers can.\n"
            , pos: position);
    }
    private void OnHoverEliminateElves(UIHoverPosition position) {
        UIManager.Instance.ShowSmallInfo(
            $"Wipe out all members of {GetMainElvenFaction().nameWithColor}.\n" +
            $"Important Notes:\n " +
            $"\t- Elven vagrants are considered as eliminated.", pos: position);
    }
    #endregion
    
    #region Center Cycle
    private void CenterCycleElves() {
        CharacterCenterCycle(elvenToEliminate);
    }
    private void CenterCycleHumans() {
        CharacterCenterCycle(humans);
    }
    private void CharacterCenterCycle(List<Character> characters) {
        if (characters != null && characters.Count > 0) {
            //normal objects to center
            ISelectable objToSelect = GetNextCharacterToCenter(characters);
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

public class SaveDataAffattWinConditionTracker : SaveDataWinConditionTracker {
    public List<string> elvensToEliminate;
    public List<string> humans;
    public override void Save(WinConditionTracker data) {
        base.Save(data);
        HumansSurviveAndElvesWipedOutWinConditionTracker humansSurviveAndElvesWipedOutWinConditionTracker = data as HumansSurviveAndElvesWipedOutWinConditionTracker;
        elvensToEliminate = SaveUtilities.ConvertSavableListToIDs(humansSurviveAndElvesWipedOutWinConditionTracker.elvenToEliminate);
        humans = SaveUtilities.ConvertSavableListToIDs(humansSurviveAndElvesWipedOutWinConditionTracker.humans);
    }
}