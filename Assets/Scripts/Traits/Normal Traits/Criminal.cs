using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Interrupts;

namespace Traits {
    public class Criminal : Status {

        public CrimeData crimeData { get; protected set; }
        public Character owner { get; private set; }
        public List<Character> charactersThatAreAlreadyWorried { get; private set; }

        public Criminal() {
            name = "Criminal";
            description = "This character has been branded as a criminal by his/her own faction.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = 0;
            charactersThatAreAlreadyWorried = new List<Character>();
        }

        #region Overrides
        public override void OnAddTrait(ITraitable sourcePOI) {
            base.OnAddTrait(sourcePOI);
            if (sourcePOI is Character) {
                owner = sourcePOI as Character;
                //TODO: sourceCharacter.homeNpcSettlement.jobQueue.UnassignAllJobsTakenBy(sourceCharacter);
                owner.CancelOrUnassignRemoveTraitRelatedJobs();
                CharacterApprehension();

                if (owner.isSettlementRuler) {
                    owner.ruledSettlement.SetRuler(null);
                    Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "no_longer_settlement_ruler");
                    log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    owner.logComponent.RegisterLog(log, onlyClickedCharacter: false);
                }

                if (owner.isFactionLeader) {
                    owner.faction.SetLeader(null);
                    Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "no_longer_faction_leader");
                    log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    owner.logComponent.RegisterLog(log, onlyClickedCharacter: false);
                }
            }

        }
        public override void OnRemoveTrait(ITraitable sourcePOI, Character removedBy) {
            owner.ForceCancelAllJobsTargettingThisCharacter(JOB_TYPE.APPREHEND);
            base.OnRemoveTrait(sourcePOI, removedBy);
        }
        public override string GetNameInUI(ITraitable traitable) {
            //if(crimeData != null) {
            //    return $"{name}:{crimeData.strCrimeType}";
            //}
            return name;
        }
        #endregion

        #region General
        public void AddCharacterThatIsAlreadyWorried(Character character) {
            charactersThatAreAlreadyWorried.Add(character);
        }
        public bool HasCharacterThatIsAlreadyWorried(Character character) {
            return charactersThatAreAlreadyWorried.Contains(character);
        }
        public void SetCrime(CRIME_TYPE crimeType, ICrimeable crime, IPointOfInterest crimeTarget) {
            if(crimeData != null) {
                Debug.LogError(
                    $"Cannot set crime to criminal {owner.name} because it already has a crime: {crimeData.crimeType}");
                return;
            }
            crimeData = new CrimeData(crimeType, crime, owner, crimeTarget);
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

