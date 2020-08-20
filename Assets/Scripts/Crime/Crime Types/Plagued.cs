using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Crime_System {
    public class Plagued : CrimeType {
        public Plagued() : base(CRIME_TYPE.Plagued) { }

        #region Overrides
        public override string GetLastStrawReason(Character witness, Character actor, IPointOfInterest target, ICrimeable crime) {
            return actor.name + " is plagued";
        }
        #endregion
    }
}