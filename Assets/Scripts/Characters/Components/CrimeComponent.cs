using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrimeComponent {
    public Character owner { get; private set; }

    public List<CrimeData> witnessedCrimes { get; private set; }
    public List<CrimeData> reportedCrimes { get; private set; }

    public CrimeComponent(Character owner) {
        this.owner = owner;
        witnessedCrimes = new List<CrimeData>();
        reportedCrimes = new List<CrimeData>();
    }

    #region Crimes
    public void AddWitnessedCrime(CrimeData data) {
        witnessedCrimes.Add(data);
    }
    public void AddReportedCrime(CrimeData data) {
        reportedCrimes.Add(data);
    }
    public bool IsReported(CrimeData data) {
        return reportedCrimes.Contains(data);
    }
    public bool HasUnreportedCrime() {
        for (int i = 0; i < witnessedCrimes.Count; i++) {
            CrimeData data = witnessedCrimes[i];
            if (!reportedCrimes.Contains(data)) {
                return true;
            }
        }
        return false;
    }
    #endregion
}
