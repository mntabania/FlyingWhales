﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;

public class CrimeComponent : CharacterComponent {

    public List<CrimeData> witnessedCrimes { get; private set; }
    public List<CrimeData> reportedCrimes { get; private set; }

    //Note: Moved Crimes done by the character here from Criminal trait, because we need to keep the data even if the criminal trait is removed
    public List<CrimeData> activeCrimes { get; protected set; }
    public List<CrimeData> previousCrimes { get; protected set; }

    public CrimeComponent() {
        witnessedCrimes = new List<CrimeData>();
        reportedCrimes = new List<CrimeData>();
        activeCrimes = new List<CrimeData>();
        previousCrimes = new List<CrimeData>();
    }
    public CrimeComponent(SaveDataCrimeComponent data) {
        witnessedCrimes = new List<CrimeData>();
        reportedCrimes = new List<CrimeData>();
        activeCrimes = new List<CrimeData>();
        previousCrimes = new List<CrimeData>();
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
    public CrimeData GetCrimeDataOf(ICrimeable crime) {
        for (int i = 0; i < activeCrimes.Count; i++) {
            CrimeData data = activeCrimes[i];
            if (data.crime == crime) {
                return data;
            }
        }
        return null;
    }
    public CrimeData AddCrime(CRIME_TYPE crimeType, CRIME_SEVERITY crimeSeverity, ICrimeable crime, Character criminal, Criminal criminalTrait, IPointOfInterest target, Faction targetFaction, REACTION_STATUS reactionStatus) {
        CrimeData newData = new CrimeData(crimeType, crimeSeverity, crime, criminal, target, targetFaction);
        activeCrimes.Add(newData);
        newData.OnCrimeAdded();
        return newData;
    }
    public void RemoveAllCrimesWantedBy(Faction faction) {
        for (int i = 0; i < activeCrimes.Count; i++) {
            CrimeData data = activeCrimes[i];
            if (data.IsWantedBy(faction)) {
                data.OnCrimeRemoved();
                previousCrimes.Add(data);
                activeCrimes.RemoveAt(i);
                i--;
            }
        }
        if (activeCrimes.Count <= 0) {
            owner.traitContainer.RemoveTrait(owner, "Criminal");
        }
    }
    public void RemoveAllActiveCrimes() {
        while (activeCrimes.Count > 0) {
            CrimeData data = activeCrimes[0];
            data.OnCrimeRemoved();
            previousCrimes.Add(data);
            activeCrimes.RemoveAt(0);
        }
    }
    public void RemoveCrime(CrimeData crimeData) {
        if (activeCrimes.Remove(crimeData)) {
            crimeData.OnCrimeRemoved();
            previousCrimes.Add(crimeData);
            if (activeCrimes.Count <= 0) {
                owner.traitContainer.RemoveTrait(owner, "Criminal");
            }
        }
    }
    public bool HasCrime(params CRIME_SEVERITY[] severity) {
        for (int i = 0; i < activeCrimes.Count; i++) {
            CrimeData data = activeCrimes[i];
            for (int j = 0; j < severity.Length; j++) {
                CRIME_SEVERITY currSeverity = severity[j];
                if (data.crimeSeverity == currSeverity) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool IsWantedBy(Faction faction) {
        for (int i = 0; i < activeCrimes.Count; i++) {
            CrimeData data = activeCrimes[i];
            if (data.IsWantedBy(faction)) {
                return true;
            }
        }
        return false;
    }
    public bool HasWantedCrime() {
        for (int i = 0; i < activeCrimes.Count; i++) {
            CrimeData data = activeCrimes[i];
            if (data.HasWanted()) {
                return true;
            }
        }
        return false;
    }
    public bool HasWantedCrimeBy(Faction faction, CRIME_STATUS status) {
        for (int i = 0; i < activeCrimes.Count; i++) {
            CrimeData data = activeCrimes[i];
            if (data.crimeStatus == status) {
                if (data.IsWantedBy(faction)) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool IsTargetOfACrime(IPointOfInterest poi) {
        for (int i = 0; i < activeCrimes.Count; i++) {
            CrimeData data = activeCrimes[i];
            if (data.target == poi) {
                return true;
            }
        }
        return false;
    }
    public List<CrimeData> GetListOfCrimesWantedBy(Faction faction) {
        List<CrimeData> dataList = null;
        for (int i = 0; i < activeCrimes.Count; i++) {
            CrimeData data = activeCrimes[i];
            if (data.IsWantedBy(faction)) {
                if (dataList == null) { dataList = new List<CrimeData>(); }
                dataList.Add(data);
            }
        }
        return dataList;
    }
    public List<CrimeData> GetListOfCrimesWantedBy(Faction faction, CRIME_STATUS status) {
        List<CrimeData> dataList = null;
        for (int i = 0; i < activeCrimes.Count; i++) {
            CrimeData data = activeCrimes[i];
            if (data.crimeStatus == status) {
                if (data.IsWantedBy(faction)) {
                    if (dataList == null) { dataList = new List<CrimeData>(); }
                    dataList.Add(data);
                }
            }
        }
        return dataList;
    }
    public CrimeData GetFirstCrimeWantedBy(Faction faction, CRIME_STATUS status) {
        for (int i = 0; i < activeCrimes.Count; i++) {
            CrimeData data = activeCrimes[i];
            if (data.crimeStatus == status) {
                if (data.IsWantedBy(faction)) {
                    return data;
                }
            }
        }
        return null;
    }
    public void SetDecisionAndJudgeToAllUnpunishedCrimesWantedBy(Faction faction, CRIME_STATUS status, Character judge) {
        for (int i = 0; i < activeCrimes.Count; i++) {
            CrimeData data = activeCrimes[i];
            if (data.crimeStatus == CRIME_STATUS.Unpunished) {
                if (data.IsWantedBy(faction)) {
                    data.SetCrimeStatus(status);
                    data.SetJudge(judge);
                }
            }
        }
    }
    public bool IsCrimeAlreadyWitnessedBy(Character character, ICrimeable crime) {
        for (int i = 0; i < activeCrimes.Count; i++) {
            CrimeData data = activeCrimes[i];
            if (data.crime == crime) {
                return data.IsWitness(character);
            }
        }
        for (int i = 0; i < previousCrimes.Count; i++) {
            CrimeData data = previousCrimes[i];
            if (data.crime == crime) {
                return data.IsWitness(character);
            }
        }
        return false;
    }
    public bool IsCrimeAlreadyWitnessedBy(Character character, CRIME_TYPE crimeType) {
        for (int i = 0; i < activeCrimes.Count; i++) {
            CrimeData data = activeCrimes[i];
            if (data.crimeType == crimeType) {
                return data.IsWitness(character);
            }
        }
        return false;
    }
    public bool IsWitnessOfAnyActiveCrime(Character character) {
        for (int i = 0; i < activeCrimes.Count; i++) {
            CrimeData data = activeCrimes[i];
            if (data.IsWitness(character)) {
                return true;
            }
        }
        return false;
    }
    public bool HasNonHostileVillagerInRangeThatConsidersVampirismACrime() {
        if (owner.marker) {
            for (int i = 0; i < owner.marker.inVisionCharacters.Count; i++) {
                Character inVision = owner.marker.inVisionCharacters[i];
                if (inVision != owner) {
                    if (!owner.IsHostileWith(inVision)) {
                        CRIME_SEVERITY severity = CrimeManager.Instance.GetCrimeSeverity(inVision, owner, owner, CRIME_TYPE.Vampire);
                        if (severity != CRIME_SEVERITY.None && severity != CRIME_SEVERITY.Unapplicable) {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        //Return true so that the character will not transform into a bat if he has no marker
        return true;
    }
    #endregion

    #region Loading
    public void LoadReferences(SaveDataCrimeComponent data) {
        if(data.witnessedCrimes != null) {
            for (int i = 0; i < data.witnessedCrimes.Count; i++) {
                CrimeData crime = DatabaseManager.Instance.crimeDatabase.GetCrimeByPersistentID(data.witnessedCrimes[i]);
                witnessedCrimes.Add(crime);
            }
        }

        if (data.reportedCrimes != null) {
            for (int i = 0; i < data.reportedCrimes.Count; i++) {
                CrimeData crime = DatabaseManager.Instance.crimeDatabase.GetCrimeByPersistentID(data.reportedCrimes[i]);
                reportedCrimes.Add(crime);
            }
        }

        if (data.activeCrimes != null) {
            for (int i = 0; i < data.activeCrimes.Count; i++) {
                CrimeData crime = DatabaseManager.Instance.crimeDatabase.GetCrimeByPersistentID(data.activeCrimes[i]);
                activeCrimes.Add(crime);
            }
        }

        if (data.previousCrimes != null) {
            for (int i = 0; i < data.previousCrimes.Count; i++) {
                CrimeData crime = DatabaseManager.Instance.crimeDatabase.GetCrimeByPersistentID(data.previousCrimes[i]);
                previousCrimes.Add(crime);
            }
        }
    }
    #endregion
}

[System.Serializable]
public class SaveDataCrimeComponent : SaveData<CrimeComponent> {
    public List<string> witnessedCrimes;
    public List<string> reportedCrimes;
    public List<string> activeCrimes;
    public List<string> previousCrimes;

    #region Overrides
    public override void Save(CrimeComponent data) {
        if(data.witnessedCrimes != null) {
            witnessedCrimes = new List<string>();
            for (int i = 0; i < data.witnessedCrimes.Count; i++) {
                CrimeData crime = data.witnessedCrimes[i];
                witnessedCrimes.Add(crime.persistentID);
                SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(crime);
            }
        }

        if (data.reportedCrimes != null) {
            reportedCrimes = new List<string>();
            for (int i = 0; i < data.reportedCrimes.Count; i++) {
                CrimeData crime = data.reportedCrimes[i];
                reportedCrimes.Add(crime.persistentID);
                SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(crime);
            }
        }

        if (data.activeCrimes != null) {
            activeCrimes = new List<string>();
            for (int i = 0; i < data.activeCrimes.Count; i++) {
                CrimeData crime = data.activeCrimes[i];
                activeCrimes.Add(crime.persistentID);
                SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(crime);
            }
        }

        if (data.previousCrimes != null) {
            previousCrimes = new List<string>();
            for (int i = 0; i < data.previousCrimes.Count; i++) {
                CrimeData crime = data.previousCrimes[i];
                previousCrimes.Add(crime.persistentID);
                SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(crime);
            }
        }
    }

    public override CrimeComponent Load() {
        CrimeComponent component = new CrimeComponent(this);
        return component;
    }
    #endregion
}