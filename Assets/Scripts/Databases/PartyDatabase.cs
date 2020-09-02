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
    public Party GetPartyByPersistentID(string id) {
        if (allParties.ContainsKey(id)) {
            return allParties[id];
        } else {
            throw new System.NullReferenceException("Trying to get a party from the database with id " + id + " but the party is not loaded");
        }
    }
}