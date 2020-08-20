using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Crime_System {
    public class CrimeSeverity {
        public string name { get; private set; }
        public CRIME_SEVERITY severity { get; private set; }

        public CrimeSeverity(CRIME_SEVERITY severity) {
            this.severity = severity;
            name = UtilityScripts.Utilities.NotNormalizedConversionEnumToString(severity.ToString());
        }
        public string EffectAndReaction(Character witness, Character actor, IPointOfInterest target, CrimeType crimeType, ICrimeable crime, REACTION_STATUS reactionStatus) {
            Effect(witness, actor, target, crimeType, crime, reactionStatus);
            return Reaction(witness, actor, target, crimeType, crime, reactionStatus);
        }


        #region Virtuals
        public virtual void Effect(Character witness, Character actor, IPointOfInterest target, CrimeType crimeType, ICrimeable crime, REACTION_STATUS reactionStatus) { }
        public virtual string Reaction(Character witness, Character actor, IPointOfInterest target, CrimeType crimeType, ICrimeable crime, REACTION_STATUS reactionStatus) { return string.Empty; }
        #endregion
    }
}