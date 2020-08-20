using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

namespace Crime_System {
    public class Misdemeanor : CrimeSeverity {
        public Misdemeanor() : base(CRIME_SEVERITY.Misdemeanor) { }

        #region Overrides
        public override void Effect(Character witness, Character actor, IPointOfInterest target, CrimeType crimeType, ICrimeable crime, REACTION_STATUS reactionStatus) {
            witness.relationshipContainer.AdjustOpinion(witness, actor, name + ": " + crimeType.name, -10, crimeType.GetLastStrawReason(witness, actor, target, crime));
        }
        #endregion
    }
}
