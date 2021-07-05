using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UtilityScripts;
public class FactionSuccessionComponent : FactionComponent {
    private const int SUCCESSOR_LIMIT = 3;
    public Character[] successors { get; private set; }
    public int[] successorWeights { get; private set; }
    public bool hasFirstDayStartedTriggered { get; private set; }

    private WeightedDictionary<Character> _successionWeightedDictionary;

    public FactionSuccessionComponent() {
        successors = new Character[SUCCESSOR_LIMIT];
        successorWeights = new int[SUCCESSOR_LIMIT];
        _successionWeightedDictionary = new WeightedDictionary<Character>();
    }
    public FactionSuccessionComponent(SaveDataFactionSuccessionComponent data) {
        successors = new Character[SUCCESSOR_LIMIT];
        successorWeights = new int[SUCCESSOR_LIMIT];
        _successionWeightedDictionary = new WeightedDictionary<Character>();
        hasFirstDayStartedTriggered = data.hasFirstDayStartedTriggered;
    }

    #region Listeners
    public void AddListeners() {
        Messenger.AddListener<Character, ILeader>(CharacterSignals.ON_SET_AS_FACTION_LEADER, OnCharacterSetAsFactionLeader);
        Messenger.AddListener<Character, Faction>(FactionSignals.CHARACTER_ADDED_TO_FACTION, OnCharacterJoinedFaction);
        Messenger.AddListener<Character, Faction>(FactionSignals.CHARACTER_REMOVED_FROM_FACTION, OnCharacterLeftFaction);
        Messenger.AddListener<Faction, Character>(FactionSignals.BECOME_WANTED_CRIMINAL_OF_FACTION, OnCharacterBecomeWantedCriminalOfFaction);
    }
    public void RemoveListeners() {
        Messenger.RemoveListener<Character, ILeader>(CharacterSignals.ON_SET_AS_FACTION_LEADER, OnCharacterSetAsFactionLeader);
        Messenger.RemoveListener<Character, Faction>(FactionSignals.CHARACTER_ADDED_TO_FACTION, OnCharacterJoinedFaction);
        Messenger.RemoveListener<Character, Faction>(FactionSignals.CHARACTER_REMOVED_FROM_FACTION, OnCharacterLeftFaction);
        Messenger.RemoveListener<Faction, Character>(FactionSignals.BECOME_WANTED_CRIMINAL_OF_FACTION, OnCharacterBecomeWantedCriminalOfFaction);
    }
    private void OnCharacterSetAsFactionLeader(Character newLeader, ILeader prevLeader) {
        if(newLeader.faction == owner) {
            UpdateSuccessors();
        }
    }
    private void OnCharacterLeftFaction(Character character, Faction faction) {
        if (faction == owner) {
            UpdateSuccessors();
        }
    }
    private void OnCharacterJoinedFaction(Character character, Faction faction) {
        if (faction == owner) {
            UpdateSuccessors();
        }
    }
    private void OnCharacterBecomeWantedCriminalOfFaction(Faction faction, Character character) {
        if (faction == owner) {
            UpdateSuccessors();
        }
    }
    public void OnCharacterDied(Character character) {
        for (int i = 0; i < successors.Length; i++) {
            Character currSuccessor = successors[i];
            if(currSuccessor != null && currSuccessor == character) {
                UpdateSuccessors();
                break;
            }
        }
    }
    public void OnDayStarted() {
        //On the first Day Started broadcast of signal, do not update successors
        //The reason for this is there are already successors at the start of the game, so we do not need to update it again at the start of game progression
        if(!hasFirstDayStartedTriggered) {
            hasFirstDayStartedTriggered = true;
        } else {
            UpdateSuccessors();
        }
    }
    #endregion

    #region Succession
    public void UpdateSuccessors() {
        _successionWeightedDictionary.Clear();
        List<Character> successorList = RuinarchListPool<Character>.Claim();
        ResetSuccessors();
        owner.factionType.succession.PopulateSuccessorListWeightsInOrder(successorList, _successionWeightedDictionary, owner);

        if(successorList.Count > 0) {
            for (int i = 0; i < successorList.Count; i++) {
                if(i < SUCCESSOR_LIMIT) {
                    Character successor = successorList[i];
                    SetSuccessor(successor, _successionWeightedDictionary.GetElementWeight(successor), i);
                }
            }
        }
        Messenger.Broadcast(FactionSignals.UPDATED_SUCCESSORS, owner);
        RuinarchListPool<Character>.Release(successorList);
    }
    private void ResetSuccessors() {
        for (int i = 0; i < successors.Length; i++) {
            successors[i] = null;
            successorWeights[i] = 0;
        }
    }
    public void SetSuccessor(Character character, int weight, int index) {
        if(index >= 0 && index < successors.Length) {
            successors[index] = character;
            successorWeights[index] = weight;
        }
    }
    private bool AreAllSuccessorSlotsFilled() {
        for (int i = 0; i < successors.Length; i++) {
            if(successors[i] == null) {
                return false;
            }
        }
        return true;
    }
    public bool IsSuccessor(Character character) {
        for (int i = 0; i < successors.Length; i++) {
            Character currSuccessor = successors[i];
            if(currSuccessor != null && currSuccessor == character) {
                return true;
            }
        }
        return false;
    }
    public int GetTotalWeightsOfSuccessors() {
        return successorWeights.Sum();
    }
    public int GetWeightOfSuccessor(Character character) {
        for (int i = 0; i < successors.Length; i++) {
            Character currSuccessor = successors[i];
            if (currSuccessor != null && currSuccessor == character) {
                return successorWeights[i];
            }
        }
        return 0;
    }
    public Character PickSuccessor() {
        return owner.factionType.succession.PickSuccessor(successors, successorWeights);
    }
    #endregion

    #region Loading
    public void LoadReferences(SaveDataFactionSuccessionComponent data) {
        for (int i = 0; i < data.successors.Length; i++) {
            string persistentID = data.successors[i];
            if (!string.IsNullOrEmpty(persistentID)) {
                successors[i] = CharacterManager.Instance.GetCharacterByPersistentID(persistentID);
            }
        }
        for (int i = 0; i < data.successorWeights.Length; i++) {
            successorWeights[i] = data.successorWeights[i];
        }
    }
    #endregion
}

[System.Serializable]
public class SaveDataFactionSuccessionComponent : SaveData<FactionSuccessionComponent> {
    public string[] successors;
    public int[] successorWeights;
    public bool hasFirstDayStartedTriggered;

    #region Overrides
    public override void Save(FactionSuccessionComponent data) {
        hasFirstDayStartedTriggered = data.hasFirstDayStartedTriggered;

        successors = new string[data.successors.Length];
        for (int i = 0; i < data.successors.Length; i++) {
            Character successor = data.successors[i];
            if(successor != null) {
                successors[i] = successor.persistentID;
            } else {
                successors[i] = string.Empty;
            }
        }

        successorWeights = new int[data.successorWeights.Length];
        for (int i = 0; i < data.successorWeights.Length; i++) {
            successorWeights[i] = data.successorWeights[i];
        }
    }

    public override FactionSuccessionComponent Load() {
        FactionSuccessionComponent component = new FactionSuccessionComponent(this);
        return component;
    }
    #endregion
}