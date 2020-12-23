using System;
using System.Collections.Generic;
using System.Linq;

public class FactionDatabase {
    
    public Dictionary<string, Faction> factionsByGUID { get; private set; }
    public List<Faction> allFactionsList { get; private set; }

    public FactionDatabase() {
        factionsByGUID = new Dictionary<string, Faction>();
        allFactionsList = new List<Faction>();
    }

    public void RegisterFaction(Faction faction) {
        factionsByGUID.Add(faction.persistentID, faction);
        allFactionsList.Add(faction);
    }
    public void UnRegisterFaction(Faction faction) {
        factionsByGUID.Remove(faction.persistentID);
        allFactionsList.Remove(faction);
    }

    #region Query
    public Faction GetRandomMajorNonPlayerFaction() {
        List<Faction> factions = null;
        for (int i = 0; i < allFactionsList.Count; i++) {
            Faction faction = allFactionsList[i];
            if (faction.isMajorNonPlayer) {
                if(factions == null) { factions = new List<Faction>(); }
                factions.Add(faction);
            }
        }
        if(factions != null && factions.Count > 0) {
            return factions[UnityEngine.Random.Range(0, factions.Count)];
        }
        return null;
    }
    public Faction GetFactionBasedOnID(int id) {
        for (int i = 0; i < allFactionsList.Count; i++) {
            if (allFactionsList[i].id == id) {
                return allFactionsList[i];
            }
        }
        return null;
    }
    public Faction GetFactionBasedOnPersistentID(string id) {
        if (factionsByGUID.ContainsKey(id)) {
            return factionsByGUID[id];
        }
        throw new Exception($"There was no faction with persistent id {id}");
    }
    public Faction GetFactionByPersistentID(string persistentID) {
        if (factionsByGUID.ContainsKey(persistentID)) {
            return factionsByGUID[persistentID];
        }
        return null;
    }
    public Faction GetFactionBasedOnName(string name) {
        for (int i = 0; i < allFactionsList.Count; i++) {
            if (allFactionsList[i].name.Equals(name, StringComparison.CurrentCultureIgnoreCase)) {
                return allFactionsList[i];
            }
        }
        return null;
    }
    public List<Faction> GetMajorFactionWithRace(RACE race) {
        List<Faction> factions = null;
        for (int i = 0; i < allFactionsList.Count; i++) {
            Faction faction = allFactionsList[i];
            if (faction.race == race && faction.isMajorFaction) {
                if (factions == null) {
                    factions = new List<Faction>();
                }
                factions.Add(faction);
            }
        }
        return factions;
    }
    public List<Faction> GetFactionsWithFactionType(params FACTION_TYPE[] p_factionType) {
        List<Faction> factions = null;
        for (int i = 0; i < allFactionsList.Count; i++) {
            Faction faction = allFactionsList[i];
            if (p_factionType.Contains(faction.factionType.type)) {
                if (factions == null) {
                    factions = new List<Faction>();
                }
                factions.Add(faction);
            }
        }
        return factions;
    }
    #endregion
}
