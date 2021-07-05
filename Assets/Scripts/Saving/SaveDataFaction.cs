using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BayatGames.SaveGameFree.Types;
using Factions;

[System.Serializable]
public class SaveDataFaction : SaveData<Faction>, ISavableCounterpart {
    public string persistentID { get; set; }
    public int id;
    public string name;
    public string description;
    public bool isMajorFaction;
    public string emblemName;
    public bool isLeaderPlayer;
    public string leaderID;
    public bool isActive;
    public ColorSave factionColor;
    public RACE race;

    public List<string> characterIDs;
    public List<string> bannedCharacterIDs;
    public List<string> ownedSettlementIDs;

    public Dictionary<string, SaveDataFactionRelationship> relationships;
    public SaveDataFactionType factionType;
    public List<string> history;

    public int newLeaderDesignationChance;

    public SaveDataPartyQuestBoard partyQuestBoard;

    public uint pathfindingTag;
    public uint pathfindingDoorTag;

    //Components
    public SaveDataFactionIdeologyComponent ideologyComponent;
    public SaveDataFactionSuccessionComponent successionComponent;

    public bool isInfoUnlocked;
    public bool isDisbanded;

    #region getters
    public OBJECT_TYPE objectType => OBJECT_TYPE.Faction;
    #endregion

    #region Overrides
    public override void Save(Faction data) {
        persistentID = data.persistentID;
        id = data.id;
        name = data.name;
        description = data.description;
        isMajorFaction = data.isMajorFaction;
        emblemName = data.emblemName;
        factionColor = data.factionColor;
        isActive = data.isActive;
        race = data.race;

        if (data.leader == null) {
            leaderID = string.Empty;
        } else {
            isLeaderPlayer = data.leader.objectType == OBJECT_TYPE.Player;
            leaderID = data.leader.persistentID;
        }

        characterIDs = new List<string>();
        if(data.characters != null) {
            for (int i = 0; i < data.characters.Count; i++) {
                characterIDs.Add(data.characters[i].persistentID);
            }
        }

        bannedCharacterIDs = new List<string>();
        if (data.bannedCharacters != null) {
            for (int i = 0; i < data.bannedCharacters.Count; i++) {
                bannedCharacterIDs.Add(data.bannedCharacters[i].persistentID);
            }
        }

        ownedSettlementIDs = new List<string>();
        if (data.ownedSettlements != null) {
            for (int i = 0; i < data.ownedSettlements.Count; i++) {
                ownedSettlementIDs.Add(data.ownedSettlements[i].persistentID);
            }
        }

        relationships = new Dictionary<string, SaveDataFactionRelationship>();
        foreach (KeyValuePair<Faction, FactionRelationship> item in data.relationships) {
            SaveDataFactionRelationship saveRel = new SaveDataFactionRelationship();
            saveRel.Save(item.Value);
            relationships.Add(item.Key.persistentID, saveRel);
        }

        factionType = new SaveDataFactionType();
        factionType.Save(data.factionType);

        partyQuestBoard = new SaveDataPartyQuestBoard();
        partyQuestBoard.Save(data.partyQuestBoard);

        ideologyComponent = new SaveDataFactionIdeologyComponent(); ideologyComponent.Save(data.ideologyComponent);
        successionComponent = new SaveDataFactionSuccessionComponent(); successionComponent.Save(data.successionComponent);

        // history = new List<string>();
        // for (int i = 0; i < data.history.Count; i++) {
        //     Log log = data.history[i];
        //     history.Add(log.persistentID);
        //     SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(log);
        // }
        newLeaderDesignationChance = data.newLeaderDesignationChance;
        pathfindingTag = data.pathfindingTag;
        isInfoUnlocked = data.isInfoUnlocked;
        pathfindingDoorTag = data.pathfindingDoorTag;
        isDisbanded = data.isDisbanded;
    }

    public override Faction Load() {
        Faction faction = FactionManager.Instance.CreateNewFaction(this);
        return faction;
    }
    #endregion

}
