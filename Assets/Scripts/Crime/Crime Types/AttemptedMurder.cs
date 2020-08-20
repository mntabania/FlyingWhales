using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Crime_System {
    public class AttemptedMurder : CrimeType {
        public AttemptedMurder() : base(CRIME_TYPE.Attempted_Murder) { }

        #region Overrides
        public override string GetLastStrawReason(Character witness, Character actor, IPointOfInterest target, ICrimeable crime) {
            return actor.name + " almost killed somebody";
        }
        #endregion
    }
}