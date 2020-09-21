using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyDatabase {
    public Dictionary<string, Party> allParties { get; }

    public PartyDatabase() {
        allParties = new Dictionary<string, Party>();
    }

    public void AddParty(Party party) {
        if (!allParties.ContainsKey(party.persistentID)) {
            allParties.Add(party.persistentID, party);
        }
    }
    public void RemoveParty(Party party) {
        if (allParties.ContainsKey(party.persistentID)) {
            allParties.Remove(party.persistentID);
        }
    }
    public Party GetPartyByPersistentID(string id) {
        if (allParties.ContainsKey(id)) {
            return allParties[id];
        } else {
            throw new System.NullReferenceException("Trying to get a party from the database with id " + id + " but the party is not loaded");
        }
    }
    public Party GetPartyByName(string name) {
        foreach (Party party in allParties.Values) {
            if(party.partyName == name) {
                return party;
            }
        }
        return null;
    }
}

public class PartyQuestDatabase {
    public Dictionary<string, PartyQuest> allPartyQuests { get; }

    public PartyQuestDatabase() {
        allPartyQuests = new Dictionary<string, PartyQuest>();
    }

    public void AddPartyQuest(PartyQuest party) {
        if (!allPartyQuests.ContainsKey(party.persistentID)) {
            allPartyQuests.Add(party.persistentID, party);
        }
    }
    public void RemovePartyQuest(PartyQuest party) {
        if (allPartyQuests.ContainsKey(party.persistentID)) {
            allPartyQuests.Remove(party.persistentID);
        }
    }
    public PartyQuest GetPartyQuestByPersistentID(string id) {
        if (allPartyQuests.ContainsKey(id)) {
            return allPartyQuests[id];
        } else {
            throw new System.NullReferenceException("Trying to get a party quest from the database with id " + id + " but the party is not loaded");
        }
    }
}