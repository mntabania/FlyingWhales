using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GatheringDatabase {
    public Dictionary<string, Gathering> allGatherings { get; }

    public GatheringDatabase() {
        allGatherings = new Dictionary<string, Gathering>();
    }

    public void AddGathering(Gathering gathering) {
        if (!allGatherings.ContainsKey(gathering.persistentID)) {
            allGatherings.Add(gathering.persistentID, gathering);
        }
    }
    public void RemoveGathering(Gathering gathering) {
        if (allGatherings.ContainsKey(gathering.persistentID)) {
            allGatherings.Remove(gathering.persistentID);
        }
    }
    public Gathering GetGatheringByPersistentID(string id) {
        if (allGatherings.ContainsKey(id)) {
            return allGatherings[id];
        } else {
            throw new System.NullReferenceException("Trying to get a gathering from the database with id " + id + " but the party is not loaded");
        }
    }
}