using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BayatGames.SaveGameFree.Types;
using Factions;

[System.Serializable]
public class SaveDataFaction : SaveData<Faction>, ISavableCounterpart {
    public string _persistentID;
    public OBJECT_TYPE _objectType;
    public int id;
    public string name;
    public string description;
    public bool isMajorFaction;
    public string emblemName;
    public bool isLeaderPlayer;
    public string leaderID;
    public bool isActive;
    public ColorSave factionColor;

    public List<string> characterIDs;
    public List<string> bannedCharacterIDs;
    public List<string> ownedSettlementIDs;

    public Dictionary<string, SaveDataFactionRelationship> relationships;
    public SaveDataFactionType factionType;
    public List<string> history;

    public int newLeaderDesignationChance;

    #region getters
    public string persistentID => _persistentID;
    public OBJECT_TYPE objectType => _objectType;
    #endregion

    #region Overrides
    public override void Save(Faction faction) {
        _persistentID = faction.persistentID;
        _objectType = faction.objectType;
        id = faction.id;
        name = faction.name;
        description = faction.description;
        isMajorFaction = faction.isMajorFaction;
        emblemName = faction.emblem.name;
        factionColor = faction.factionColor;
        isActive = faction.isActive;

        if (faction.leader == null) {
            leaderID = string.Empty;
        } else {
            isLeaderPlayer = faction.leader.objectType == OBJECT_TYPE.Player;
            leaderID = faction.leader.persistentID;
        }

        characterIDs = new List<string>();
        if(faction.characters != null) {
            for (int i = 0; i < faction.characters.Count; i++) {
                characterIDs.Add(faction.characters[i].persistentID);
            }
        }

        bannedCharacterIDs = new List<string>();
        if (faction.bannedCharacters != null) {
            for (int i = 0; i < faction.bannedCharacters.Count; i++) {
                bannedCharacterIDs.Add(faction.bannedCharacters[i].persistentID);
            }
        }

        ownedSettlementIDs = new List<string>();
        if (faction.ownedSettlements != null) {
            for (int i = 0; i < faction.ownedSettlements.Count; i++) {
                ownedSettlementIDs.Add(faction.ownedSettlements[i].persistentID);
            }
        }

        relationships = new Dictionary<string, SaveDataFactionRelationship>();
        foreach (KeyValuePair<Faction, FactionRelationship> item in faction.relationships) {
            SaveDataFactionRelationship saveRel = new SaveDataFactionRelationship();
            saveRel.Save(item.Value);
            relationships.Add(item.Key.persistentID, saveRel);
        }

        factionType = new SaveDataFactionType();
        factionType.Save(faction.factionType);

        history = new List<string>();
        for (int i = 0; i < faction.history.Count; i++) {
            Log log = faction.history[i];
            history.Add(log.persistentID);
            SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(log);
        }
        newLeaderDesignationChance = faction.newLeaderDesignationChance;
    }

    public override Faction Load() {
        Faction faction = FactionManager.Instance.CreateNewFaction(this);
        return faction;
    }
    #endregion

    public void LoadCharacters() {
        Faction faction = FactionManager.Instance.GetFactionByPersistentID(persistentID);
        if (!isLeaderPlayer) {
            if(leaderID != string.Empty) {
                Character character = CharacterManager.Instance.GetCharacterByPersistentID(leaderID);
                faction.OnlySetLeader(character);
            }
        }
        for (int i = 0; i < characterIDs.Count; i++) {
            Character character = CharacterManager.Instance.GetCharacterByPersistentID(characterIDs[i]);
            faction.AddCharacter(character);
        }
        for (int i = 0; i < bannedCharacterIDs.Count; i++) {
            Character character = CharacterManager.Instance.GetCharacterByPersistentID(bannedCharacterIDs[i]);
            faction.AddBannedCharacter(character);
        }
    }
    public void LoadSettlements() {
        //TODO
    }
    public void LoadRelationships() {
        Faction faction1 = FactionManager.Instance.GetFactionByPersistentID(persistentID);
        foreach (KeyValuePair<string, SaveDataFactionRelationship> item in relationships) {
            Faction faction2 = FactionManager.Instance.GetFactionByPersistentID(item.Key);
            FactionRelationship rel = null;
            if (faction1.GetRelationshipWith(faction2) == null) {
                if (rel == null) {
                    item.Value.Load();
                }
                faction1.AddNewRelationship(faction2, rel);
            }
            if (faction2.GetRelationshipWith(faction1) == null) {
                if(rel == null) {
                    item.Value.Load();
                }
                faction2.AddNewRelationship(faction1, rel);
            }
        }
    }
    public void LoadLogs() {
        Faction faction = FactionManager.Instance.GetFactionByPersistentID(persistentID);
        for (int i = 0; i < history.Count; i++) {
            SaveDataLog saveLog = SaveManager.Instance.currentSaveDataProgress.GetFromSaveHub<SaveDataLog>(OBJECT_TYPE.Log, history[i]);
            faction.AddHistory(saveLog.Load());
        }
    }
}
