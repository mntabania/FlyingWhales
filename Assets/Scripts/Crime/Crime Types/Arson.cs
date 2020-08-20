using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Crime_System {
    public class Arson : CrimeType {
        public Arson() : base(CRIME_TYPE.Arson) { }

        #region Overrides
        public override string GetLastStrawReason(Character witness, Character actor, IPointOfInterest target, ICrimeable crime) {
            return actor.name + " is an arsonist";
        }
        #endregion
    }
}