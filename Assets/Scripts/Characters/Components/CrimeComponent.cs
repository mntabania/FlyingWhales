using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrimeComponent : CharacterComponent {

    public List<CrimeData> witnessedCrimes { get; private set; }
    public List<CrimeData> reportedCrimes { get; private set; }

    public CrimeComponent() {
        witnessedCrimes = new List<CrimeData>();
        reportedCrimes = new List<CrimeData>();
    }
    public CrimeComponent(SaveDataCrimeComponent data) {
    }

    #region Crimes
    public void AddWitnessedCrime(CrimeData data) {
        witnessedCrimes.Add(data);
    }
    public void RemoveWitnessedCrime(CrimeData data) {
        witnessedCrimes.Remove(data);
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
    public bool HasUnreportedCrimeOf(Character criminal) {
        for (int i = 0; i < witnessedCrimes.Count; i++) {
            CrimeData data = witnessedCrimes[i];
            if (data.criminal == criminal) {
                if (!reportedCrimes.Contains(data)) {
                    return true;
                }    
            }
        }
        return false;
    }
    public bool CanCreateReportCrimeJob(Character actor, IPointOfInterest target, CrimeData crimeData, ICrimeable crime) {
        string opinionLabel = owner.relationshipContainer.GetOpinionLabel(actor);
        CRIME_SEVERITY severity = crimeData.crimeSeverity;
        if(opinionLabel == RelationshipManager.Close_Friend && severity != CRIME_SEVERITY.Heinous) {
            return false;
        } else if (opinionLabel == RelationshipManager.Friend && severity != CRIME_SEVERITY.Heinous && severity != CRIME_SEVERITY.Serious) {
            return false;
        } else if ((owner.relationshipContainer.IsFamilyMember(actor) || owner.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR))
            && opinionLabel != RelationshipManager.Rival && severity != CRIME_SEVERITY.Heinous) {
            return false;
        }
        return true;
    }
    #endregion

    #region Loading
    public void LoadReferences(SaveDataCrimeComponent data) {
        for (int i = 0; i < data.witnessedCrimes.Count; i++) {
            CrimeData crime = DatabaseManager.Instance.crimeDatabase.GetCrimeByPersistentID(data.witnessedCrimes[i]);
            witnessedCrimes.Add(crime);
        }

        for (int i = 0; i < data.reportedCrimes.Count; i++) {
            CrimeData crime = DatabaseManager.Instance.crimeDatabase.GetCrimeByPersistentID(data.reportedCrimes[i]);
            reportedCrimes.Add(crime);
        }
    }
    #endregion
}

[System.Serializable]
public class SaveDataCrimeComponent : SaveData<CrimeComponent> {
    public List<string> witnessedCrimes;
    public List<string> reportedCrimes;

    #region Overrides
    public override void Save(CrimeComponent data) {
        witnessedCrimes = new List<string>();
        for (int i = 0; i < data.witnessedCrimes.Count; i++) {
            CrimeData crime = data.witnessedCrimes[i];
            witnessedCrimes.Add(crime.persistentID);
            SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(crime);
        }

        reportedCrimes = new List<string>();
        for (int i = 0; i < data.reportedCrimes.Count; i++) {
            CrimeData crime = data.reportedCrimes[i];
            reportedCrimes.Add(crime.persistentID);
            SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(crime);
        }
    }

    public override CrimeComponent Load() {
        CrimeComponent component = new CrimeComponent(this);
        return component;
    }
    #endregion
}