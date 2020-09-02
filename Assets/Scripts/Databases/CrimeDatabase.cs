using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrimeDatabase {
    public Dictionary<string, CrimeData> allCrimes { get; }

    public CrimeDatabase() {
        allCrimes = new Dictionary<string, CrimeData>();
    }

    public void AddCrime(CrimeData crime) {
        if (!allCrimes.ContainsKey(crime.persistentID)) {
            allCrimes.Add(crime.persistentID, crime);
        }
    }
    public CrimeData GetCrimeByPersistentID(string id) {
        if (allCrimes.ContainsKey(id)) {
            return allCrimes[id];
        } else {
            throw new System.NullReferenceException("Trying to get a crime from the database with id " + id + " but the crime is not loaded");
        }
    }
}