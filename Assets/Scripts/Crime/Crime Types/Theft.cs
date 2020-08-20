using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Crime_System {
    public class Theft : CrimeType {
        public Theft() : base(CRIME_TYPE.Theft) { }

        #region Overrides
        public override string GetLastStrawReason(Character witness, Character actor, IPointOfInterest target, ICrimeable crime) {
            return actor.name + " is a thief";
        }
        #endregion
    }
}