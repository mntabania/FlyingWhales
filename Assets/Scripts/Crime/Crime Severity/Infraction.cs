using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

namespace Crime_System {
    public class Infraction : CrimeSeverity {
        public Infraction() : base(CRIME_SEVERITY.Infraction) { }

        #region Overrides
        public override void Effect(Character witness, Character actor, IPointOfInterest target, CrimeType crimeType, ICrimeable crime, REACTION_STATUS reactionStatus) {
            witness.relationshipContainer.AdjustOpinion(witness, actor, name + ": " + crimeType.name, -5, crimeType.GetLastStrawReason(witness, actor, target, crime));
        }
        #endregion
    }
}
