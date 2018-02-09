﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class ConsoleMenu : UIMenu {

    public bool isShowing = false;

    private Dictionary<string, Action<string[]>> _consoleActions;

    private List<string> commandHistory;
    private int currentHistoryIndex;

    [SerializeField] private UILabel consoleLbl;
    [SerializeField] private UIInput consoleInputField;

    [SerializeField] private GameObject commandHistoryGO;
    [SerializeField] private UILabel commandHistoryLbl;

    internal override void Initialize() {
        commandHistory = new List<string>();
        _consoleActions = new Dictionary<string, Action<string[]>>() {
            {"/help", ShowHelp},
            {"/change_faction_rel_stat", ChangeFactionRelationshipStatus},
            {"/force_accept_quest", AcceptQuest},
            {"/kill",  KillCharacter},
            {"/quest_cancel", CancelQuest},
            {"/adjust_gold", AdjustGold}
        };
    }

    public void ShowConsole() {
        isShowing = true;
        this.gameObject.SetActive(true);
        ClearCommandField();
        consoleInputField.isSelected = true;
    }

    public void HideConsole() {
        isShowing = false;
        this.gameObject.SetActive(false);
        consoleInputField.isSelected = false;
        HideCommandHistory();
        ClearCommandHistory();
    }
    private void ClearCommandField() {
        consoleLbl.text = string.Empty;
    }
    private void ClearCommandHistory() {
        commandHistoryLbl.text = string.Empty;
        commandHistory.Clear();
    }
    private void ShowCommandHistory() {
        commandHistoryGO.SetActive(true);
    }
    private void HideCommandHistory() {
        commandHistoryGO.SetActive(false);
    }

    public void SubmitCommand() {
        string command = consoleLbl.text;
        string[] words = command.Split(' ');
        string mainCommand = words[0];
        if (_consoleActions.ContainsKey(mainCommand)) {
            _consoleActions[mainCommand](words);
        } else {
            AddCommandHistory(command);
            AddErrorMessage("Error: there is no such command as " + mainCommand + "![-]");
        }
    }
    private void AddCommandHistory(string history) {
        commandHistoryLbl.text += history + "\n";
        commandHistory.Add(history);
        currentHistoryIndex = commandHistory.Count - 1;
        ShowCommandHistory();
    }
    private void AddErrorMessage(string errorMessage) {
        errorMessage += ". Use /help for a list of commands";
        commandHistoryLbl.text += "[FF0000]" + errorMessage + "[-]\n";
        ShowCommandHistory();
    }
    private void AddSuccessMessage(string successMessage) {
        commandHistoryLbl.text += "[00FF00]" + successMessage + "[-]\n";
        ShowCommandHistory();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.UpArrow)) {
            int newIndex = currentHistoryIndex - 1;
            string command = commandHistory.ElementAtOrDefault(newIndex);
            if (!string.IsNullOrEmpty(command)) {
                consoleLbl.text = command;
                currentHistoryIndex = newIndex;
            }
        }
        if (Input.GetKeyDown(KeyCode.DownArrow)) {
            int newIndex = currentHistoryIndex + 1;
            string command = commandHistory.ElementAtOrDefault(newIndex);
            if (!string.IsNullOrEmpty(command)) {
                consoleLbl.text = command;
                currentHistoryIndex = newIndex;
            }
        }
    }

    #region Misc
    private void ShowHelp(string[] parameters) {
        for (int i = 0; i < _consoleActions.Count; i++) {
            AddCommandHistory(_consoleActions.Keys.ElementAt(i));
        }
    }
    #endregion

    #region Faction Relationship
    private void ChangeFactionRelationshipStatus(string[] parameters) {
        if (parameters.Length != 4) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of /change_faction_rel_stat");
            return;
        }
        string faction1ParameterString = parameters[1];
        string faction2ParameterString = parameters[2];
        string newRelStatusString = parameters[3];

        Faction faction1;
        Faction faction2;

        int faction1ID = -1;
        int faction2ID = -1;

        bool isFaction1Numeric = int.TryParse(faction1ParameterString, out faction1ID);
        bool isFaction2Numeric = int.TryParse(faction2ParameterString, out faction2ID);

        string faction1Name = parameters[1];
        string faction2Name = parameters[2];

        RELATIONSHIP_STATUS newRelStatus;

        if (isFaction1Numeric) {
            faction1 = FactionManager.Instance.GetFactionBasedOnID(faction1ID);
        } else {
            faction1 = FactionManager.Instance.GetFactionBasedOnName(faction1Name);
        }

        if (isFaction2Numeric) {
            faction2 = FactionManager.Instance.GetFactionBasedOnID(faction2ID);
        } else {
            faction2 = FactionManager.Instance.GetFactionBasedOnName(faction2Name);
        }

        try {
            newRelStatus = (RELATIONSHIP_STATUS)Enum.Parse(typeof(RELATIONSHIP_STATUS), newRelStatusString, true);
        } catch {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of /change_faction_rel_stat");
            return;
        }

        if (faction1 == null || faction2 == null) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of /change_faction_rel_stat");
            return;
        }

        FactionRelationship rel = FactionManager.Instance.GetRelationshipBetween(faction1, faction2);
        rel.ChangeRelationshipStatus(newRelStatus);

        AddSuccessMessage("Changed relationship status of " + faction1.name + " and " + faction2.name + " to " + rel.relationshipStatus.ToString());
    }
    #endregion

    #region Quests
    private void AcceptQuest(string[] parameters) {
        if (parameters.Length != 3) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of /force_accept_quest");
            return;
        }
        string questParameterString = parameters[1];
        string characterParameterString = parameters[2];

        int questID;
        int characterID;

        bool isQuestParameterNumeric = int.TryParse(questParameterString, out questID);
        bool isCharacterParameterNumeric = int.TryParse(characterParameterString, out characterID);

        if (isQuestParameterNumeric && isCharacterParameterNumeric) {
            Quest quest = FactionManager.Instance.GetQuestByID(questID);
            ECS.Character character = FactionManager.Instance.GetCharacterByID(characterID);

            if(character.currentTask != null) {
                character.SetTaskToDoNext(quest);
                //cancel character's current quest
                character.currentTask.EndTask(TASK_STATUS.CANCEL);
            } else {
                quest.PerformTask(character);
            }
            

            AddSuccessMessage(character.name + " has accepted quest " + quest.questType.ToString());
        } else {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of /force_accept_quest");
        }
    }
    private void CancelQuest(string[] parameters) {
        if (parameters.Length != 2) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of /quest_cancel");
            return;
        }
        string questParameterString = parameters[1];

        int questID;

        bool isQuestParameterNumeric = int.TryParse(questParameterString, out questID);
        if (isQuestParameterNumeric) {
            Quest quest = FactionManager.Instance.GetQuestByID(questID);
            quest.GoBackToQuestGiver(TASK_STATUS.CANCEL);

            AddSuccessMessage(quest.questType.ToString() + " quest posted at " + quest.postedAt.location.name + " was cancelled.");
        } else {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of /cancel_quest");
        }
    }
    #endregion

    #region Characters
    private void KillCharacter(string[] parameters) {
        if (parameters.Length != 2) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of /kill");
            return;
        }
        string characterParameterString = parameters[1];
        int characterID;

        bool isCharacterParameterNumeric = int.TryParse(characterParameterString, out characterID);

        if (isCharacterParameterNumeric) {
            ECS.Character character = FactionManager.Instance.GetCharacterByID(characterID);
            character.Death();
        } else {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of /kill");
        }
    }
    private void AdjustGold(string[] parameters) {
        if (parameters.Length != 3) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of /adjust_gold");
            return;
        }
        string characterParameterString = parameters[1];
        string goldAdjustmentParamterString = parameters[2];

        int characterID;
        int goldAdjustment;

        bool isCharacterParameterNumeric = int.TryParse(characterParameterString, out characterID);
        bool isGoldParameterNumeric = int.TryParse(goldAdjustmentParamterString, out goldAdjustment);
        if (isCharacterParameterNumeric && isGoldParameterNumeric) {
            ECS.Character character = FactionManager.Instance.GetCharacterByID(characterID);
            character.AdjustGold(goldAdjustment);
            AddSuccessMessage(character.name + "'s gold was adjusted by " + goldAdjustment.ToString() + ". New gold is " + character.gold.ToString());
        } else {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of /adjust_gold");
        }
    }
    #endregion


}
