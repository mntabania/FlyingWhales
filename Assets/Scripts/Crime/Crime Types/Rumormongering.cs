using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Crime_System {
    public class Rumormongering : CrimeType {

        public Rumormongering() : base(CRIME_TYPE.Rumormongering) { }

        #region Overrides
        public override string GetLastStrawReason(Character witness, Character actor, IPointOfInterest target, ICrimeable crime) {
            return actor.name + " spreads rumors";
        }
        #endregion
    }
}