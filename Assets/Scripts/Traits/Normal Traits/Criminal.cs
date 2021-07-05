using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Interrupts;
using Traits;
using UnityEngine.Assertions;
namespace Traits {
    public class Criminal : Status {
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
        }

        #region Loading
        public override void LoadSecondWaveInstancedTrait(SaveDataTrait p_saveDataTrait) {
            base.LoadSecondWaveInstancedTrait(p_saveDataTrait);
            SaveDataCriminal saveDataCriminal = p_saveDataTrait as SaveDataCriminal;
            Assert.IsNotNull(saveDataCriminal);
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
            owner.crimeComponent.RemoveAllActiveCrimes(false);
            base.OnRemoveTrait(sourcePOI, removedBy);
        }
        protected override string GetDescriptionInUI() {
            string desc = base.GetDescriptionInUI();
            if(owner.crimeComponent.activeCrimes.Count > 0) {
                desc += "\n\nACTIVE CRIMES";
                for (int i = 0; i < owner.crimeComponent.activeCrimes.Count; i++) {
                    CrimeData data = owner.crimeComponent.activeCrimes[i];
                    desc += "\n" + data.GetCrimeDataDescription();
                }
            }
            if (owner.crimeComponent.previousCrimes.Count > 0) {
                desc += "\n\nPREVIOUS CRIMES";
                for (int i = 0; i < owner.crimeComponent.previousCrimes.Count; i++) {
                    CrimeData data = owner.crimeComponent.previousCrimes[i];
                    desc += "\n" + data.GetCrimeDataDescription();
                }
            }
            return desc;
        }
        public override void OnCopyStatus(Status statusToCopy, ITraitable from, ITraitable to) {
            base.OnCopyStatus(statusToCopy, from, to);
            if (statusToCopy is Criminal status) {
                isImprisoned = status.isImprisoned;
                charactersThatAreAlreadyWorried.AddRange(status.charactersThatAreAlreadyWorried);
            }
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
    public List<string> alreadyWorriedCharacterIDs;
    public bool isImprisoned;
    public override void Save(Trait trait) {
        base.Save(trait);
        Criminal criminal = trait as Criminal;
        Assert.IsNotNull(criminal);

        alreadyWorriedCharacterIDs = SaveUtilities.ConvertSavableListToIDs(criminal.charactersThatAreAlreadyWorried);
        isImprisoned = criminal.isImprisoned;
    }
}
#endregion

