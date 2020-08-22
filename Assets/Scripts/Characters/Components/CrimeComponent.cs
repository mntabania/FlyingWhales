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
}
