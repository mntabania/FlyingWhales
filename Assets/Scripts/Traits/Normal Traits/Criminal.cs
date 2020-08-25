using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Interrupts;

namespace Traits {
    public class Criminal : Status {

        public List<CrimeData> activeCrimes { get; protected set; }
        public List<CrimeData> previousCrimes { get; protected set; }
        public Character owner { get; private set; }
        public List<Character> charactersThatAreAlreadyWorried { get; private set; }
        public bool isImprisoned { get; private set; }

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

        #region Overrides
        public override void OnAddTrait(ITraitable sourcePOI) {
            base.OnAddTrait(sourcePOI);
            if (sourcePOI is Character) {
                owner = sourcePOI as Character;
                //TODO: sourceCharacter.homeNpcSettlement.jobQueue.UnassignAllJobsTakenBy(sourceCharacter);
                owner.CancelOrUnassignRemoveTraitRelatedJobs();
                //CharacterApprehension();
            }

        }
        public override void OnRemoveTrait(ITraitable sourcePOI, Character removedBy) {
            owner.ForceCancelAllJobsTargettingThisCharacter(JOB_TYPE.APPREHEND);
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
            newData.SetCriminalTrait(criminalTrait);
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
        public List<CrimeData> GetListOfUnpunishedCrimesWantedBy(Faction faction) {
            List<CrimeData> dataList = null;
            for (int i = 0; i < activeCrimes.Count; i++) {
                CrimeData data = activeCrimes[i];
                if(data.crimeStatus == CRIME_STATUS.Unpunished) {
                    if (data.IsWantedBy(faction)) {
                        if (dataList == null) { dataList = new List<CrimeData>(); }
                        dataList.Add(data);
                    }
                }
            }
            return dataList;
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
                if (owner.currentSettlement != null && owner.currentSettlement is NPCSettlement settlement && (settlement.locationType == LOCATION_TYPE.SETTLEMENT)) {
                    settlement.settlementJobTriggerComponent.TryCreateApprehend(owner);
                }
            }
        }
        #endregion
    }
}

