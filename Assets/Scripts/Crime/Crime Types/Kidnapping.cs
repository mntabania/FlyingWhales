using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Crime_System {
    public class Kidnapping : CrimeType {
        public Kidnapping() : base(CRIME_TYPE.Kidnapping) { }

        #region Overrides
        public override string GetLastStrawReason(Character witness, Character actor, IPointOfInterest target, ICrimeable crime) {
            return actor.name + " is a kidnapper";
        }
        #endregion
    }
}