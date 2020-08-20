using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Crime_System {
    public class Infidelity : CrimeType {
        public Infidelity() : base(CRIME_TYPE.Infidelity) { }

        #region Overrides
        public override string GetLastStrawReason(Character witness, Character actor, IPointOfInterest target, ICrimeable crime) {
            return actor.name + " is unfaithful";
        }
        #endregion
    }
}