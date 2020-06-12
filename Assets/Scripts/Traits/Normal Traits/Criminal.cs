using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Interrupts;

namespace Traits {
    public class Criminal : Status {

        public CrimeData crimeData { get; protected set; }
        public Character owner { get; private set; }

        public Criminal() {
            name = "Criminal";
            description = "This character has been branded as a criminal by his/her own faction.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = 0;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable sourcePOI) {
            base.OnAddTrait(sourcePOI);
            if (sourcePOI is Character) {
                owner = sourcePOI as Character;
                //TODO: sourceCharacter.homeNpcSettlement.jobQueue.UnassignAllJobsTakenBy(sourceCharacter);
                owner.CancelOrUnassignRemoveTraitRelatedJobs();
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
        public void SetCrime(CRIME_TYPE crimeType, ICrimeable crime, IPointOfInterest crimeTarget) {
            if(crimeData != null) {
                Debug.LogError(
                    $"Cannot set crime to criminal {owner.name} because it already has a crime: {crimeData.crimeType}");
                return;
            }
            crimeData = new CrimeData(crimeType, crime, owner, crimeTarget);
        }
        #endregion
    }
}

