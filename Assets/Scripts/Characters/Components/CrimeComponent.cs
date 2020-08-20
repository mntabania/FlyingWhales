using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrimeComponent {
    public Character owner { get; private set; }

    public List<CrimeData> witnessedCrimes { get; private set; }

    public CrimeComponent(Character owner) {
        this.owner = owner;
        witnessedCrimes = new List<CrimeData>();
    }

    #region Crimes
    public void AddWitnessedCrime(CrimeData data) {
        witnessedCrimes.Add(data);
    }
    public bool HasUnreportedCrime() {
        for (int i = 0; i < witnessedCrimes.Count; i++) {
            CrimeData data = witnessedCrimes[i];
            if (!data.isReported) {
                return true;
            }
        }
        return false;
    }
    #endregion
}
