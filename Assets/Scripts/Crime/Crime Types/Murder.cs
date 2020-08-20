using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Crime_System {
    public class Murder : CrimeType {
        public Murder() : base(CRIME_TYPE.Murder) { }

        #region Overrides
        public override string GetLastStrawReason(Character witness, Character actor, IPointOfInterest target, ICrimeable crime) {
            return actor.name + " is a murderer";
        }
        #endregion
    }
}