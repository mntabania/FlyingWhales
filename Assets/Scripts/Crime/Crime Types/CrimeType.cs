using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Crime_System {
    public class CrimeType {
        public CRIME_TYPE type { get; private set; }
        public string name { get; private set; }

        public CrimeType(CRIME_TYPE type) {
            this.type = type;
            name = UtilityScripts.Utilities.NotNormalizedConversionEnumToString(type.ToString());
        }

        #region Virtuals
        public virtual CRIME_SEVERITY GetCrimeSeverity(Character witness, Character actor, IPointOfInterest target, ICrimeable crime) {
            return CRIME_SEVERITY.Unapplicable;
        }
        public virtual string GetLastStrawReason(Character witness, Character actor, IPointOfInterest target, ICrimeable crime) { return string.Empty; }
        #endregion
    }
}