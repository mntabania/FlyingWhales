using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Crime_System {
    public class Treason : CrimeType {
        public Treason() : base(CRIME_TYPE.Treason) { }

        #region Overrides
        public override string GetLastStrawReason(Character witness, Character actor, IPointOfInterest target, ICrimeable crime) {
            return actor.name + " is treasonous";
        }
        #endregion
    }
}