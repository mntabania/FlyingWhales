using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Interrupts;
using Traits;
using UnityEngine.Assertions;
namespace Traits {
    public class Criminal : Status {

        public List<CrimeData> activeCrimes { get; protected set; }
        public List<CrimeData> previousCrimes { get; protected set; }
        public Character owner { get; private set; }
        public List<Character> charactersThatAreAlreadyWorried { get; private set; }
        public bool isImprisoned { get; private set; }

        #region getters
        public override Type serializedData => typeof(SaveDataCriminal);
        #endregion
        
        public Criminal() {
            name = "Criminal";
            description = "Has been witnessed or accused of doing something illegal.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = 0;
            charactersThatAreAlreadyWorried = new List<Character>();
            activeCrimes = new List<CrimeData>();
            previousCrimes = new List<CrimeData>();
        }

        #region Loading
        public override void LoadSecondWaveInstancedTrait(SaveDataTrait saveDataTrait) {
            base.LoadSecondWaveInstancedTrait(saveDataTrait);
            SaveDataCriminal saveDataCriminal = saveDataTrait as SaveDataCriminal;
            Assert.IsNotNull(saveDataCriminal);
            for (int i = 0; i < saveDataCriminal.activeCrimeIDs.Count; i++) {
                string crimeID = saveDataCriminal.activeCrimeIDs[i];
                CrimeData crimeData = DatabaseManager.Instance.crimeDatabase.GetCrimeByPersistentID(crimeID);
                activeCrimes.Add(crimeData);
            }
            for (int i = 0; i < saveDataCriminal.previousCrimeIDs.Count; i++) {
                string crimeID = saveDataCriminal.previousCrimeIDs[i];
                CrimeData crimeData = DatabaseManager.Instance.crimeDatabase.GetCrimeByPersistentID(crimeID);
                previousCrimes.Add(crimeData);
            }
            charactersThatAreAlreadyWorried = SaveUtilities.ConvertIDListToCharacters(saveDataCriminal.alreadyWorriedCharacterIDs);
            isImprisoned = saveDataCriminal.isImprisoned;
        }
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            if (addTo is Character) {
                owner = addTo as Character;
            }
        }
        #endregion
        
        #region Overrides
        public override void OnAddTrait(ITraitable sourcePOI) {
            base.OnAddTrait(sourcePOI);
            if (sourcePOI is Character) {
                owner = sourcePOI as Character;
                owner.CancelOrUnassignRemoveTraitRelatedJobs();
            }

        }
        public override void OnRemoveTrait(ITraitable sourcePOI, Character removedBy) {
            owner.ForceCancelAllJobsTargetingThisCharacter(JOB_TYPE.APPREHEND);
            RemoveAllActiveCrimes();
            base.OnRemoveTrait(sourcePOI, removedBy);
        }
        protected override string GetDescriptionInUI() {
            string desc = base.GetDescriptionInUI();
            if(activeCrimes.Count > 0) {
                desc += "\n\nACTIVE CRIMES";
                for (int i = 0; i < activeCrimes.Count; i++) {
                    CrimeData data = activeCrimes[i];
                    desc += "\n" + data.GetCrimeDataDescription();
                }
            }
            if (previousCrimes.Count > 0) {
                desc += "\n\nPREVIOUS CRIMES";
                for (int i = 0; i < previousCrimes.Count; i++) {
                    CrimeData data = previousCrimes[i];
                    desc += "\n" + data.GetCrimeDataDescription();
                }
            }
            return desc;
        }
        #endregion

        #region General
        public void AddCharacterThatIsAlreadyWorried(Character character) {
            charactersThatAreAlreadyWorried.Add(character);
        }
        public bool HasCharacterThatIsAlreadyWorried(Character character) {
            return charactersThatAreAlreadyWorried.Contains(character);
        }
        public void SetIsImprisoned(bool state) {
            if(isImprisoned != state) {
                isImprisoned = state;
            }
        }
        public CrimeData GetCrimeDataOf(ICrimeable crime) {
            for (int i = 0; i < activeCrimes.Count; i++) {
                CrimeData data = activeCrimes[i];
                if(data.crime == crime) {
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
            if(activeCrimes.Count <= 0) {
                owner.traitContainer.RemoveTrait(owner, this);
            }
        }
        private void RemoveAllActiveCrimes() {
            while(activeCrimes.Count > 0) {
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
                    owner.traitContainer.RemoveTrait(owner, this);
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
                    if(dataList == null) { dataList = new List<CrimeData>(); }
                    dataList.Add(data);
                }
            }
            return dataList;
        }
        public List<CrimeData> GetListOfCrimesWantedBy(Faction faction, CRIME_STATUS status) {
            List<CrimeData> dataList = null;
            for (int i = 0; i < activeCrimes.Count; i++) {
                CrimeData data = activeCrimes[i];
                if(data.crimeStatus == status) {
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
                if(data.crime == crime) {
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
        private void CharacterApprehension() {
            bool hasCreatedPersonalApprehend = false;
            if (owner.marker) {
                for (int i = 0; i < owner.marker.inVisionCharacters.Count; i++) {
                    Character inVision = owner.marker.inVisionCharacters[i];
                    if (inVision.relationshipContainer.IsFriendsWith(owner)) {
                        if (!HasCharacterThatIsAlreadyWorried(inVision)) {
                            AddCharacterThatIsAlreadyWorried(inVision);
                            inVision.interruptComponent.TriggerInterrupt(INTERRUPT.Worried, owner);
                        }
                    } else {
                        if (!hasCreatedPersonalApprehend) {
                            bool canDoJob = false;
                            hasCreatedPersonalApprehend = inVision.jobComponent.TryCreateApprehend(owner, ref canDoJob);
                            if (!canDoJob) {
                                inVision.combatComponent.Flight(owner, "fleeing crime scene");
                            }
                        } else {
                            inVision.combatComponent.Flight(owner, "fleeing crime scene");
                        }
                    }
                }
            }
            if (!hasCreatedPersonalApprehend) {
                if (owner.currentSettlement != null && owner.currentSettlement is NPCSettlement settlement && (settlement.locationType == LOCATION_TYPE.VILLAGE)) {
                    settlement.settlementJobTriggerComponent.TryCreateApprehend(owner);
                }
            }
        }
        #endregion
    }
}

#region Save Data
public class SaveDataCriminal : SaveDataTrait {
    public List<string> activeCrimeIDs;
    public List<string> previousCrimeIDs;
    public List<string> alreadyWorriedCharacterIDs;
    public bool isImprisoned;
    public override void Save(Trait trait) {
        base.Save(trait);
        Criminal criminal = trait as Criminal;
        Assert.IsNotNull(criminal);
        activeCrimeIDs = new List<string>();
        for (int i = 0; i < criminal.activeCrimes.Count; i++) {
            CrimeData activeCrime = criminal.activeCrimes[i];
            activeCrimeIDs.Add(activeCrime.persistentID);
            SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(activeCrime);
        }
        
        previousCrimeIDs = new List<string>();
        for (int i = 0; i < criminal.previousCrimes.Count; i++) {
            CrimeData previousCrimes = criminal.previousCrimes[i];
            previousCrimeIDs.Add(previousCrimes.persistentID);
            SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(previousCrimes);
        }

        alreadyWorriedCharacterIDs = SaveUtilities.ConvertSavableListToIDs(criminal.charactersThatAreAlreadyWorried);
        isImprisoned = criminal.isImprisoned;
    }
}
#endregion

