using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BayatGames.SaveGameFree.Types;
using Factions;

[System.Serializable]
public class SaveDataFaction : SaveData<Faction> {
    public int id;
    public string name;
    public string description;
    public bool isMajorFaction;
    public int emblemIndex;
    public bool isLeaderPlayer;
    public int leaderID;
    public bool isActive;
    public ColorSave factionColor;

    public List<int> characterIDs;
    public List<int> bannedCharacterIDs;
    public List<int> ownedSettlementIDs;

    public Dictionary<int, SaveDataFactionRelationship> relationships;
    public SaveDataFactionType factionType;
    public List<SaveDataLog> history;

    public int newLeaderDesignationChance;

    #region Overrides
    public override void Save(Faction faction) {
        id = faction.id;
        name = faction.name;
        description = faction.description;
        isMajorFaction = faction.isMajorFaction;
        emblemIndex = FactionManager.Instance.GetFactionEmblemIndex(faction.emblem);
        factionColor = faction.factionColor;
        isActive = faction.isActive;

        if (faction.leader == null) {
            leaderID = -999;
        } else {
            if(faction.leader is Player) {
                isLeaderPlayer = true;
            }
            leaderID = faction.leader.id;
        }

        characterIDs = new List<int>();
        if(faction.characters != null) {
            for (int i = 0; i < faction.characters.Count; i++) {
                characterIDs.Add(faction.characters[i].id);
            }
        }

        bannedCharacterIDs = new List<int>();
        if (faction.bannedCharacters != null) {
            for (int i = 0; i < faction.bannedCharacters.Count; i++) {
                bannedCharacterIDs.Add(faction.bannedCharacters[i].id);
            }
        }

        ownedSettlementIDs = new List<int>();
        if (faction.ownedSettlements != null) {
            for (int i = 0; i < faction.ownedSettlements.Count; i++) {
                ownedSettlementIDs.Add(faction.ownedSettlements[i].id);
            }
        }

        relationships = new Dictionary<int, SaveDataFactionRelationship>();
        foreach (KeyValuePair<Faction, FactionRelationship> item in faction.relationships) {
            SaveDataFactionRelationship saveRel = new SaveDataFactionRelationship();
            saveRel.Save(item.Value);
            relationships.Add(item.Key.id, saveRel);
        }

        factionType = new SaveDataFactionType();
        factionType.Save(faction.factionType);

        history = new List<SaveDataLog>();
        for (int i = 0; i < faction.history.Count; i++) {
            SaveDataLog saveLog = new SaveDataLog();
            saveLog.Save(faction.history[i]);
            history.Add(saveLog);
        }
        newLeaderDesignationChance = faction.newLeaderDesignationChance;
    }

    public override Faction Load() {
        Faction faction = FactionManager.Instance.CreateNewFaction(this);
        return faction;
    }
    #endregion

    public void LoadCharacters() {
        Faction faction = FactionManager.Instance.GetFactionBasedOnID(id);
        if (!isLeaderPlayer) {
            if(leaderID != -999) {
                Character character = CharacterManager.Instance.GetCharacterByID(leaderID);
                faction.OnlySetLeader(character);
            }
        }
        for (int i = 0; i < characterIDs.Count; i++) {
            Character character = CharacterManager.Instance.GetCharacterByID(characterIDs[i]);
            faction.AddCharacter(character);
        }
        for (int i = 0; i < bannedCharacterIDs.Count; i++) {
            Character character = CharacterManager.Instance.GetCharacterByID(characterIDs[i]);
            faction.AddBannedCharacter(character);
        }
    }
    public void LoadSettlements() {
        //TODO
    }
    public void LoadRelationships() {
        foreach (KeyValuePair<int, SaveDataFactionRelationship> item in relationships) {
            FactionRelationship rel = item.Value.Load();
            if (rel.faction2.GetRelationshipWith(rel.faction1) == null) {
                rel.faction2.AddNewRelationship(rel.faction1, rel);
            }
            if (rel.faction1.GetRelationshipWith(rel.faction2) == null) {
                rel.faction1.AddNewRelationship(rel.faction2, rel);
            }
        }
    }
    public void LoadLogs() {
        Faction faction = FactionManager.Instance.GetFactionBasedOnID(id);
        for (int i = 0; i < history.Count; i++) {
            SaveDataLog saveLog = history[i];
            faction.AddHistory(saveLog.Load());
        }
    }
}
