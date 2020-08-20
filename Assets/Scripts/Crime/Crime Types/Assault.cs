using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Crime_System {
    public class Assault : CrimeType {
        public Assault() : base(CRIME_TYPE.Assault) { }

        #region Overrides
        public override string GetLastStrawReason(Character witness, Character actor, IPointOfInterest target, ICrimeable crime) {
            return actor.name + " is violent";
        }
        #endregion
    }
}